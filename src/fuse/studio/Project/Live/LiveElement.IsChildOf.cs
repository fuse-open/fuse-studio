using System;
using System.Reactive.Linq;

namespace Outracks.Fuse.Live
{
	partial class LiveElement
	{
		public IObservable<bool> IsChildOf(string type)
		{
			return Parent.Is(type);
		}

		public IObservable<bool> IsDescendantOf(IElement element)
		{
			return Parent.SimulatorId.CombineLatest(element.SimulatorId, (our, their) => our.Equals(their))
				.Or(Parent.IsDescendantOf(element));
		}

	}
}
