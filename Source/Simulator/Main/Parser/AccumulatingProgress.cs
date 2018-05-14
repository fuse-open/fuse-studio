using System;
using System.Collections.Immutable;

namespace Outracks.Simulator.Parser
{
	public class AccumulatingProgress<T> : IProgress<T>
	{
		readonly IProgress<T> _progress;

		System.Collections.Immutable.ImmutableList<T> _reportedProgress = ImmutableList.Create<T>();

		public System.Collections.Immutable.ImmutableList<T> GetProgressSoFar()
		{
			return _reportedProgress;
		}

		public AccumulatingProgress(IProgress<T> progress)
		{
			_progress = progress;
		}

		public void Report(T value)
		{
			_reportedProgress = _reportedProgress.Add(value);
			_progress.Report(value);
		}
	}
}