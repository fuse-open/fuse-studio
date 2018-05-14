using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using Outracks.Fusion;

namespace Outracks.Fuse.Hierarchy
{
	public interface ITreeViewModel
	{
		int VisibleRowCount { get; set; }
		int VisibleRowOffset { get; set; }

		IObservable<bool> HighlightSelectedElement { get; }

		IObservable<int> ScrollTarget { get; }

		IObservable<int> TotalRowCount { get; }
		IObservable<IEnumerable<ITreeRowViewModel>> VisibleRows { get; }

		Command PopScopeCommand { get; }
		IObservable<string> PopScopeLabel { get; }

		IObservable<Optional<PendingDrop>> PendingDrop { get; }

		Command NavigateUpCommand { get; }
		Command NavigateDownCommand { get; }
	}
}
