using System;

namespace Outracks.Diagnostics.Reactive
{
	public interface IObservableTraceHandler
	{
		TOut Trace<TIn, TOut>(TIn source, Func<TIn, TOut> factory, ObservableTraceInfo info);
	}
}