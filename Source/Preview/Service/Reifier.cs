using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Outracks;
using Outracks.IO;
using Outracks.Simulator;
using Outracks.Simulator.Bytecode;
using Outracks.Simulator.CodeGeneration;
using Outracks.Simulator.Parser;
using Outracks.Simulator.Protocol;
using Outracks.Simulator.UXIL;
using Uno.UX.Markup.UXIL;

namespace Fuse.Preview
{
	public class Reifier
	{
		readonly ISubject<Optional<ProjectMarkup>> _result = new ReplaySubject<Optional<ProjectMarkup>>(1);
		readonly Builder _builder;

		public Reifier(Builder builder)
		{
			_builder = builder;
		}

		public IObservable<Optional<ProjectMarkup>> Result
		{
			get { return _result; }
		}

		/// <remarks> This function will update Result as a side-effect </remarks>
		/// <returns> (RebuildRequired | (Started (LogEvent)* [BytecodeGenerated] Ended))* </returns>
		public IObservable<IBinaryMessage> Reify(IObservable<GenerateBytecode> args)
		{
			return _builder.Result.Switch(maybeBuild =>
				maybeBuild.MatchWith(
					none: () =>
					{
						// Can't do any reifying without a build 
						_result.OnNext(Optional.None());
						return args.Select(arg => 
							(IBinaryMessage)new RebuildRequired());
					},
					some: build =>
					{
						var markupParser = new MarkupParser(
							build,
							new UxParser(
								build.Project,
								new GhostCompilerFactory(build.TypeInfo)));
						
						// Let's answer those reify calls with some proper bytecode and build events
						return args
							.Select(a => 
								Observable.Create<IBinaryMessage>(observer =>
								{
									observer.OnNext(new Started { Command = a });

									var markup = markupParser.TryParseDocuments(a, observer, a.Id);
									_result.OnNext(markup);

									var bytecode = TryCompile(markup, observer, a.Id);
									if (bytecode.HasValue)
										observer.OnNext(new BytecodeGenerated(bytecode.Value));

									observer.OnNext(new Ended { Command = a, Success = bytecode.HasValue, BuildDirectory = AbsoluteFilePath.Parse(build.Assembly).ContainingDirectory });

									observer.OnCompleted();

									return Disposable.Empty;
								}))
							.Concat();
					}));

		}

		static Optional<ProjectBytecode> TryCompile(Optional<ProjectMarkup> markup, IObserver<IBinaryMessage> buildEvents, Guid buildId)
		{
			return markup.SelectMany(m => TryCompile(m, buildEvents, buildId));
		}

		static Optional<ProjectBytecode> TryCompile(ProjectMarkup markup, IObserver<IBinaryMessage> buildEvents, Guid buildId)
		{
			try
			{
				var projectDirectory = Path.GetDirectoryName(markup.Build.Project);
				var dependencies = 
					ImmutableList.ToImmutableList(
						markup.Project.RootClasses
							.SelectMany(node => ImportExpression.FindFiles(node, projectDirectory))
							.Distinct());

				var ids = ProjectObjectIdentifiers.Create(markup.Project, markup.Build.TypeInfo, 
					onError: e => ReportFactory.FallbackReport.Exception("Failed to create identifiers for document", e));
			
				var ctx = new Context(
					names: new UniqueNames(),
					tryGetTagHash: ids.TryGetIdentifier,
					projectDirectory: projectDirectory,
					typesDeclaredInUx: markup.Project
						.AllNodesInProject()
						.OfType<ClassNode>()
						.Select(c => c.GetTypeName())
						.ToImmutableHashSet());

				var reify = markup.Project.GenerateGlobalScopeConstructor(ctx);
				var metadata = markup.Project.GenerateMetadata(ids);
				return new ProjectBytecode(reify, dependencies, metadata);
			}
			catch (Exception e)
			{
				buildEvents.OnNext(new BuildIssueDetected(BuildIssueType.Error, "", e.Message, Optional.None(), buildId));
				return Optional.None();
			}
		}
	}
}