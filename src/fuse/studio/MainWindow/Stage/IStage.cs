using System;
using System.Collections.Generic;

namespace Outracks.Fuse.Stage
{
	interface IStage
	{
		IObservable<IEnumerable<IViewport>> Viewports { get; }

		IObservable<Optional<IViewport>> FocusedViewport { get; }
	}
}
