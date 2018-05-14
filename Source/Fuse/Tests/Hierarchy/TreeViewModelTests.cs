using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Outracks.Fuse.Hierarchy;
using Outracks.Fuse.Live;
using Outracks.Fusion;
using Outracks.Simulator;

namespace Outracks.Fuse.Tests.Hierarchy
{
	[TestFixture]
	public class TreeViewModelTests
	{
		[SetUp]
		public void SetUp()
		{
			_path = "/project/MainView.ux";
			var invalidated = new Subject<Unit>();
			_element = LiveElementFactory.CreateLiveElement("<App><StackPanel/></App>", _path, invalidated);
			UpdateElementIds();
			invalidated.Subscribe(_ => UpdateElementIds());
			_context = new Context(
				_element,
				id =>
				{
					UpdateElementIds();
					return _element.DescendantsAndSelf().Where(x => x.SimulatorId.Value == id).FirstOr(Element.Empty);
				});
			//await _context.Select(_element);
			_highlightSelectedElement = new BehaviorSubject<bool>(true);
			_model = new TreeViewModel(
				_context,
				_highlightSelectedElement,
				_ => { throw new NotImplementedException(); });

			_lastTotalRowCount = null;
			_lastVisibleRows = null;
			_model.VisibleRowCount = 10;
			_model.TotalRowCount.Subscribe(v => _lastTotalRowCount = v);
			_model.VisibleRows.Subscribe(v => _lastVisibleRows = v);
		}

		[TearDown]
		public void TearDown()
		{
			_element = null;
			_context = null;
			_model = null;
		}

		LiveElement _element;
		Context _context;
		TreeViewModel _model;

		int? _lastTotalRowCount;
		IEnumerable<ITreeRowViewModel> _lastVisibleRows;
		string _path;
		BehaviorSubject<bool> _highlightSelectedElement;

		[Test]
		public async Task Ancestors_should_automatically_expand_when_element_in_Ancestor_part_of_tree_is_selected()
		{
			UpdateXml(
				"<App><Ancestor0><Ancestor1><Ancestor2><Ancestor3><Ancestor4><SelectedElement /></Ancestor4></Ancestor3></Ancestor2></Ancestor1></Ancestor0></App>");
			var selectedElement = GetElementByName("SelectedElement");
			var ancestor0Row = _lastVisibleRows.First(x => x.ElementName.LastNonBlocking() == "Ancestor0");
			// Make sure Ancestor0 is collapsed
			if (ancestor0Row.IsExpanded.LastNonBlocking())
			{
				await ancestor0Row.ExpandToggleCommand.ExecuteOnceAsync();
				Assert.That(ancestor0Row.IsExpanded.LastNonBlocking(), Is.EqualTo(false));
			}
			Assert.That(_lastTotalRowCount, Is.EqualTo(2));
			await _context.Select(selectedElement);
			Assert.That(_context.CurrentSelection.LiveElement.LastNonBlocking(), Is.EqualTo(Optional.Some(selectedElement)));
			Assert.That(_lastTotalRowCount, Is.EqualTo(7));
			Assert.That(_lastVisibleRows.FirstOrDefault(x => x.ElementName.LastNonBlocking() == "SelectedElement"), Is.Not.Null);
		}

		[Test]
		public void Non_root_ux_class_element_is_not_expandable()
		{
			UpdateXml("<App><StackPanel ux:Class='FooBar'><Panel/><Panel/></StackPanel></App>");
			Assert.That(_lastTotalRowCount, Is.EqualTo(2));
			var classRowModel = GetRow(1);
			Assert.That(classRowModel.IsExpanded.LastNonBlocking(), Is.False);
			Assert.That(classRowModel.CanExpand.LastNonBlocking(), Is.False);
			Assert.That(classRowModel.ExpandToggleCommand.IsEnabled.LastNonBlocking(), Is.False);
		}

		[Test]
		public async Task Faded_of_deselected_element_is_true_while_HighlightSelectedElement_is_true()
		{
			UpdateXml("<App><Selected/><Deselected/></App>");
			await _context.Select(GetElementByName("Selected"));
			var deselectedRow = GetRowByName("Deselected");
			_highlightSelectedElement.OnNext(false);
			Assert.That(deselectedRow.IsFaded.LastNonBlocking(), Is.False);
			_highlightSelectedElement.OnNext(true);
			Assert.That(deselectedRow.IsFaded.LastNonBlocking(), Is.True);
		}

