using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Outracks.Fusion;

namespace Outracks
{
	public static class UpdateSubjectCommand
	{
		public static Command Update<T>(this IObserver<T> self, T value)
		{
			return Command.Enabled(() => self.OnNext(value));
		}

		public static Command Update<T>(this ISubject<T, T> self, Func<T, T> transform)
		{
			return self.DistinctUntilChanged().Switch(value =>
				Command.Enabled(() => self.OnNext(transform(value))));
		}
	}
}
