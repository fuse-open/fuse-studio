using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Outracks.Simulator;

namespace Outracks.Fuse.Live
{
	partial class LiveElement
	{
		public IObservable<bool> IsSiblingOf(string type)
		{
			// Here we exploit the fact that siblings of a LiveElement will always be a LiveElement
			// This is to avoid combining with all siblings.
			var typeObjectIdentifier = new ObjectIdentifier(type);
			return _parent.Select(
				parent => parent.Children.WherePerElement(x => x != this).CombineLatest(
					_metadata,
					(siblings, metadata) =>
					{
						return siblings.OfType<LiveElement>()
							.Any(sibling => Is(sibling._elementId.Value, typeObjectIdentifier, metadata));
					})).Or(Observable.Return(false));
		}
		
		public IObservable<IEnumerable<IElement>> Siblings()
		{
			return Parent.Children.Select(x => x.ExceptOne(this));
		}
	}
}