		[Test]
		public async Task Faded_of_selected_element_is_false_while_HighlightSelectedElement_is_true()
		{
			UpdateXml("<App><Selected/><Deselected/></App>");
			await _context.Select(GetElementByName("Selected"));
			var deselectedRow = GetRowByName("Selected");
			_highlightSelectedElement.OnNext(false);
			Assert.That(deselectedRow.IsFaded.LastNonBlocking(), Is.False);
			_highlightSelectedElement.OnNext(true);
			Assert.That(deselectedRow.IsFaded.LastNonBlocking(), Is.False);
		}

		[Test]
		public async Task Faded_of_selected_descendant_is_false_while_HighlightSelectedElement_is_true()
		{
			UpdateXml("<App><Selected><Descendant/></Selected><Deselected/></App>");
			await _context.Select(GetElementByName("Selected"));
			var deselectedRow = GetRowByName("Descendant");
			_highlightSelectedElement.OnNext(false);
			Assert.That(deselectedRow.IsFaded.LastNonBlocking(), Is.False);
			_highlightSelectedElement.OnNext(true);
			Assert.That(deselectedRow.IsFaded.LastNonBlocking(), Is.False);
		}

		[Test]
		public void Dropping_element_inside_itself_is_illegal()
		{
			UpdateXml("<App><Dragged/></App>");
			var draggedObject = GetRowByName("Dragged").DraggedObject.LastNonBlocking();
			var targetRow = GetRowByName("Dragged");
			Assert.That(targetRow.CanDrop(DropPosition.Inside, draggedObject), Is.False);
		}

		[Test]
		public void Dropping_ancestor_element_inside_descendant_is_illegal()
		{
			UpdateXml("<App><Dragged><Target/></Dragged></App>");
			var draggedObject = GetRowByName("Dragged").DraggedObject.LastNonBlocking();
			var targetRow = GetRowByName("Target");
			Assert.That(targetRow.CanDrop(DropPosition.Inside, draggedObject), Is.False);
		}

		[Test]
		public void Dragging_an_element_and_dropping_it_inside_another_is_successful()
		{
			UpdateXml("<App><Dragged/><Target/></App>");
			var draggedObject = GetRowByName("Dragged").DraggedObject.LastNonBlocking();
			var targetRow = GetRowByName("Target");
			targetRow.DragEnter(DropPosition.Inside);
			var pendingDrop = _model.PendingDrop.LastNonBlocking();
			pendingDrop.Do(
				pd =>
				{
					Assert.That(pd.DropPosition, Is.EqualTo(DropPosition.Inside));
					Assert.That(pd.Depth, Is.EqualTo(1));
					Assert.That(pd.RowOffset, Is.EqualTo(targetRow.RowOffset.LastNonBlocking()));
				},
				() => Assert.Fail("Expected there to be a drop pending"));
			Assert.That(targetRow.CanDrop(DropPosition.Inside, draggedObject), Is.True);

			targetRow.Drop(DropPosition.Inside, draggedObject);

			Assert.That(GetRow(1).ElementName.LastNonBlocking(), Is.EqualTo("Target"));
			Assert.That(GetRow(1).Depth.LastNonBlocking(), Is.EqualTo(1));
			Assert.That(GetRow(2).ElementName.LastNonBlocking(), Is.EqualTo("Dragged"));
			Assert.That(GetRow(2).Depth.LastNonBlocking(), Is.EqualTo(2));
		}

		[Test]
		public void Dragging_an_element_and_dropping_it_before_another_is_successful()
		{
			UpdateXml("<App><Target/><Dragged/></App>");
			var draggedObject = GetRowByName("Dragged").DraggedObject.LastNonBlocking();
			var targetRow = GetRowByName("Target");
			var dropPosition = DropPosition.Before;
			targetRow.DragEnter(dropPosition);
			var pendingDrop = _model.PendingDrop.LastNonBlocking();
			pendingDrop.Do(
				pd =>
				{
					Assert.That(pd.DropPosition, Is.EqualTo(dropPosition));
					Assert.That(pd.Depth, Is.EqualTo(1));
					Assert.That(pd.RowOffset, Is.EqualTo(targetRow.RowOffset.LastNonBlocking()));
				},
				() => Assert.Fail("Expected there to be a drop pending"));
			Assert.That(targetRow.CanDrop(dropPosition, draggedObject), Is.True);

			targetRow.Drop(dropPosition, draggedObject);

			Assert.That(GetRow(1).ElementName.LastNonBlocking(), Is.EqualTo("Dragged"));
			Assert.That(GetRow(1).Depth.LastNonBlocking(), Is.EqualTo(1));
			Assert.That(GetRow(2).ElementName.LastNonBlocking(), Is.EqualTo("Target"));
			Assert.That(GetRow(2).Depth.LastNonBlocking(), Is.EqualTo(1));
		}

