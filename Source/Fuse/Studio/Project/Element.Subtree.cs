using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Outracks.Fuse
{
	public static partial class Element
	{

		public static IObservable<IEnumerable<IElement>> Subtree(this IElement root)
		{
			return root.Children.CachePerElement(Subtree)
				.ToObservableEnumerable()
				.Select(e => e.Join())
				.Select(e => new[] { root }.Concat(e));
		}
	}
}