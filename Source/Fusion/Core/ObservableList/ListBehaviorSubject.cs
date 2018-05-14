using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Subjects;

namespace Outracks.Fusion
{
	public class ListBehaviorSubject<T> : IListSubject<T>, IDisposable, IListBehavior<T>
	{
		readonly object _mutex = new object();
		ImmutableList<T> _value;
		public IReadOnlyList<T> Value
		{
			get { return _value; }
		}
		readonly Subject<ListChange<T>> _subject;

		public ListBehaviorSubject()
		{
			_value = ImmutableList<T>.Empty;
			_subject = new Subject<ListChange<T>>();
		}

		public ListBehaviorSubject(IEnumerable<T> value)
			: this()
		{
			var change = ListChange.Combine(value.Select((item, index) => ListChange.Insert(index, item)).ToArray());
			OnNext(change);
		}

		public void OnNext(ListChange<T> change)
		{
			lock (_mutex)
			{
				_value = change.Apply(_value);
				_subject.OnNext(change);
			}
		}

		public void OnInsert(int index, T item)
		{
			OnNext(ListChange.Insert(index, item));
		}

		public T this[int i]
		{
			get { return _value[i]; }
			set { OnNext(ListChange.Replace(i, value)); }
		}

		public void OnAdd(T item)
		{
			lock (_mutex)
				OnNext(ListChange.Insert(_value.Count, item));
		}

		public void OnRemove(int index)
		{
			OnNext(ListChange.Remove<T>(index));
		}

		public void OnReplace(int index, T item)
		{
			OnNext(ListChange.Replace(index, item));
		}

		public void OnClear()
		{
			OnNext(ListChange.Clear<T>());
		}

		public void OnError(Exception error)
		{
			lock (_mutex)
				_subject.OnError(error);
		}

		public void OnCompleted()
		{
			lock (_mutex)
				_subject.OnCompleted();
		}

		public IDisposable Subscribe(IObserver<ListChange<T>> observer)
		{
			lock (_mutex)
			{
				var change = ListChange.Combine(_value.Select((item, index) => ListChange.Insert(index, item)).ToArray());
				observer.OnNext(change);

				return _subject.Subscribe(observer);
			}
		}

		public void Dispose()
		{
			lock (_mutex)
				_subject.Dispose();
		}
	}
}