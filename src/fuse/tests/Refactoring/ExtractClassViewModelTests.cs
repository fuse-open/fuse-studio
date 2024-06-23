using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Outracks.Fuse.Refactoring;
using Outracks.Fuse.Tests;
using Outracks.IO;

namespace Outracks.Fuse.Protocol.Tests.Refactoring
{
	// These tests should finish pretty fast as they involve no IO or CPU intensive work,
	// this timeout attribute is here to avoid waiting forever due to some async mistake
	[Timeout(3000)]
	[TestFixture]
	public class ExtractClassViewModelTests
	{
		IExtractClassViewModel _model;
		BehaviorSubject<IElement> _currentSelection;
		IElement _root;
		MockedClassExtractor _classExtractor;
		IFileSystem _fileSystem;

		[SetUp]
		public async Task SetUp()
		{
			var project = Substitute.For<IProject>();
			project.Classes.Returns(Observable.Return(new IElement[] { }));
			project.RootDirectory.Returns(Observable.Return(AbsoluteDirectoryPath.Parse("/project")));

			var context = Substitute.For<IContext>();
			_root = CreateTree();

			_fileSystem = Substitute.For<IFileSystem>();

			_fileSystem.Exists(Arg.Any<AbsoluteFilePath>())
				.Returns(
					callInfo =>
					{
						var absoluteFilePath = callInfo.Arg<AbsoluteFilePath>();
						var result = absoluteFilePath == AbsoluteFilePath.Parse("/project/MainView.ux");
						Console.WriteLine("FileSystem.Exists({0}) -> {1}", absoluteFilePath, result);
						return result;
					});

			var panel = await GetTreeChild("Panel");
			_currentSelection = new BehaviorSubject<IElement>(panel);
			context.CurrentSelection.Returns(_currentSelection.Switch());
			_classExtractor = new MockedClassExtractor();
			_model = new ExtractClassViewModel(
				context: context,
				suggestedName: "MyPanel",
				allClassNames: Observable.Return(new HashSet<string>(new[] { "MyCircle" })),
				classExtractor: _classExtractor,
				fileSystem: _fileSystem,
				project: project);
			project.Classes.Returns(Observable.Return(new[] { await GetTreeChild("Circle") }));
		}

		[Test]
		public async Task ClassName_is_initially_set_to_name_suggestion_passed_to_constructor()
		{
			Assert.That(await _model.ClassName.FirstAsync(), Is.EqualTo("MyPanel"));
		}

		[Test]
		public async Task CreateCommand_is_disabled_while_ClassName_is_set_to_name_of_already_existing_class()
		{
			_model.ClassName.Write("MyCircle");
			Assert.That(await _model.CreateCommand.IsEnabled.FirstAsync(), Is.False);
		}

		[Test]
		public async Task CreateCommand_is_disabled_while_ClassName_contains_an_invalid_identifier()
		{
			// We don't allow spaces in the name
			_model.ClassName.Write("Super Duper");
			Assert.That(await _model.CreateCommand.IsEnabled.FirstAsync(), Is.False);
		}

		[Test]
		public async Task CreateCommand_is_disabled_while_ClassName_is_empty()
		{
			_model.ClassName.Write(string.Empty);
			Assert.That(await _model.CreateCommand.IsEnabled.FirstAsync(), Is.False);
		}

		[Test]
		public async Task CreateCommand_is_disabled_while_NewFileName_does_not_end_in_ux()
		{
			_model.CreateInNewFile.Write(true);
			_model.NewFileName.Write("Nisse.jpg");
			Assert.That(await _model.CreateCommand.IsEnabled.FirstAsync(), Is.False);
		}

		[Test]
		public async Task CreateCommand_is_disabled_while_NewFileName_is_not_a_valid_filename()
		{
			_model.CreateInNewFile.Write(true);
			_model.NewFileName.Write(">??#@@;;");
			Assert.That(await _model.CreateCommand.IsEnabled.FirstAsync(), Is.False);
		}

		[Test]
		public async Task CreateCommand_is_enabled_while_ClassName_is_valid_and_unique()
		{
			_model.ClassName.Write("SuperPanel");
			Assert.That(await _model.CreateCommand.IsEnabled.FirstAsync(), Is.True);
		}