		[Test]
		public void Dragging_an_element_and_dropping_it_after_another_is_successful()
		{
			UpdateXml("<App><Dragged/><Target/></App>");
			var draggedObject = GetRowByName("Dragged").DraggedObject.LastNonBlocking();
			var targetRow = GetRowByName("Target");
			var dropPosition = DropPosition.After;
			targetRow.DragEnter(dropPosition);
			var pendingDrop = _model.PendingDrop.LastNonBlocking();
			pendingDrop.Do(
				pd =>
				{
					Assert.That(pd.DropPosition, Is.EqualTo(dropPosition));
					Assert.That(pd.Depth, Is.EqualTo(1));
					Assert.That(pd.RowOffset, Is.EqualTo(targetRow.RowOffset.LastNonBlocking()));
				},
				() => Assert.Fail("Expected there to be a drop pending"));
			Assert.That(targetRow.CanDrop(dropPosition, draggedObject), Is.True);

			targetRow.Drop(dropPosition, draggedObject);

			Assert.That(GetRow(1).ElementName.LastNonBlocking(), Is.EqualTo("Target"));
			Assert.That(GetRow(1).Depth.LastNonBlocking(), Is.EqualTo(1));
			Assert.That(GetRow(2).ElementName.LastNonBlocking(), Is.EqualTo("Dragged"));
			Assert.That(GetRow(2).Depth.LastNonBlocking(), Is.EqualTo(1));
		}

		[Test]
		public async Task IsAncestorSelected_of_row_is_true_for_descendant_of_selected_element()
		{
			UpdateXml("<App><Selected><Descendant/></Selected></App>");
			await _context.Select(GetElementByName("Selected"));
			Assert.That(GetRowByName("Descendant").IsAncestorSelected.LastNonBlocking(), Is.True);
		}

		[Test]
		public void ScopeIntoClassCommand_is_disabled_for_non_class_element()
		{
			UpdateXml("<App><Panel/></App>");
			var row = GetRow(1);
			Assert.That(row.ScopeIntoClassCommand.IsEnabled.LastNonBlocking(), Is.False);
		}

		[Test]
		public void ScopeIntoClassCommand_is_disabled_for_class_root_element()
		{
			UpdateXml("<Panel ux:Class='PoshAndIgnorantUpperClass'/>");
			var row = GetRow(0);
			Assert.That(row.ScopeIntoClassCommand.IsEnabled.LastNonBlocking(), Is.False);
		}

		[Test]
		public async Task ScopeIntoClassCommand_for_class_element_scopes_into_class()
		{
			UpdateXml("<App><Panel ux:Class='PoshAndIgnorantUpperClass'/></App>");
			var row = GetRow(1);
			Assert.That(row.ScopeIntoClassCommand.IsEnabled.LastNonBlocking(), Is.True);
			await row.ScopeIntoClassCommand.ExecuteOnceAsync();

			// Should be scoped into class now, having only one row
			Assert.That(_model.TotalRowCount.LastNonBlocking(), Is.EqualTo(1));
			row = GetRow(0);
			Assert.That(row.HeaderText.LastNonBlocking(), Is.EqualTo("PoshAndIgnorantUpperClass (Panel)"));
			Assert.That(row.Depth.LastNonBlocking(), Is.EqualTo(0));
		}

		[Test]
		public void PopScopeCommand_is_disabled_when_scope_stack_is_empty()
		{
			Assert.That(_model.PopScopeCommand.IsEnabled.LastNonBlocking(), Is.False);
		}

