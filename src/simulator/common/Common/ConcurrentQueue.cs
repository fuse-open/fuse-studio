using Uno;
using Uno.Collections;

using Uno.Threading;

namespace Outracks.Simulator
{
	public class ConcurrentQueue<T>
	{
		readonly Queue<T> _queue;
		readonly object _mutex;

		public ConcurrentQueue()
		{
			_queue = new Queue<T>();
			_mutex = new object();
		}

		public void Enqueue(T data)
		{
			lock (_mutex)
				_queue.Enqueue(data);
		}

		public bool TryDequeue(out T data)
		{
			bool result = false;
			data = default(T);
			lock (_mutex)
			{
				if (_queue.Count != 0)
				{
					data = _queue.Dequeue();
					result = true;
				}
			}
			return result;
		}
	}
}
