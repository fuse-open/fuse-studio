using System;
using System.Reactive.Subjects;
using Outracks;

namespace Fuse.Preview
{
	public class SimulatorHost
	{
		readonly Action<IBinaryMessage> _send;

		public IConnectableObservable<IBinaryMessage> Messages { get; private set; }

		public void Send(IBinaryMessage message)
		{
			_send(message);
		}

		public SimulatorHost(Action<IBinaryMessage> send, IConnectableObservable<IBinaryMessage> messages)
		{
			_send = send;
			Messages = messages;
		}
	}
}