		[Test]
		public async Task PopScopeCommand_is_enabled_and_works_when_there_are_scopes_to_pop()
		{
			UpdateXml("<App><Panel ux:Class='PoshAndIgnorantUpperClass'/></App>");
			var classElement = GetElementByName("Panel");
			await _context.PushScope(classElement, classElement);
			Assert.That(_model.PopScopeCommand.IsEnabled.LastNonBlocking(), Is.True);
			await _model.PopScopeCommand.ExecuteOnceAsync();
			Assert.That(GetRow(0).ElementName.LastNonBlocking(), Is.EqualTo("App"));
		}

		[Test]
		public async Task IsSelected_of_row_for_selected_element_is_true()
		{
			UpdateXml("<App><Panel/><Selected/></App>");
			await _context.Select(GetElementByName("Selected"));
			var selectedRow = GetRowByName("Selected");
			Assert.That(selectedRow.IsSelected.LastNonBlocking(), Is.True);
		}

		[Test]
		public async Task IsSelected_of_row_for_not_selected_element_is_false()
		{
			UpdateXml("<App><Panel/><Selected/></App>");
			await _context.Select(GetElementByName("Selected"));
			var selectedRow = GetRowByName("Panel");
			Assert.That(selectedRow.IsSelected.LastNonBlocking(), Is.False);
		}

		[Test]
		public void HeaderText_of_row_for_instance_element_is_correct()
		{
			UpdateXml("<App><Panel/></App>");
			Assert.That(GetRowByName("Panel").HeaderText.LastNonBlocking(), Is.EqualTo("Panel"));
		}

		[Test]
		public void HeaderText_of_row_for_ux_class_element_is_correct()
		{
			UpdateXml("<App><Panel ux:Class='MyFancyClass'/></App>");
			Assert.That(GetRowByName("Panel").HeaderText.LastNonBlocking(), Is.EqualTo("MyFancyClass (Panel)"));
		}

		[Test]
		public async Task Selecting_an_element_outside_of_visible_rows_pushes_selected_element_to_ScrollTarget()
		{
			_model.VisibleRowCount = 2;
			UpdateXml("<App><N1/><N2/><N3/><N4/></App>");

			int? scrollTarget = null;
			_model.ScrollTarget.Subscribe(v => scrollTarget = v);
			Assert.That(scrollTarget, Is.Null);

			await _context.Select(GetElementByName("N4"));
			Assert.That(scrollTarget, Is.EqualTo(4));
		}

		[Test]
		public async Task Model_will_not_keep_alive_elements_after_they_have_been_removed_from_tree()
		{
			UpdateXml("<App><StackPanel><FooBar/></StackPanel></App>");
			var row = GetRowByName("StackPanel");
			var elementWeakRef = GetElementWeakReferenceByName("StackPanel");
			await row.ExpandToggleCommand.ExecuteOnceAsync();
			UpdateXml("<App />");

			// Do a double collect, for good measure.
			GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
			Thread.Sleep(100);
			GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);

			ILiveElement _;
			Assert.That(elementWeakRef.TryGetTarget(out _), Is.False);
			// We have to be very careful to not keep a strong reference on the stack
		}

		[Test]
		public async Task Elements_that_were_previously_visible_and_still_is_after_update_will_be_assigned_same_row_model()
		{
			// The point of this test is to make sure the assignment of elements to rows
			// is consistent over several scans, ie. reusing the same row for the same element.

			UpdateXml("<App><Pre/><Expandable><Inner/></Expandable><Post/></App>");
			var preRowBeforeExpandToggle = GetRowByName("Pre");
			var postRowBeforeExpandToggle = GetRowByName("Post");
			var expandableRow = GetRowByName("Expandable");
			var totalRowCountBefore = _lastTotalRowCount;

			// Toggling the expand state of the expandable row should either collapse or expand it,
			// either inserting or removing a row from the middle of the list.
			await expandableRow.ExpandToggleCommand.ExecuteOnceAsync();

			Assert.That(_lastTotalRowCount, Is.Not.EqualTo(totalRowCountBefore));
			Assert.That(GetRowByName("Pre"), Is.EqualTo(preRowBeforeExpandToggle));
			Assert.That(GetRowByName("Post"), Is.EqualTo(postRowBeforeExpandToggle));
		}

