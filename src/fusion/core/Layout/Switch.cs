using System;
using System.Collections.Immutable;
using System.Reactive.Linq;

namespace Outracks.Fusion
{
	public static partial class Layout
	{
		public static IControl ToControl<T>(this IObservable<T> content, Func<T, IControl> select)
		{
			return content.DistinctUntilChanged().Select(select).Switch();
		}

		public static IControl Switch(this IObservable<IControl> content)
		{
			content = content
				//.StartWith(Control.Empty)
				.DistinctUntilChanged()
				.Replay(1).RefCount()
				;

			var size = content
				.Switch(n => n.DesiredSize)
				.DistinctUntilChanged()
				.Replay(1).RefCount()
				;

			return Layer(content.Select(ImmutableList.Create)).WithSize(size);
		}
	}
}