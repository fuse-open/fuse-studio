using System;
using Outracks.Fusion;

namespace Outracks.Fuse.Hierarchy
{
	public interface ITreeRowViewModel
	{
		IObservable<int> RowOffset { get; }
		IObservable<int> Depth { get; }
		IObservable<int> ExpandedDescendantCount { get; }

		Command ExpandToggleCommand { get; }
		IObservable<bool> CanExpand { get; }
		IObservable<bool> IsExpanded { get; }
		IObservable<bool> IsSelected { get; }

		IObservable<bool> IsAncestorSelected { get; }
		Command SelectCommand { get; }
		IObservable<bool> IsFaded { get; }
		Command ScopeIntoClassCommand { get; }

		IObservable<string> HeaderText { get; }
		IObservable<string> ElementName { get; }

		Menu ContextMenu { get; }

		IObservable<object> DraggedObject { get; }
		void DragEnter(DropPosition position);
		void DragExit();
		bool CanDrop(DropPosition position, object dragged);
		void Drop(DropPosition position, object dragged);

		Command EnterHoverCommand { get; }
		Command ExitHoverCommand { get; }
	}
}