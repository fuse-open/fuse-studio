using System;
using System.Reactive;
using System.Threading.Tasks;

namespace Outracks.Fusion.Mac
{
	public static class MainThread
	{
		public static Task BeginInvoke(Action operation)
		{
			return Fusion.Application.MainThread.InvokeAsync(() =>
			{
				operation();
				return Unit.Default;
			});
		}
	}
}