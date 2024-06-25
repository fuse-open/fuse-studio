using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Outracks.UnoHost.Windows
{
	public class FpsProfiler
	{
		readonly Subject<TimeSpan> _samples = new Subject<TimeSpan>();

		public IObservable<int> GetAverageFramesPerSecond(int numSamples)
		{
			return GetSumOfFps(numSamples)
				.Select(sumOfTime => (int)Math.Round(sumOfTime / numSamples));
		}

		public IObservable<double> GetAverageFrameTime(int numSamples)
		{
			return GetSumOfSamples(numSamples)
				.Select(sumTime => sumTime / (double)numSamples);
		}

		IObservable<double> GetSumOfFps(int numSamples)
		{
			return _samples
				.Buffer(numSamples)
				.Select(timespans => timespans.Sum(span => 1.0 / span.TotalSeconds));
		}

		IObservable<double> GetSumOfSamples(int numSamples)
		{
			return _samples
				.Buffer(numSamples)
				.Select(timespans => timespans.Sum(span => span.TotalSeconds));
		}

		public void Profile(Action profileMe)
		{
			var stopwatch = Stopwatch.StartNew();
			profileMe();
			_samples.OnNext(stopwatch.Elapsed);
		}
	}
}