using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;

namespace Outracks.Fusion
{
	public static partial class ObservableList
	{
		public static IObservableList<T> Where<T>(this IObservableList<T> self, Predicate<T> predicate)
		{
			return new WhereObservableList<T>(self, predicate);
		}

		public static IObservableList<T> Where<T>(this IObservableList<T> self, Func<T, IObservable<bool>> predicate)
		{
			return self
				.Select(item => predicate(item).Select(p => new { Item = item, Pred = p }))
				.Switch()
				.Where(x => x.Pred)
				.Select(x => x.Item);
		}

		public static IObservableList<T> NotNone<T>(this IObservableList<Optional<T>> self)
		{
			return self.Where(x => x.HasValue).Select(x => x.Value);
		}
	}

	class WhereObservableList<T> : IObservableList<T>
	{
		readonly IObservableList<T> _source;
		readonly Predicate<T> _predicate;

		public WhereObservableList(IObservableList<T> source, Predicate<T> predicate)
		{
			_source = source;
			_predicate = predicate;
		}

		public IDisposable Subscribe(IObserver<ListChange<T>> observer)
		{
			var mapper = new WhereChangeMapper();
			var gate = new object();
			return _source.Subscribe(
				Observer.Create<ListChange<T>>(
					onNext: change =>
					{
						lock (gate)
						{
							var newChanges = new List<ListChange<T>>();

							change(
								insert: (index, item) => newChanges.AddRange(mapper.Insert(index, item, _predicate(item))),
								replace: (index, item) => newChanges.AddRange(mapper.Replace(index, item, _predicate(item))),
								remove: index => newChanges.AddRange(mapper.Remove<T>(index)),
								clear: () => newChanges.AddRange(mapper.Clear<T>()));

							if (newChanges.Count > 0)
								observer.OnNext(ListChange.Combine(newChanges));
						}
					},
					onCompleted: observer.OnCompleted,
					onError: observer.OnError));
		}
	}

	class WhereChangeMapper
	{
		SumTree<int> _items = SumTree<int>.Empty(0, (x, y) => x + y);

		public Optional<ListChange<T>> Insert<T>(int index, T item, bool predicateTrue)
		{
			var toIndex = _items.Sum(index);

			_items = _items.Insert(index, predicateTrue ? 1 : 0);

			return predicateTrue
				? ListChange.Insert(toIndex, item)
				: Optional.None<ListChange<T>>();
		}

		public void InsertFalse(int index)
		{
			_items = _items.Insert(index, 0);
		}

		public Optional<ListChange<T>> Replace<T>(int index, T item, bool predicateTrue)
		{
			var toIndex = _items.Sum(index);

			var oldPredicateTrue = _items[index] == 1;

			_items = _items.ReplaceAt(index, predicateTrue ? 1 : 0);

			if (predicateTrue)
			{
				return oldPredicateTrue
					? ListChange.Replace(toIndex, item)
					: ListChange.Insert(toIndex, item);
			}
			else
			{
				return oldPredicateTrue
					? ListChange.Remove<T>(toIndex)
					: Optional.None<ListChange<T>>();
			}
		}

		public Optional<ListChange<T>> ReplaceFalse<T>(int index)
		{
			var toIndex = _items.Sum(index);

			var oldPredicateTrue = _items[index] == 1;

			_items = _items.ReplaceAt(index, 0);

			return oldPredicateTrue
				? ListChange.Remove<T>(toIndex)
				: Optional.None<ListChange<T>>();
		}

		public Optional<ListChange<T>> Remove<T>(int index)
		{
			var toIndex = _items.Sum(index);
			var predicateTrue = _items[index] == 1;

			_items = _items.RemoveAt(index);

			return predicateTrue
				? ListChange.Remove<T>(toIndex)
				: Optional.None<ListChange<T>>();
		}

		public Optional<ListChange<T>> Clear<T>()
		{
			var oldTrueCount = _items.Sum();
			_items = _items.Clear();

			return oldTrueCount == 0
				? Optional.None<ListChange<T>>()
				: ListChange.Clear<T>();
		}
	}
}