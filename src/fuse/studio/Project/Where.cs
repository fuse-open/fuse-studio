using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Outracks.Fuse
{
	public static partial class Element
	{
		public static IObservable<IEnumerable<IElement>> Where(
			this IObservable<IEnumerable<IElement>> self,
			Func<IElement, IObservable<bool>> predicate)
		{
			return self.Switch(elements =>
				elements.Select(elm => predicate(elm).Select(passed => passed ? Optional.Some(elm) : Optional.None()))
					.ToObservableEnumerable()
					.Select(elms => elms.NotNone()));
		}

	}
}