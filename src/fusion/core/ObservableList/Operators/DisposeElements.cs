using System;
using System.Collections.Generic;
using System.Reactive;

namespace Outracks.Fusion
{
	public static partial class ObservableList
	{
		public static IObservableList<T> DisposeElements<T>(this IObservableList<T> self) where T : IDisposable
		{
			return self.DisposeElements(x => x);
		}

		public static IObservableList<T> DisposeElements<T>(this IObservableList<T> self, Func<T, IDisposable> getDisposable)
		{
			return new DisposeElementsObservableList<T>(self, getDisposable);
		}
	}

	class DisposeElementsObservableList<T> : IObservableList<T>
	{
		readonly IObservableList<T> _source;
		readonly Func<T, IDisposable> _getDisposable;

		public DisposeElementsObservableList(IObservableList<T> source, Func<T, IDisposable> getDisposable)
		{
			_source = source;
			_getDisposable = getDisposable;
		}

		public IDisposable Subscribe(IObserver<ListChange<T>> observer)
		{
			var gate = new object();
			var items = new List<IDisposable>();
			Action clear = () =>
			{
				Disposable.Combine(items).Dispose();
				items.Clear();
			};

			return Disposable.Combine(
				_source.Subscribe(
					Observer.Create<ListChange<T>>(
						onNext: changes =>
						{
							lock (gate)
							{
								observer.OnNext(changes);
								changes(
									insert: (index, item) => items.Insert(index, _getDisposable(item)),
									remove: index =>
									{
										items[index].Dispose();
										items.RemoveAt(index);
									},
									replace: (index, item) =>
									{
										items[index].Dispose();
										items[index] = _getDisposable(item);
									},
									clear: clear);
							}
						},
						onError: error =>
						{
							lock (gate)
							{
								observer.OnError(error);
								clear();
							}
						},
						onCompleted: () =>
						{
							lock (gate)
								observer.OnCompleted();
						})),
				Disposable.Create(clear));
		}
	}
}