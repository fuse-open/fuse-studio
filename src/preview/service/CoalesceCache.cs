using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Outracks;

namespace Fuse.Preview
{
	// This message is sent from Studio, and requests to add a coalesce entry
	public class CoalesceEntry
	{
		/* Unique key for the same type of data, that means if another CoalesceEntry with the same coalesce key exist, the new CoalesceEntry supersede it.
		   So that old entries having that key should be removed. */
		public string CoalesceKey;

		/* The binary stream can be zero which basically means that the entry is removed, however we can't remove it from the registry,
		since clients also need to know if the entry has been removed.*/
		public Optional<IBinaryMessage> BlobData;

		public bool AddFirst;
	}

	// This message is sent from a preview client to request replay from the server from a specific point, zero means from the beginning.
	class RequestReplayFrom
	{
		public int UniqueQueueIdx = 0;
	}

	class CoalesceEntryCacheData
	{
		public CoalesceEntry Entry;
		public int QueueIdx;
	}

	class CoalesceEntryCache
	{
		readonly Dictionary<string, CoalesceEntry> _coalesceEntries = new Dictionary<string, CoalesceEntry>();
		readonly LinkedList<CoalesceEntryCacheData> _coalesceEntryQueue = new LinkedList<CoalesceEntryCacheData>();
		readonly Subject<CoalesceEntryCacheData> _hotEntries = new Subject<CoalesceEntryCacheData>();
		int _queueIdx;

		object _mutex = new object();
		public void Add(CoalesceEntry entry)
		{
			lock(_mutex)
			{
				var node = entry.AddFirst ? _coalesceEntryQueue.First : _coalesceEntryQueue.Last;
				while (node != null)
				{
					var next = entry.AddFirst ? node.Next : node.Previous;
					if (node.Value.Entry.CoalesceKey == entry.CoalesceKey)
					{
						_coalesceEntryQueue.Remove(node);
						break;
					}
					node = next;
				}

				_coalesceEntries[entry.CoalesceKey] = entry;

				var cacheEntry = new CoalesceEntryCacheData()
				{
					Entry = entry,
					QueueIdx = _queueIdx++
				};

				if (entry.AddFirst)
					_coalesceEntryQueue.AddFirst(cacheEntry);
				else
					_coalesceEntryQueue.AddLast(cacheEntry);

				_hotEntries.OnNext(cacheEntry);
			}
		}

		// Unique queue index is basically an index which doesn't change when items before it has been popped.
		public IObservable<CoalesceEntryCacheData> ReplayFrom(int uniqueQueueIdx)
		{
			return Observable.Create<CoalesceEntryCacheData>(observer =>
			{
				lock (_mutex)
				{
					return _coalesceEntryQueue.Where(e => e.QueueIdx > uniqueQueueIdx)
						.ToObservable()
						.Merge(_hotEntries)
						.Subscribe(observer);
				}
			});
		}

		public IObservable<Unit> HasEntry(string coalesceKey)
		{
			return Observable.Create<Unit>(
				observer =>
				{
					lock (_mutex)
					{
						if (_coalesceEntries.ContainsKey(coalesceKey))
						{
							observer.OnNext(Unit.Default);
							return Disposable.Empty;
						}
						else
						{
							return _hotEntries
								.Where(e => e.Entry.CoalesceKey == coalesceKey)
								.Select(e => Unit.Default)
								.FirstAsync()
								.Subscribe(observer);
						}
					}
				});
		}
	}

	static class CoalesceExtensions
	{
		public static CoalesceEntry ToCoalesceEntry(
			this IBinaryMessage message, string coalesceKey, bool addFirst = false)
		{
			return new CoalesceEntry()
			{
				BlobData = Optional.Some(message),
				CoalesceKey = coalesceKey,
				AddFirst = addFirst
			};
		}

		public static IObservable<CoalesceEntry> ToCoalesceEntry(
			this IObservable<IBinaryMessage> message, string coalesceKey, bool addFirst = false)
		{
			return message.Select(
				m => new CoalesceEntry()
				{
					BlobData = Optional.Some(m),
					CoalesceKey = coalesceKey,
					AddFirst = addFirst
				});
		}

		public static IObservable<CoalesceEntry> ToCoalesceEntry(
			this IObservable<Optional<IBinaryMessage>> message, string coalesceKey, bool addFirst = false)
		{
			return message.Select(
				m => new CoalesceEntry()
				{
					BlobData = m,
					CoalesceKey = coalesceKey,
					AddFirst = addFirst
				});
		}
	}
}