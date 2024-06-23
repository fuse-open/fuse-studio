using System;

namespace Outracks
{
	public static class TrySomethingExtension
	{
		public static void TrySomethingBlocking(this IReport errorHandler, Action action)
		{
			try
			{
				action();
			}
			catch (Exception e)
			{
				errorHandler.Exception("Something failed: " + e.Message, e);
			}
		}

		public static T Try<T>(this IReport errorHandler, Func<T> action, T defaultResult)
		{
			try
			{
				return action();
			}
			catch (Exception e)
			{
				errorHandler.Exception("Something failed: " + e.Message, e);
				return defaultResult;
			}
		}
	}
}