		[Test]
		public async Task ExpandToggleCommand_of_expanded_row_collapses_it()
		{
			// Arrange
			UpdateXml("<App><StackPanel><Panel/></StackPanel></App>");
			Assert.That(_lastTotalRowCount, Is.EqualTo(3));
			var stackRow = GetRow(1);
			Assert.That(stackRow.IsExpanded.LastNonBlocking(), Is.True);

			// Act
			await stackRow.ExpandToggleCommand.ExecuteOnceAsync();

			// Assert
			Assert.That(_lastTotalRowCount, Is.EqualTo(2));
			Assert.That(stackRow.IsExpanded.LastNonBlocking(), Is.False);
			await stackRow.ExpandToggleCommand.ExecuteOnceAsync();
			Assert.That(_lastTotalRowCount, Is.EqualTo(3));
			Assert.That(stackRow.IsExpanded.LastNonBlocking(), Is.True);
		}

		[Test]
		public async Task ExpandToggleCommand_of_expanded_row_collapses_it_and_selects_it_when_descendant_was_selected()
		{
			// Arrange
			UpdateXml("<App><StackPanel><Descendant/></StackPanel></App>");
			Assert.That(_lastTotalRowCount, Is.EqualTo(3));
			var stackRow = GetRowByName("StackPanel");
			Assert.That(stackRow.IsExpanded.LastNonBlocking(), Is.True);
			await _context.Select(GetElementByName("Descendant"));

			// Act
			await stackRow.ExpandToggleCommand.ExecuteOnceAsync();

			//Assert
			Assert.That(_lastTotalRowCount, Is.EqualTo(2));
			Assert.That(stackRow.IsSelected.LastNonBlocking(), Is.True);
		}

		[Test]
		public void Root_element_with_ux_class_element_is_expanded()
		{
			UpdateXml("<Panel ux:Class='TurboPanel'><Rectangle /></Panel>");
			Assert.That(_lastTotalRowCount, Is.EqualTo(2));
		}

		[Test]
		public void Setting_VisibleRowCount_will_add_new_rows_if_count_of_VisibleRows_is_less()
		{
			UpdateXml("<App><N1/><N2/><N3/><N4/><N5/><N6/><N7/><N8/><N9/><N10/><N11/></App>");
			// Just checking that our default MaxVisibleRowCount small enough for the test data
			Assert.That(_lastTotalRowCount, Is.GreaterThan(_model.VisibleRowCount));
			_model.VisibleRowCount = 11;
			Assert.That(_lastVisibleRows.Count(), Is.EqualTo(11));
			_model.VisibleRowCount = 3;
			// Decreasing the visible rows isn't expected to reduce the capacity
			Assert.That(_lastVisibleRows.Count(), Is.EqualTo(11));
		}

		[Test]
		public void TotalRowCount_is_updated_when_child_is_added_or_removed_from_element()
		{
			Assert.That(_lastTotalRowCount, Is.EqualTo(2));
			UpdateXml("<App><StackPanel><Panel/><Panel/></StackPanel></App>");
			Assert.That(_lastTotalRowCount, Is.EqualTo(4));
			UpdateXml("<App><StackPanel /></App>");
			Assert.That(_lastTotalRowCount, Is.EqualTo(2));
		}

		void UpdateElementIds()
		{
			var dict = new Dictionary<ObjectIdentifier, IElement>();
			// Need to update element id first
			_element.UpdateElementIds(_path, 0, dict);
		}

		ITreeRowViewModel GetRow(int rowOffset)
		{
			return GetRow(x => x.RowOffset, rowOffset);
		}

		ITreeRowViewModel GetRowByName(string name)
		{
			return GetRow(x => x.ElementName, name);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		WeakReference<ILiveElement> GetElementWeakReferenceByName(string name)
		{
			return new WeakReference<ILiveElement>(GetElementByName(name));
		}

		ILiveElement GetElementByName(string name)
		{
			return _element.DescendantsAndSelf().First(x => x.Name.Value == name);
		}

		ITreeRowViewModel GetRow<TValue>(Func<ITreeRowViewModel, IObservable<TValue>> obsProp, TValue value)
		{
			Assert.That(_lastVisibleRows, Is.Not.Null);
			var classRowModel =
				_lastVisibleRows.FirstOrDefault(row => obsProp(row).LastNonBlocking().Equals(value));
			Assert.That(classRowModel, Is.Not.Null);
			return classRowModel;
		}

		void UpdateXml(string xml)
		{
			_element.UpdateFrom(xml);
			UpdateElementIds();
		}
	}
}