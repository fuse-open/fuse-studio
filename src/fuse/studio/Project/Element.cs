using System;
using System.Reactive.Linq;

namespace Outracks.Fuse
{
	public static partial class Element
	{
		public static IObservable<bool> HasProperty(this IElement element, string property)
		{
			return element[property].Select(v => v.HasValue);
		}

		public static IObservable<bool> IsSameAs(this IElement element, IElement other)
		{
			return element.SimulatorId.CombineLatest(other.SimulatorId, (a, b) => a.Equals(b));
		}

		public static IElement As(this IElement element, string type)
		{
			return element.Is(type)
				.Select(isType => isType ? element : Empty)
				.Switch();
		}
	}
}