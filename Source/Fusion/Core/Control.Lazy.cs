using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Outracks.Fusion
{
	public static partial class Control
	{
		public static IControl Lazy(Func<IControl> content)
		{
			var resultObs = new BehaviorSubject<IControl>(Empty);
			var result = resultObs.Switch();

			result
				.IsRooted
				.Where(b => b)
				.Take(1)
				.Subscribe(_ => resultObs.OnNext(content()));

			return result;
		}
	}
}