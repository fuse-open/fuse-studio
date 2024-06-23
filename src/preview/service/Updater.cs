using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using Outracks;
using Outracks.IO;
using Outracks.Simulator;
using Outracks.Simulator.Bytecode;
using Outracks.Simulator.CodeGeneration;
using Outracks.Simulator.Protocol;
using Outracks.Simulator.Runtime;
using Outracks.Simulator.UXIL;
using Uno.UX.Markup.UXIL;

namespace Fuse.Preview
{
	public class Updater
	{
		readonly Reifier _reifier;

		public Updater(Reifier reifier)
		{
			_reifier = reifier;
		}

		public IObservable<IBinaryMessage> Update(IObservable<UpdateAttribute> args)
		{
			return _reifier.Result.Switch(maybeBuild =>
				maybeBuild.MatchWith(
					none: () => args.Select(arg => (IBinaryMessage)new ReifyRequired()),
					some: m =>
					{
						var objectIdentifiers = ProjectObjectIdentifiers.Create(m.Project, m.Build.TypeInfo,
							onError: e => ReportFactory.FallbackReport.Exception("Failed to create identifiers for document", e));

						var valueParser = new ValueParser(m.Build.TypeInfo, m.Build.Project);

						var context = new Context(
							new UniqueNames(), null, null,
							typesDeclaredInUx: m.Project
								.AllNodesInProject()
								.OfType<ClassNode>()
								.Where(c => c.Simulate)
								.Select(c => c.GetTypeName())
								.ToImmutableHashSet());

						return args
							.SelectMany(p =>
							{
								if (p.Value.HasValue && ContainsUXExpression(p.Value.Value))
									return new IBinaryMessage[] { new ReifyRequired() };

								if (p.Property.StartsWith("ux:", StringComparison.InvariantCultureIgnoreCase))
									return new IBinaryMessage[] { new ReifyRequired() };

								if (p.Property.Contains("."))
									return new IBinaryMessage[] { new ReifyRequired() };

								if (p.Property == "Layer")
									return new IBinaryMessage[] { new ReifyRequired() };

								var node = objectIdentifiers.TryGetNode(p.Object);

								if (!node.HasValue)
									return new IBinaryMessage[] { new ReifyRequired() };

								// If this attribute contained an UX expression last reify,
								// we must require reify to remove old binding objects.
								var currentValue = node.Value.RawProperties
									.Where(x => x.Name == p.Property)
									.Select(x => x.Value)
									.FirstOrDefault();
								if (ContainsUXExpression(currentValue))
									return new IBinaryMessage[] { new ReifyRequired() };

								try
								{
									var message = Execute(p.Object, instance =>
										new SingleProperties(node.Value, context, instance)
											.UpdateValue(p.Property, p.Value, valueParser));

									return new[]
									{
										new Started { Command = p },
										message,
										new Ended { Command = p, Success = true, BuildDirectory = AbsoluteFilePath.Parse(m.Build.Assembly).ContainingDirectory }
									};
								}
								catch (Exception)
								{
									return new IBinaryMessage[] { new ReifyRequired()  };
								}
							});
					}));
		}

		public static IBinaryMessage ExecuteMultiple(ObjectIdentifier id, Func<Expression, Statement[]> objectTransform)
		{
			return new BytecodeUpdated(
				new Lambda(
					Signature.Action(),
					Enumerable.Empty<BindVariable>(),
					new[] { ExecuteStatementMultiple(id, objectTransform) }));
		}

		static Statement ExecuteStatementMultiple(ObjectIdentifier id, Func<Expression, Statement[]> objectTransform)
		{
			return new CallStaticMethod(
				TryExecuteOnObjectsWithTag,
				new StringLiteral(id.ToString()),
				new Lambda(
					Signature.Action(Variable.This),
					Enumerable.Empty<BindVariable>(),
					objectTransform(new ReadVariable(Variable.This))));
		}

		public static IBinaryMessage Execute(ObjectIdentifier id, Func<Expression, Statement> objectTransform)
		{
			return new BytecodeUpdated(
				new Lambda(
					Signature.Action(),
					Enumerable.Empty<BindVariable>(),
					new[] { ExecuteStatement(id, objectTransform) }));
		}

		static Statement ExecuteStatement(ObjectIdentifier id, Func<Expression, Statement> objectTransform)
		{
			return new CallStaticMethod(
				TryExecuteOnObjectsWithTag,
				new StringLiteral(id.ToString()),
				new Lambda(
					Signature.Action(Variable.This),
					Enumerable.Empty<BindVariable>(),
					new[]
				{
					objectTransform(new ReadVariable(Variable.This))
				}));
		}

		static StaticMemberName TryExecuteOnObjectsWithTag
		{
			get
			{
				return new StaticMemberName(
					TypeName.Parse(typeof(ObjectTagRegistry).FullName),
					new TypeMemberName("TryExecuteOnObjectsWithTag"));
			}
		}

		// This methods checks if "str" contains an unescaped '{' followed by an unescaped '}'
		static bool ContainsUXExpression(string str)
		{
			if (str == null) return false;

			for (int i = 0; i < str.Length; i++)
			{
				switch (str[i])
				{
					case '\\':
						i++;
						continue;
					case '{':
						for (int j = i + 1; j < str.Length; j++)
						{
							switch (str[j])
							{
								case '\\':
									j++;
									continue;
								case '}':
									return true;
							}
						}

						break;
				}
			}

			return false;
		}
	}
}
