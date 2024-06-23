using System;
using System.Reactive;
using System.Reactive.Subjects;
using Outracks;
using Outracks.Simulator.Parser;
using Uno.Logging;

namespace Fuse.Preview
{
	class LogSubject
	{
		public LogSubject(Guid id)
		{
			var buildEvents = new Subject<IBinaryMessage>();
			var loggedEvents = new AccumulatingProgress<IBinaryMessage>(buildEvents.ToProgress());
			var errorList = new ErrorListAdapter(id, loggedEvents);
			var textWriter = new TextWriterAdapter(id, loggedEvents);

			Log = new Log(errorList, textWriter);
			Messages = buildEvents;
		}

		public Log Log { get; private set; }
		public IObservable<IBinaryMessage> Messages { get; private set; }
	}
}