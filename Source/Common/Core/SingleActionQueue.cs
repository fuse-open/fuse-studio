using System;
using System.Threading;

namespace Outracks
{
	public class SingleActionQueue
	{
		Optional<Action> _nextAction = Optional.None();
		readonly object _lockObject = new object();
		readonly ManualResetEvent _itemEnqueued = new ManualResetEvent(false);

		public void Enqueue(Action a)
		{
			lock (_lockObject)
			{
				_nextAction = a;
				_itemEnqueued.Set();
			}
		}

		public Action Dequeue()
		{
			while (true)
			{
				_itemEnqueued.WaitOne();
				lock (_lockObject)
				{
					//Multiple threads could have been released from WaitOne(), only let one thread pick the action
					if (_nextAction.HasValue)
					{
						var value = _nextAction.Value;
						_nextAction = Optional.None();
						return value;
					}
				}
			}
		}
	}
}
