using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using NSubstitute;
using NUnit.Framework;
using Outracks.Fuse.Live;
using Outracks.Fuse.Refactoring;
using Outracks.IO;
using Outracks.Simulator;

namespace Outracks.Fuse.Protocol.Tests.Refactoring
{
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
		public void SetUp()
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

			_classElement = GetTreeChild("Circle");
			_instanceElement = GetTreeChild("Panel");
			_javaScriptElement = GetTreeChild("JavaScript");
			_includeElement = GetTreeChild("Include");
			project.Classes.Returns(Observable.Return(new[] { _classElement }));
		}

		[Test]
		public void Button_is_disabled_while_nothing_is_selected()
		{
			_currentSelection.OnNext(Element.Empty);
			AssertButtonDisabled();
		}

		[Test]
		public void Button_is_disabled_while_selecting_element_that_is_already_a_class()
		{
			_currentSelection.OnNext(_classElement);
			AssertButtonDisabled();
		}

		[Test]
		public void Button_is_disabled_while_selecting_root()
		{
			_currentSelection.OnNext(_root);
			AssertButtonDisabled();
		}

		[Test]
		public void Button_is_disabled_while_javascript_selected()
		{
			_currentSelection.OnNext(_javaScriptElement);
			AssertButtonDisabled();
		}

		[Test]
		public void Button_is_disabled_while_include_element_selected()
		{
			_currentSelection.OnNext(_includeElement);
			AssertButtonDisabled();
		}

		[Test]
		public void Button_is_enabled_while_element_that_is_an_instance_is_selected()
		{
			_currentSelection.OnNext(_instanceElement);
			AssertButtonEnabled();
		}

		[Test]
		public void HighlightSelectedElement_is_true_after_HoverEnter_is_called_while_instance_element_is_selected()
		{
			_currentSelection.OnNext(_instanceElement);
			_model.HoverEnter();
			Assert.That(_model.HighlightSelectedElement.LastNonBlocking, Is.True);
		}

		[Test]
		public void HighlightSelectedElement_is_false_after_HoverEnter_is_called_while_class_element_is_selected()
		{
			_currentSelection.OnNext(_classElement);
			_model.HoverEnter();
			Assert.That(_model.HighlightSelectedElement.LastNonBlocking(), Is.False);
		}

		[Test]
		public void HighlightSelectedElement_is_cleared_after_HoverExit_is_called_while_instance_element_is_selected()
		{
			_currentSelection.OnNext(_instanceElement);
			_model.HoverEnter();
			Assert.That(_model.HighlightSelectedElement.LastNonBlocking(), Is.True);
			_model.HoverExit();
			Assert.That(_model.HighlightSelectedElement.LastNonBlocking, Is.False);
		}

		[Test]
		public void Clicking_button_invokes_and_passes_dialog_view_model_to_callback()
		{
			_currentSelection.OnNext(_instanceElement);
			var action = _model.Command.Action.LastNonBlocking().Value;
			action();
			Assert.That(_buttonClicked, Is.True);
			Assert.That(_resultingViewModel, Is.Not.Null);

			// Just do some initial assertions that the dialog is in a state we expect
			// (more testing of the dialog in another fixture)
			Assert.That(_resultingViewModel.ClassName.LastNonBlocking(), Is.EqualTo("MyPanel"));
			Assert.That(_resultingViewModel.CreateInNewFile.LastNonBlocking, Is.False);
			Assert.That(_resultingViewModel.CreateInNewFile.IsReadOnly.LastNonBlocking, Is.False);
		}

		IElement GetTreeChild(string name)
		{
			return _root.Children.Where(x => x.Name.Is(name)).Select(x => x.First()).LastNonBlocking();
		}

		void AssertButtonDisabled()
		{
			Assert.That(_model.Command.Action.LastNonBlocking().HasValue, Is.False);
		}

		void AssertButtonEnabled()
		{
			Assert.That(_model.Command.Action.LastNonBlocking().HasValue, Is.True);
		}

		static IElement CreateTree(string uxSource)
		{
			var parent = new LiveElement(
				AbsoluteFilePath.Parse("foo/bar"),
				Observable.Never<ILookup<ObjectIdentifier, ObjectIdentifier>>(),
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