		[Test]
		public async Task NewFileName_is_read_only_while_CreateInNewFile_is_false()
		{
			_model.CreateInNewFile.Write(false);
			Assert.That(await _model.NewFileName.IsReadOnly.FirstAsync(), Is.True);
		}

		[Test]
		public async Task NewFileName_is_writable_while_CreateInNewFile_is_true()
		{
			_model.CreateInNewFile.Write(true);
			Assert.That(await _model.NewFileName.IsReadOnly.FirstAsync(), Is.False);
		}

		[Test]
		public async Task CreateCommand_invokes_class_extractor_with_filename_set_to_none_if_CreateNewFile_is_false()
		{
			_model.CreateInNewFile.Write(false);
			_model.ClassName.Write("WeirdPanel");
			var action = await _model.CreateCommand.Action.FirstAsync();
			Assert.That(action.HasValue, Is.True);
			action.Value();
			Assert.That(_classExtractor.HasBeenCalled, Is.True);
			Assert.That(_classExtractor.ReceivedName, Is.EqualTo("WeirdPanel"));
			Assert.That(_classExtractor.ReceivedFileName.HasValue, Is.False);
			Assert.That(await _classExtractor.ReceivedElement.IsSameAs(_currentSelection.Value).FirstAsync(), Is.True);
		}

		[Test]
		public async Task Editing_class_name_suggests_filename()
		{
			_model.ClassName.Write("SomeName");
			Assert.That(await _model.NewFileName.FirstAsync(), Is.EqualTo("SomeName.ux"));

			_model.ClassName.Write("AnotherName");
			Assert.That(await _model.NewFileName.FirstAsync(), Is.EqualTo("AnotherName.ux"));
		}

		[Test]
		public async Task Editing_class_name_suggests_filename_in_folder_for_class_in_namespace()
		{
			_model.ClassName.Write("Lonely.Foo");
			Assert.That(await _model.NewFileName.FirstAsync(), Is.EqualTo("Lonely" + Path.DirectorySeparatorChar + "Foo.ux"));
		}

		[Test]
		public async Task Editing_class_name_after_editing_filename_does_not_overwrite_filename()
		{
			_model.NewFileName.Write("SomeFileName.ux");
			_model.ClassName.Write("SomeClassName");
			Assert.That(await _model.NewFileName.FirstAsync(), Is.EqualTo("SomeFileName.ux"));
		}

		[Test]
		public async Task Create_command_disabled_if_file_exist()
		{
			bool? isEnabled = null;
			string userInfo = null;

			using (_model.CreateCommand.IsEnabled.Subscribe(v => isEnabled = v))
			using (_model.UserInfo.Subscribe(v => userInfo = v))
			{
				Assert.That(isEnabled, Is.EqualTo(true));
				_model.ClassName.Write("MainView");
				_model.CreateInNewFile.Write(true);
				Assert.That(await _model.NewFileName.FirstAsync(), Is.EqualTo("MainView.ux"));
				Assert.That(isEnabled, Is.EqualTo(false));
				Assert.That(userInfo, Is.EqualTo("The file already exists"));
			}
		}

		class MockedClassExtractor : IClassExtractor
		{
			public Task ExtractClass(IElement element, string name, Optional<RelativeFilePath> fileName)
			{
				HasBeenCalled = true;
				ReceivedElement = element;
				ReceivedName = name;
				ReceivedFileName = fileName;
				return Task.FromResult(new object());
			}

			public IObservable<string> LogMessages
			{
				get { throw new NotImplementedException(); }
			}

			public bool HasBeenCalled { get; set; }

			public Optional<RelativeFilePath> ReceivedFileName { get; set; }

			public string ReceivedName { get; set; }

			public IElement ReceivedElement { get; private set; }
		}

		async Task<IElement> GetTreeChild(string name)
		{
			return await _root.Children.Where(x => x.Name.Is(name)).Select(x => x.First()).FirstAsync();
		}

		static IElement CreateTree()
		{
			return LiveElementFactory.CreateLiveElement("<App><Panel /><Circle ux:Class=\"MyCircle\" /></App>");
		}
	}
}