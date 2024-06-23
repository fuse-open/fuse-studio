using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Fuse.Preview;
using Outracks.IO;
using Outracks.Simulator;
using Outracks.Simulator.Protocol;
using Uno.ProjectFormat;

namespace Outracks.Fuse.Live
{
	public class LiveProject : IProject, IProjectLike
	{
		public LiveProject(Fusion.IDocument fusionDocument, IFileSystem shell, IObservable<BytecodeGenerated> bytecodeGenerated, IScheduler scheduler = null)
		{
			scheduler = scheduler ?? Scheduler.Default;
			_shell = shell;

			var mutations = new Subject<IBinaryMessage>();

			var idToElement = new BehaviorSubject<Dictionary<ObjectIdentifier, IElement>>(new Dictionary<ObjectIdentifier, IElement>());

			var metadata = bytecodeGenerated
				.Select(bc => bc.Bytecode.Metadata.ElementTypeHierarchy.ToLookup())
				.StartWith(Lookup.Empty<ObjectIdentifier, ObjectIdentifier>())
				.Replay(1);

			_idToElement = idToElement;
			metadata.Connect();

			bytecodeGenerated.Subscribe(bc =>
			{
				var elements = idToElement.Value;

				foreach (var type in bc.Bytecode.Metadata.PrecompiledElements)
				{
					IElement element;
					if (!elements.TryGetValue(type.Id, out element))
						elements[type.Id] = element = new LiveElement(
							AbsoluteFilePath.Parse("N/A"),
							metadata,
							Observable.Return(true),
							new Subject<Unit>(),
							Observer.Create<IBinaryMessage>(_ => { }),
							getElement: GetElement);

					element.Replace(_ => Task.FromResult(SourceFragment.FromString(type.Source))).Wait();
				}

				idToElement.OnNext(elements);
			});

			var unoProj = ProjectWatcher.Create(fusionDocument, shell, scheduler);
			Name = unoProj.Name;
			PackageReferences = unoProj.PackageReferences;
			ProjectReferences = unoProj.ProjectReferences;
			var liveDocuments = unoProj.UxFiles
				.CachePerElement(e => LiveDocument.Open(e, shell, metadata, idToElement, mutations, scheduler), val => val.Dispose())
				.Select(ImmutableList.CreateRange)
				.Replay(1).RefCount();

			var filePath = fusionDocument.FilePath.NotNone();


			FilePath = filePath;
			RootDirectory = filePath.Select(f => f.ContainingDirectory);
			BuildOutputDirectory = unoProj.BuildOutputDirectory;

			_liveDocuments = liveDocuments;
			Mutations = mutations;

			BundleFiles = unoProj.BundleFiles;
			FuseJsFiles = unoProj.FuseJsFiles;

			Documents = liveDocuments.Select(d => ImmutableList.ToImmutableList(d.Cast<IDocument>()));

			var allElements = Documents
				.CachePerElement(doc => doc.Elements)
				.Select(docs => docs.ToObservableEnumerable()).Switch()
				.Select(e => e.Join().ToArray())
				.Replay(1).RefCount();

			var classes = allElements
				.Where(e => e.HasProperty("ux:Class"))
				.DistinctUntilSetChanged()
				.Replay(1);

			var globals = allElements
				.Where(e => e.HasProperty("ux:Global"))
				.DistinctUntilSetChanged()
				.Replay(1);

			Classes = classes;
			GlobalElements = globals;

			LogMessages = _liveDocuments.Switch(docs =>
				docs.Select(doc => doc.Errors.NotNone().Select(o => doc.SimulatorIdPrefix + ": " + o.Message))
					.Merge());

			_garbage = Disposable.Combine(
				classes.Connect(),
				globals.Connect(),
				Disposable.Create(() =>
					liveDocuments
						.FirstAsync()
						.Subscribe(documents => documents.Each(d => d.Dispose()))));

			var app = Documents
				.SelectPerElement(d => d.Root)
				.WherePerElement(e => e.Name.Is("App"))
				.Select(e => e.FirstOr(Element.Empty))
				.Switch();

			Context = new Context(app, GetElement);
		}

		readonly IDisposable _garbage;
		readonly IFileSystem _shell;
		readonly BehaviorSubject<Dictionary<ObjectIdentifier, IElement>> _idToElement;
		readonly IObservable<IImmutableList<LiveDocument>> _liveDocuments;

		public IObservable<IEnumerable<DocumentSource>> LatestSource
		{
			get
			{
				return _liveDocuments
					.SelectPerElement(liveDocument =>
						liveDocument.Source.Select(source => new DocumentSource
						{
							LiveDocument = liveDocument,
							FileContents = new UxFileContents
							{
								Contents = source.ToString(),
								Path = liveDocument.SimulatorIdPrefix
							},
						}))
					.ToObservableEnumerable()
					.Replay(1).RefCount();
			}
		}

		public IObservable<IImmutableList<IDocument>> Documents { get; private set; }
		public IObservable<IBinaryMessage> Mutations { get; private set; }


		public IContext Context { get; private set; }

		public IObservable<IImmutableSet<PackageReference>> PackageReferences { get; private set; }
		public IObservable<IImmutableSet<ProjectReference>> ProjectReferences { get; private set; }
		public IObservable<string> Name { get; private set; }

		public IObservable<AbsoluteFilePath> FilePath { get; private set; }
		public IObservable<AbsoluteDirectoryPath> RootDirectory { get; private set; }
		public IObservable<AbsoluteDirectoryPath> BuildOutputDirectory { get; private set; }

		public IObservable<IImmutableSet<AbsoluteFilePath>> BundleFiles { get; private set; }
		public IObservable<IImmutableSet<AbsoluteFilePath>> FuseJsFiles { get; private set; }

		public IObservable<IEnumerable<IElement>> GlobalElements { get; private set; }

		public IObservable<IEnumerable<IElement>> Classes { get; private set; }

		public IElement GetElement(ObjectIdentifier simulatorId)
		{
			return _idToElement.Value.TryGetValue(simulatorId).Or(Element.Empty);
		}

		/// <summary>
		/// Creates a new document in the project, and adds it to unoproj file.
		/// Also creates any missing directories in path.
		/// </summary>
		public async Task CreateDocument(RelativeFilePath relativePath, SourceFragment contents = null)
		{
			contents = contents ?? SourceFragment.Empty;

			var rootDir = await RootDirectory.FirstAsync();
			var newFilePath = rootDir.Combine(relativePath);

			var containingDir = newFilePath.ContainingDirectory;
			_shell.Create(containingDir);

			using (var stream = _shell.CreateNew(newFilePath))
			{
				var bytes = contents.ToBytes();
				stream.Write(bytes, 0, bytes.Length);
			}

			var projectFilePath = await FilePath.FirstAsync();
			var project = Project.Load(projectFilePath.NativePath);

			if (project.AllFiles.None(item => item.UnixPath == relativePath.NativeRelativePath))
			{
				project.MutableIncludeItems.Add(new IncludeItem(relativePath.NativeRelativePath));
				project.Save();
			}
		}
		public IObservable<string> LogMessages { get; private set; }

		public void Dispose()
		{
			_garbage.Dispose();
		}
	}

}