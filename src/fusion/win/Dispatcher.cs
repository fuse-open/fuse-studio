using System;
using System.Reactive.Subjects;
using System.Threading;
using System.Windows.Threading;

namespace Outracks.Fusion.Windows
{
	public class Dispatcher : SingleThreadDispatcherBase
	{
		readonly IObservable<long> _perFrame;
		readonly DispatcherTimer _timer = new DispatcherTimer(DispatcherPriority.Render)
		{
			Interval = TimeSpan.FromSeconds(1.0 / 60.0)
		};

		public IObservable<long> PerFrame
		{
			get { return _perFrame; }
		}

		public Dispatcher(Thread thread) : base(thread)
		{
			var perFrame = new Subject<long>();
			long i = 0;
			_timer.Tick += (s, a) =>
			{
				Drain();
				perFrame.OnNext(i++);
			};
			_timer.Start();
			_perFrame = perFrame;
		}

		protected override void Flush()
		{
			if (RunningOnDispatcherThread)
				Drain();
		}
	}
}