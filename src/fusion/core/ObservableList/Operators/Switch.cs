using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Outracks.Fusion
{
	public static partial class ObservableList
	{
		public static IObservableList<T> Switch<T>(this IObservable<IObservableList<T>> self)
		{
			return self.Select(ol => ol.StartWith(ListChange.Clear<T>())).Switch().UnsafeAsObservableList();
		}

		public static IObservableList<T> Switch<T>(this IObservableList<IObservable<T>> self)
		{
			return new SwitchObservableListObservable<T>(self);
		}

		public static IObservableList<TResult> Switch<TSource, TResult>(
			this IObservable<TSource> self,
			Func<TSource, IObservableList<TResult>> func)
		{
			return self.Select(func).Switch();
		}
	}

	class SwitchObservableListObservable<T> : IObservableList<T>
	{
		readonly IObservableList<IObservable<T>> _sources;

		public SwitchObservableListObservable(IObservableList<IObservable<T>> sources)
		{
			_sources = sources;
		}

		public IDisposable Subscribe(IObserver<ListChange<T>> observer)
		{
			return _sources.Subscribe(new Obs(observer));
		}

		class Obs : IObserver<ListChange<IObservable<T>>>
		{
			readonly IObserver<ListChange<T>> _observer;
			readonly IndexTrackerList<SingleAssignmentDisposable> _innerSubscriptions
				= new IndexTrackerList<SingleAssignmentDisposable>();
			readonly WhereChangeMapper _changeMapper
				= new WhereChangeMapper();

			readonly object _gate = new object();

			bool _outerCompleted;
			SumTree<bool> _innerCompleted
				= SumTree<bool>.Empty(false, (d1, d2) => d1 && d2);

			public Obs(IObserver<ListChange<T>> observer)
			{
				_observer = observer;
			}

			public void OnNext(ListChange<IObservable<T>> changes)
			{
				lock (_gate)
				{
					changes(
						insert: (index, obs) =>
						{
							var indexedItem = new IndexTrackerList<SingleAssignmentDisposable>.IndexedItem(new SingleAssignmentDisposable());
							_innerSubscriptions.Insert(index, indexedItem);
							_changeMapper.InsertFalse(index);
							_innerCompleted = _innerCompleted.Insert(index, false);

							indexedItem.Value.Disposable = SubscribeInner(obs, indexedItem);
						},
						replace: (index, obs) =>
						{
							_innerSubscriptions[index].Dispose();
							var indexedItem = new IndexTrackerList<SingleAssignmentDisposable>.IndexedItem(new SingleAssignmentDisposable());
							_innerSubscriptions.Replace(index, indexedItem);
							_innerCompleted = _innerCompleted.ReplaceAt(index, false);

							indexedItem.Value.Disposable = SubscribeInner(obs, indexedItem);
							// The new observable hasn't produced any values left, so remove any leftover old values
							foreach (var change in _changeMapper.ReplaceFalse<T>(index))
								_observer.OnNext(change);
						},
						remove: (index) =>
						{
							_innerSubscriptions[index].Dispose();
							_innerSubscriptions.Remove(index);
							_innerCompleted = _innerCompleted.RemoveAt(index);
							foreach (var change in _changeMapper.Remove<T>(index))
								_observer.OnNext(change);
						},
						clear: () =>
						{
							Disposable.Combine(_innerSubscriptions).Dispose();
							_innerSubscriptions.Clear();
							_innerCompleted = SumTree<bool>.Empty(false, (d1, d2) => d1 && d2);
							foreach (var change in _changeMapper.Clear<T>())
								_observer.OnNext(change);
						});
				}
			}

			IDisposable SubscribeInner(IObservable<T> obs, IndexTrackerList<SingleAssignmentDisposable>.IndexedItem indexedItem)
			{
				return obs.Subscribe(
					Observer.Create<T>(
						onNext: item =>
						{
							lock (_gate)
								foreach (var currentIndex in indexedItem.Index)
									foreach (var change in _changeMapper.Replace(currentIndex, item, true))
										_observer.OnNext(change);
						},
						onError: OnError,
						onCompleted: () =>
						{
							lock (_gate)
							{
								foreach (var currentIndex in indexedItem.Index)
								{
									_innerSubscriptions[currentIndex].Dispose();
									_innerCompleted = _innerCompleted.ReplaceAt(currentIndex, true);
									if (_outerCompleted && _innerCompleted.Sum())
									{
										Disposable.Combine(_innerSubscriptions).Dispose();
										_innerSubscriptions.Clear();
										_observer.OnCompleted();
									}
								}
							}
						}));
			}

			public void OnError(Exception error)
			{
				lock (_gate)
				{
					_observer.OnError(error);
					Disposable.Combine(_innerSubscriptions).Dispose();
					_innerSubscriptions.Clear();
					_innerCompleted = SumTree<bool>.Empty(false, (d1, d2) => d1 && d2);
				}
			}

			public void OnCompleted()
			{
				lock (_gate)
				{
					_outerCompleted = true;
					if (_innerCompleted.Sum())
					{
						Disposable.Combine(_innerSubscriptions).Dispose();
						_innerSubscriptions.Clear();
						_observer.OnCompleted();
					}
				}
			}
		}
	}

	class IndexTrackerList<T> : IEnumerable<T>
	{
		public class IndexedItem
		{
			public Optional<int> Index;
			public T Value;

			public IndexedItem(T value)
			{
				Value = value;
			}
		}

		readonly List<IndexedItem> _items = new List<IndexedItem>();

		public T this[int index]
		{
			get { return _items[index].Value; }
		}

		public void Insert(int index, IndexedItem item)
		{
			_items.Insert(index, item);
			InvalidateFrom(index);
		}

		public void Replace(int index, IndexedItem item)
		{
			_items[index].Index = Optional.None<int>();
			item.Index = index;
			_items[index] = item;
		}

		public void Remove(int index)
		{
			_items[index].Index = Optional.None<int>();
			_items.RemoveAt(index);
			InvalidateFrom(index);
		}

		public void Clear()
		{
			foreach (var item in _items)
				item.Index = Optional.None<int>();
			_items.Clear();
		}

		void InvalidateFrom(int fromIndex)
		{
			for (var i = fromIndex; i < _items.Count; ++i)
				_items[i].Index = i;
		}

		public IEnumerator<T> GetEnumerator()
		{
			return _items.Select(item => item.Value).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}