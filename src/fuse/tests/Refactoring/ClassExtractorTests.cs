using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Outracks.Fuse.Live;
using Outracks.Fuse.Refactoring;
using Outracks.IO;
using Outracks.Simulator;

namespace Outracks.Fuse.Protocol.Tests.Refactoring
{
	// These tests should finish pretty fast as they involve no IO or CPU intensive work,
	// this timeout attribute is here to avoid waiting forever due to some async mistake
	[Timeout(3000)]
	[TestFixture]
	public class ClassExtractorTests
	{
		IElement _root;

		[Test]
		public async Task ExtractClass_turns_element_into_a_class_and_inserts_an_instance_of_that_class_beside_it()
		{
			_root = CreateRootElement();
			var classExtractor = new ClassExtractor(Substitute.For<IProject>());
			await classExtractor.ExtractClass(await GetTreeChild("Panel"), "CustomPanel", Optional.None());
			var uxClassAttribute = await (await GetTreeChild("Panel")).UxClass().FirstAsync();
			Assert.That(uxClassAttribute, Is.EqualTo(Optional.Some("CustomPanel")));
			Assert.That(await GetTreeChild("CustomPanel"), Is.Not.Null);
		}

		[Test]
		public async Task ExtractClass_prints_message_to_logview_when_exception_is_thrown()
		{
			_root = CreateRootElement();
			var projectClasses = new BehaviorSubject<IEnumerable<IElement>>(new IElement[] { });
			var project = Substitute.For<IProject>();

			project.Classes.ReturnsForAnyArgs(projectClasses);

			project
				.CreateDocument(default(RelativeFilePath))
				.ReturnsForAnyArgs(_ =>
					Task.Run(() => { throw new IOException("Couldn't write a file, as the disk just imploded"); }));

			var classExtractor = new ClassExtractor(project);
			string logMessage = null;
			using (classExtractor.LogMessages.Subscribe(message => logMessage = message))
			{
				await classExtractor.ExtractClass(await GetTreeChild("Panel"), "MyPanel", RelativeFilePath.Parse("CustomPanel.ux"));
			}

			Assert.That(logMessage, Is.EqualTo("Error: Unable to create class. Couldn't write a file, as the disk just imploded\r\n"));
		}

		[Test]
		public async Task ExtractClass_with_filename_set_copies_element_to_new_file_and_inserts_ux_class_attribute()
		{
			// Setup
			_root = CreateRootElement();
			var projectClasses = new BehaviorSubject<IEnumerable<IElement>>(new IElement[] {});
			var project = Substitute.For<IProject>();
			IElement createdFileRoot = null;

			project.Classes.ReturnsForAnyArgs(projectClasses);

			project
				.CreateDocument(default(RelativeFilePath))
				.ReturnsForAnyArgs(
					ci =>
					{
						var fragment = ci.Arg<SourceFragment>();
						var nativeRelativePath = ci.Arg<RelativeFilePath>().NativeRelativePath;
						createdFileRoot = CreateRootElement(fragment, nativeRelativePath);
						projectClasses.OnNext(new[] { createdFileRoot });
						return Task.FromResult(false); // no idea what i'm doing
					});
			var classExtractor = new ClassExtractor(project);

			// Do
			await classExtractor.ExtractClass(await GetTreeChild("Panel"), "MyPanel", RelativeFilePath.Parse("CustomPanel.ux"));

			// Assert
			Assert.That(createdFileRoot, Is.Not.Null);
			Assert.That(await createdFileRoot.Name.FirstAsync(), Is.EqualTo("Panel"));
			Assert.That(await createdFileRoot.UxClass().FirstAsync(), Is.EqualTo(Optional.Some("MyPanel")));
			Assert.That(await GetTreeChild("MyPanel"), Is.Not.Null);
		}

		async System.Threading.Tasks.Task<IElement> GetTreeChild(string name)
		{
			return await _root.Children.Where(x => x.Name.Is(name)).Select(x => x.First()).FirstAsync();
		}

		IElement CreateRootElement()
		{
			return CreateRootElement(SourceFragment.FromString("<App><Panel /><Circle ux:Class=\"MyCircle\" /></App>"), "MainView.ux");
		}

		static IElement CreateRootElement(SourceFragment fragment, string filename)
		{
			var parent = new LiveElement(
				AbsoluteFilePath.Parse("/project/" + filename),
				Observable.Never<ILookup<ObjectIdentifier, ObjectIdentifier>>(),
				Observable.Return(true),
				new Subject<Unit>(),
				Observer.Create<IBinaryMessage>(msg => { }),
				s => Element.Empty);

			// Panel is an example of an instance
			// Circle is an example of a class (called MyClass)
			parent.UpdateFrom(fragment.ToXml());

			return parent;
		}
	}
}