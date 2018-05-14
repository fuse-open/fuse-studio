using System;
using System.Collections.Generic;
using System.Linq;

namespace Outracks
{
	public static class Disposable
	{
		public static IDisposable Combine(IEnumerable<IDisposable> disposables)
		{
			return Combine(disposables.ToArray());
		}

		public static IDisposable Combine(params IDisposable[] disposables)
		{
			return Create(
				() =>
				{
					Exception exception = null;
					for (int i = disposables.Length; i-- > 0; )
					{
						try
						{
							disposables[i].Dispose();
						}
						catch (Exception e)
						{
							exception = e;
						}
					}
					if (exception != null)
						exception.RethrowWithStackTrace();
				});
		}

		public static IDisposable Create(Action dispose)
		{
			return System.Reactive.Disposables.Disposable.Create(dispose);
		}

		public static IDisposable Empty
		{
			get { return System.Reactive.Disposables.Disposable.Empty; }
		}
	}
}