using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive;

namespace Outracks.Fusion
{
	public static partial class ObservableList
	{
		public static IConnectableObservableList<T> Replay<T>(this IObservableList<T> self)
		{
			return new ReplayObservableList<T>(self);
		}
	}

	class ReplayObservableList<T> : IConnectableObservableList<T>
	{
		enum State
		{
			Unconnected,
			Connected,
			Error,
			Completed
		}

		readonly object _gate = new object();
		readonly IObservableList<T> _source;

		State _state;
		ImmutableList<IObserver<ListChange<T>>> _observers = ImmutableList<IObserver<ListChange<T>>>.Empty;
		ImmutableList<T> _value = ImmutableList<T>.Empty;
		Exception _error;

		public ReplayObservableList(IObservableList<T> source)
		{
			_source = source;
		}

		public IDisposable Connect()
		{
			lock (_gate)
			{
				if (_state != State.Unconnected)
					throw new Exception("Can't connect an already connected ReplayObservableList");

				_state = State.Connected;

				var subscription = _source.Subscribe(
					Observer.Create<ListChange<T>>(
						onNext: change =>
						{
							lock (_gate)
							{
								_value = change.Apply(_value);

								foreach (var observer in _observers)
									observer.OnNext(change);
							}
						},
						onError: error =>
						{
							lock (_gate)
							{
								_state = State.Error;
								_error = error;

								foreach (var observer in _observers)
									observer.OnError(error);
								_observers = ImmutableList<IObserver<ListChange<T>>>.Empty;
							}
						},
						onCompleted: () =>
						{
							lock (_gate)
							{
								_state = State.Completed;

								foreach (var observer in _observers)
									observer.OnCompleted();
								_observers = ImmutableList<IObserver<ListChange<T>>>.Empty;
							}
						}));

				return Disposable.Combine(
					subscription,
					Disposable.Create(
						() =>
						{
							_state = State.Unconnected;
							_value = ImmutableList<T>.Empty;
							_observers = ImmutableList<IObserver<ListChange<T>>>.Empty;
						}));
			}
		}

		public IDisposable Subscribe(IObserver<ListChange<T>> observer)
		{
			lock (_gate)
			{
				if (_value.Count > 0)
				{
					var changes = ListChange.Combine(_value.Select((item, index) => ListChange.Insert(index, item)));
					observer.OnNext(changes);
				}

				switch (_state)
				{
					case State.Unconnected:
					case State.Connected:
						_observers = _observers.Add(observer);
						return Disposable.Create(() =>
						{
							lock (_gate)
								_observers = _observers.Remove(observer);
						});
					case State.Completed:
						observer.OnCompleted();
						return Disposable.Empty;
					case State.Error:
						observer.OnError(_error);
						return Disposable.Empty;
					default:
						throw new Exception("ReplayObservableList.Subscribe: Impossible");
				}
			}
		}
	}
}