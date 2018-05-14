using System.Threading;

namespace Outracks.UnoHost.OSX
{
	public class Dispatcher : SingleThreadDispatcherBase
	{
		public Dispatcher(Thread thread) : base(thread) { }

		protected override void Flush()
		{
			if (RunningOnDispatcherThread)
				Drain();
			else
				CoreFoundation.DispatchQueue.MainQueue.DispatchAsync(Drain);
		}
	}
}
