using System;
using System.Runtime.ExceptionServices;

namespace Outracks
{
	public static class RethrowExtension
	{
		public static void RethrowWithStackTrace(this Exception e)
		{
			ExceptionDispatchInfo.Capture(e).Throw();
		}
	}
}