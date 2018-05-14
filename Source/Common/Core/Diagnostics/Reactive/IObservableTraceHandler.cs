using System;
using System.Reactive.Subjects;

namespace Outracks.Diagnostics.Reactive
{
	public interface IObservableTraceHandler
	{
		TOut Trace<TIn, TOut>(TIn source, Func<TIn, TOut> factory, ObservableTraceInfo info);
	}
}