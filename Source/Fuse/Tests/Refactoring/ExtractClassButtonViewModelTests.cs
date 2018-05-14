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

namespace Outracks.Fuse.Protocol.Tests.Refactoring
{
	// These tests should finish pretty fast as they involve no IO or CPU intensive work,
	// this timeout attribute is here to avoid waiting forever due to some async mistake
	[Timeout(3000)]
	[TestFixture]
	public class ExtractClassButtonViewModelTests
	{
		IExtractClassButtonViewModel _model;
		BehaviorSubject<IElement> _currentSelection;
		IElement _root;
		IElement _classElement;
		IElement _instanceElement;
		bool _buttonClicked;
		IExtractClassViewModel _resultingViewModel;
		IElement _javaScriptElement;
		IElement _includeElement;

		[SetUp]
		public async Task SetUp()
		{
			var project = Substitute.For<IProject>();
			project.Classes.Returns(Observable.Return(new IElement[] { }));
			var context = Substitute.For<IContext>();
			_currentSelection = new BehaviorSubject<IElement>(Element.Empty);
			_buttonClicked = false;
			context.CurrentSelection.Returns(_currentSelection.Switch());
			_model = new ExtractClassButtonViewModel(project, context,
				dialogViewModel =>
				{
					_buttonClicked = true;
					_resultingViewModel = dialogViewModel;
				}, new Shell(), Substitute.For<IClassExtractor>());
			_root = CreateTree("<App><JavaScript /><Include /><Panel /><Circle ux:Class=\"MyClass\" /></App>");
				
			_classElement = await GetTreeChild("Circle");
			_instanceElement = await GetTreeChild("Panel");
			_javaScriptElement = await GetTreeChild("JavaScript");
			_includeElement = await GetTreeChild("Include");
			project.Classes.Returns(Observable.Return(new[] { _classElement }));
		}

		[Test]
		public async Task Button_is_disabled_while_nothing_is_selected()
		{
			_currentSelection.OnNext(Element.Empty);
			await AssertButtonDisabled();
		}

		[Test]
		public async Task Button_is_disabled_while_selecting_element_that_is_already_a_class()
		{
			_currentSelection.OnNext(_classElement);
			await AssertButtonDisabled();
		}

		[Test]
		public async Task Button_is_disabled_while_selecting_root()
		{
			_currentSelection.OnNext(_root);
			await AssertButtonDisabled();
		}

		[Test]
		public async Task Button_is_disabled_while_javascript_selected()
		{
			_currentSelection.OnNext(_javaScriptElement);
			await AssertButtonDisabled();
		}

		[Test]
		[Ignore("Times out on AppVeyor for some reason")]
		public async Task Button_is_disabled_while_include_element_selected()
		{
			_currentSelection.OnNext(_includeElement);
			await AssertButtonDisabled();
		}

		[Test]
		public async Task Button_is_enabled_while_element_that_is_an_instance_is_selected()
		{
			_currentSelection.OnNext(_instanceElement);
			await AssertButtonEnabled();
		}

		[Test]
		public async Task HighlightSelectedElement_is_true_after_HoverEnter_is_called_while_instance_element_is_selected()
		{
			_currentSelection.OnNext(_instanceElement);
			_model.HoverEnter();
			Assert.That(await _model.HighlightSelectedElement.FirstAsync(), Is.True);
		}

		[Test]
		public async Task HighlightSelectedElement_is_false_after_HoverEnter_is_called_while_class_element_is_selected()
		{
			_currentSelection.OnNext(_classElement);
			_model.HoverEnter();
			Assert.That(await _model.HighlightSelectedElement.FirstAsync(), Is.False);
		}

		[Test]
		public async Task HighlightSelectedElement_is_cleared_after_HoverExit_is_called_while_instance_element_is_selected()
		{
			_currentSelection.OnNext(_instanceElement);
			_model.HoverEnter();
			Assert.That(await _model.HighlightSelectedElement.FirstAsync(), Is.True);
			_model.HoverExit();
			Assert.That(await _model.HighlightSelectedElement.FirstAsync(), Is.False);
		}

		[Test]
		public async Task Clicking_button_invokes_and_passes_dialog_view_model_to_callback()
		{
			_currentSelection.OnNext(_instanceElement);
			var action = (await _model.Command.Action.FirstAsync()).Value;
			action();
			Assert.That(_buttonClicked, Is.True);
			Assert.That(_resultingViewModel, Is.Not.Null);

			// Just do some initial assertions that the dialog is in a state we expect
			// (more testing of the dialog in another fixture)
			Assert.That(await _resultingViewModel.ClassName.FirstAsync(), Is.EqualTo("MyPanel"));
			Assert.That(await _resultingViewModel.CreateInNewFile.FirstAsync(), Is.False);
			Assert.That(await _resultingViewModel.CreateInNewFile.IsReadOnly.FirstAsync(), Is.False);
		}

		async Task<IElement> GetTreeChild(string name)
		{
			return await _root.Children.Where(x => x.Name.Is(name)).Select(x => x.First()).FirstAsync();
		}

		async Task AssertButtonDisabled()
		{
			Assert.That((await _model.Command.Action.FirstAsync()).HasValue, Is.False);
		}

		async Task AssertButtonEnabled()
		{
			Assert.That((await _model.Command.Action.FirstAsync()).HasValue, Is.True);
		}

		static IElement CreateTree(string uxSource)
		{
			var parent = new LiveElement(
				AbsoluteFilePath.Parse("foo/bar"),
				Observable.Never<ILookup<Simulator.ObjectIdentifier, Simulator.ObjectIdentifier>>(),
				Observable.Return(true),
				new Subject<Unit>(),
				Observer.Create<IBinaryMessage>(msg => { }),
				s => Element.Empty);

			// Panel is an example of an instance
			// Circle is an example of a class (called MyClass)
			parent.UpdateFrom(SourceFragment.FromString(uxSource).ToXml());

			return parent;
		}
	}
}
