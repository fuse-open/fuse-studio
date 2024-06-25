using System.Threading;
using CoreFoundation;

namespace Outracks.UnoHost.Mac
{
	public class Dispatcher : SingleThreadDispatcherBase
	{
		public Dispatcher(Thread thread) : base(thread) { }

		protected override void Flush()
		{
			if (RunningOnDispatcherThread)
				Drain();
			else
				DispatchQueue.MainQueue.DispatchAsync(Drain);
		}
	}
}
