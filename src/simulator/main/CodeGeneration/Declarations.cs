using System.Collections.Generic;
using System.Linq;
using Outracks.Simulator.Bytecode;
using Outracks.Simulator.Runtime;
using Outracks.Simulator.UXIL;
using Uno.UX.Markup.UXIL;

namespace Outracks.Simulator.CodeGeneration
{
	static class Declarations
	{
		public static IEnumerable<BindVariable> GetDeclarations(this IEnumerable<Node> nodes, Context ctx)
		{
			return nodes.SelectMany(n => n.GetDeclaration(ctx));
		}

		static Optional<BindVariable> GetDeclaration(this Node n, Context ctx)
		{
			return from valueExpression in n.GetDeclarationExpression(ctx)
				select new BindVariable(ctx.Names[n], valueExpression);
		}

		static Optional<Expression> GetDeclarationExpression(this Node node, Context ctx)
		{
			return node.MatchWith<Optional<Expression>>(
				(DocumentScope ds) => ds.MatchWith(
					(ClassNode cn) => cn.GetClassExpression(ctx),
					(TemplateNode fn) => fn.GetTemplateExpression(ctx)),

				(ObjectNode n) => n.MatchWith(
					(BoxedValueNode bvn) =>
						bvn.Value.GetExpression(ctx),

					(NewObjectNode non) =>
						non.GetDeclarationExpression(ctx),

					(ResourceRefNode rfn) =>
						Expression.Throw(new InvalidUXIL("ResourceRefNodes do not need instantiation")),

					(NameTableNode ntn) =>
						ntn.GetDeclarationExpression(ctx)),
				(PropertyNode n) => Optional.None(),
                (DependencyNode n) => Optional.None());
		}

		static readonly TypeName NameTable = TypeName.Parse("Uno.UX.NameTable");

		static Expression GetDeclarationExpression(this NameTableNode ntn, Context ctx)
		{
			return new Instantiate(
				NameTable,
				new Expression[]
				{
					ntn.ParentTable == null
						? new StringLiteral(null)
						: ntn.ParentTable.GetExpression(ctx),

					CreateArray(
						TypeName.Parse("Uno.String"),
						ctx.Names,
						ntn.Entries.Select(e => new StringLiteral(e.Name)))
				});
		}

		static Expression CreateArray(TypeName elementType, UniqueNames names, IEnumerable<Expression> elements)
		{
			var list = names.GetUniqueName();
			names = names.Reserve(list);

			return new CallLambda(
				new Lambda(
					Signature.Func(TypeName.Parse("Uno.Object")),
					new[] { new BindVariable(list, new Instantiate(ObjectList.Parameterize(elementType))), },
					elements
						.Select(e => (Statement)new CallDynamicMethod(new ReadVariable(list), new TypeMemberName("Add"), e))
						.Concat(new [] { new Return(new CallDynamicMethod(new ReadVariable(list), new TypeMemberName("ToArray"))) })));
		}

		static readonly TypeName ObjectList = TypeName.Parse("Uno.Collections.List<Uno.Object>");

		static Expression GetDeclarationExpression(this NewObjectNode non, Context ctx)
		{
			var typeName = non.DataType.GetTypeName();
			var arguments = non.ConstructorArguments().GetExpressions(ctx);

			return Instantiate(typeName, arguments, ctx, ctx.TryGetTagHash(non));
		}

		public static Expression Instantiate(this TypeName typeName, Expression[] arguments, Context ctx, Optional<ObjectIdentifier> tagHash)
		{
			var obj = ctx.TryGetUxConstructorFunction(typeName).MatchWith(
				some: ex => (Expression)new CallLambda(ex, arguments),
				none: () =>  (Expression)new Instantiate(typeName, arguments));

			if (!tagHash.HasValue)
				return obj;

			return ExpressionConverter.BytecodeFromSimpleLambda(() =>
				ObjectTagRegistry.RegisterObjectTag(obj, tagHash.Value.ToString()));
		}

		// UXIL extensions:
		public static Variable GetUxConstructorFunctionName(this ClassNode classNode)
		{
			return GetUxConstructorFunctionName(TypeName.Parse(classNode.GetGeneratedClassQualifiedName()));
		}
		public static Variable GetUxConstructorFunctionName(this TypeName typeName)
		{
			return new Variable("new_" + typeName.FullName);
		}
		public static string GetGeneratedClassQualifiedName(this ClassNode node)
		{
			if (node.IsInnerClass) return GetGeneratedClassQualifiedName(GetParentClass(node)) + "." + node.GeneratedClassName.FullName;
			return node.GeneratedClassName.FullName;
		}

		public static ClassNode GetParentClass(this Node node)
		{
			var p = node.ParentScope;
			while (p != null && !(p is ClassNode))
			{
				p = p.ParentScope;
			}
			return p as ClassNode;
		}
	}
}