using System;
using System.Collections.Concurrent;
using Outracks.Fuse.Protocol;

namespace Outracks.Fuse.Daemon
{
	class ErrorListIntercepter : IDisposable
	{
		public readonly ConcurrentQueue<Error> Errors = new ConcurrentQueue<Error>();
		readonly IDisposable _errorSub;

		public ErrorListIntercepter(IObservable<Error> errors)
		{
			_errorSub = errors.Subscribe(e => Errors.Enqueue(e));
		}

		public bool HasErrors()
		{
			return !Errors.IsEmpty;
		}

		public void Dispose()
		{
			_errorSub.Dispose();
		}
	}
}