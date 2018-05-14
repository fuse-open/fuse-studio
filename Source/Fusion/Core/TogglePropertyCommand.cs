using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Outracks.Fusion
{
	public static class TogglePropertyCommand
	{
		public static Command Toggle(this IProperty<bool> self)
		{
			return Command.Enabled(() => self.Take(1).Subscribe(v => self.Write(!v, save: false)));
		}

		public static Command Toggle(this BehaviorSubject<bool> self)
		{
			return Command.Enabled(() => self.OnNext(!self.Value));
		}
	}
}