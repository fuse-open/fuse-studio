using System;
using System.Collections.Generic;
using System.Linq;
using Uno.UX.Markup.UXIL;

namespace Outracks.Simulator.CodeGeneration
{
	using Bytecode;

	public static class Scopes
	{
		public static Lambda GenerateScopeConstructor(this DocumentScope scope, TypeName simulatedType, TypeName producedType, Context ctx)
		{
			var nodesToDeclare = scope.NodesDeclaredInScope();
			var nodesToInitialize = scope.NodesToInitializeInScope();

			var usedNames = ctx.Names;
			var self = usedNames.GetUniqueName();
			var names = usedNames.Add(scope, self).GenerateNames(nodesToDeclare);
			var newCtx = ctx.With(names: names);

            var parameters = ImmutableList<Parameter>.Empty;

            var cs = scope as ClassNode;
            if (cs != null)
            {
                parameters = cs.DeclaredDependencies
                    .Select(x => new Parameter(TypeName.Parse(x.ResultingType.FullName), names[x]))
                    .ToImmutableList();
            }

			var isApp = parameters.Count == 0 && producedType == App;

			if (isApp)
				parameters = List.Create(new Parameter(App, Variable.This));

			var baseParameters = scope.Properties
                .Where(x => x.Facet.IsConstructorArgument && x.HasValue)
                .Select(x =>
                {
                    if (x is ReferenceProperty) return ((ReferenceProperty)x).Source.GetExpression(newCtx);
                    else if (x is AtomicProperty) return ((AtomicProperty)x).Value.GetExpression(newCtx);
                    else throw new Exception("Unsupported constructor argument property type");
                })
                .ToArray();
    
			return new Lambda(
				new Signature(
					parameters: parameters,
					returnType: producedType),
				localVariables:
                    new[] 
					{ 
						new BindVariable(self,
 							isApp 
							? new ReadVariable(Variable.This) // use object passed to ctor as this for the App tag
							: producedType.Instantiate(baseParameters, newCtx, newCtx.TryGetTagHash(scope))) 
					}
                    .Concat(nodesToDeclare.GetDeclarations(newCtx)),
				statements: 
					nodesToInitialize
						.GetInitializations(newCtx)
						.Concat(new [] { new Return(new ReadVariable(self)) }));
		}

		static TypeName App { get { return TypeName.Parse("Fuse.App"); } }

		public static Lambda GenerateGlobalScopeConstructor(this Project globalScope, Context ctx)
		{
			var nodesToDeclare = globalScope.NodesDeclaredInScope();
			var nodesToInitialize = globalScope.NodesToInitializeInScope();

			// Making a new name for the transformed application ctor that takes an App as parameter
			// This is so we can still keep the regular ctor for use in other ux files
			var mutatingAppCtorName = ctx.Names.GetUniqueName();
			var names = ctx.Names.Reserve(mutatingAppCtorName).GenerateNames(nodesToDeclare);
			var newCtx = ctx.With(names: names);

			var declarations = nodesToDeclare.GetDeclarations(newCtx).ToImmutableList();

			var appCtor = declarations.FirstOrDefault(decl => decl.IsAppConstructor());
			if (appCtor == null) 
				throw new MissingAppTag();
			
			return new Lambda(
				Signature.Action(Variable.This),
				localVariables:
					declarations,
				statements:
					nodesToInitialize.GetInitializations(newCtx)
						.Concat(nodesToDeclare.RegisterGlobalKeys(newCtx))
						.Concat(new [] { new CallLambda(new ReadVariable(appCtor.Variable), new ReadVariable(Variable.This)) }));
		}


		static IEnumerable<Statement> RegisterGlobalKeys(this IEnumerable<Node> nodes, Context ctx)
		{
			foreach (var node in nodes)
			{
				if (node.IsGlobalDeclaration())
				{
					yield return new CallStaticMethod(
						SetGlobalKey,
						node.GetExpression(ctx), 
						new StringLiteral(node.Name));
				}
			}
		}

		static StaticMemberName SetGlobalKey = new StaticMemberName(
			TypeName.Parse("Uno.UX.Resource"),
			new TypeMemberName("SetGlobalKey"));

	}

	static class UseSimulatorApp
	{
		public static bool IsAppConstructor(this BindVariable bind)
		{
			var lambda = bind.Expression as Lambda;
			return lambda != null && lambda.Signature.ReturnType == App;
		}

		static TypeName App { get { return TypeName.Parse("Fuse.App"); } }
	}

}



