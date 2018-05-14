using System;
using System.Collections.Generic;
using System.Reactive;

namespace Outracks.Fusion
{
	public static partial class ObservableList
	{
		public static IObservableList<T> Concat<T>(this IObservableList<T> left, IObservableList<T> right)
		{
			return new ConcatObservableLists<T>(left, right);
		}
	}

	class ConcatObservableLists<T> : IObservableList<T>
	{
		readonly IObservableList<T> _left;
		readonly IObservableList<T> _right;

		public ConcatObservableLists(IObservableList<T> left, IObservableList<T> right)
		{
			_left = left;
			_right = right;
		}

		public IDisposable Subscribe(IObserver<ListChange<T>> observer)
		{
			return new Subscription(this, observer);
		}

		class Subscription : IDisposable
		{
			readonly IObserver<ListChange<T>> _observer;

			readonly object _gate = new object();
			int _leftCount;
			int _rightCount;
			bool _leftCompleted;
			bool _rightCompleted;

			readonly IDisposable _dispose;

			public Subscription(ConcatObservableLists<T> parent, IObserver<ListChange<T>> observer)
			{
				_observer = observer;
				_dispose = Disposable.Combine(
					parent._left.Subscribe(LeftObserver()),
					parent._right.Subscribe(RightObserver()));
			}

			public void Dispose()
			{
				_dispose.Dispose();
			}

			IObserver<ListChange<T>> LeftObserver()
			{
				return Observer.Create<ListChange<T>>(
					onNext: changes =>
					{
						lock (_gate)
						{
							var newChanges = new List<ListChange<T>>();
							changes(
								insert: (index, item) =>
								{
									newChanges.Add(ListChange.Insert(index, item));
									++_leftCount;
								},
								replace: (index, item) => { newChanges.Add(ListChange.Replace(index, item)); },
								remove: index =>
								{
									newChanges.Add(ListChange.Remove<T>(index));
									--_leftCount;
								},
								clear: () =>
								{
									if (_rightCount == 0)
										newChanges.Add(ListChange.Clear<T>());
									else
										for (var i = _leftCount - 1; i >= 0; --i)
											newChanges.Add(ListChange.Remove<T>(i));

									_leftCount = 0;
								});
							if (newChanges.Count > 0)
								_observer.OnNext(ListChange.Combine(newChanges));
						}
					},
					onCompleted: () =>
					{
						lock (_gate)
						{
							if (_rightCompleted)
								_observer.OnCompleted();

							_leftCompleted = true;
						}
					},
					onError: e =>
					{
						lock (_gate)
							_observer.OnError(e);
					});
			}

			IObserver<ListChange<T>> RightObserver()
			{
				return Observer.Create<ListChange<T>>(
						onNext: changes =>
						{
							lock (_gate)
							{
								var newChanges = new List<ListChange<T>>();
								changes(
									insert: (index, item) =>
									{
										var newIndex = index + _leftCount;
										newChanges.Add(ListChange.Insert(newIndex, item));
										++_rightCount;
									},
									replace: (index, item) =>
									{
										var newIndex = index + _leftCount;
										newChanges.Add(ListChange.Replace(newIndex, item));
									},
									remove: index =>
									{
										var newIndex = index + _leftCount;
										newChanges.Add(ListChange.Remove<T>(newIndex));
										--_rightCount;
									},
									clear: () =>
									{
										if (_leftCount == 0)
											newChanges.Add(ListChange.Clear<T>());
										else
											for (var i = _rightCount - 1; i >= 0; --i)
												newChanges.Add(ListChange.Remove<T>(i + _leftCount));

										_rightCount = 0;
									});
								if (newChanges.Count > 0)
									_observer.OnNext(ListChange.Combine(newChanges));
							}
						},
						onCompleted: () =>
						{
							lock (_gate)
							{
								if (_leftCompleted)
									_observer.OnCompleted();

								_rightCompleted = true;
							}
						},
						onError: e =>
						{
							lock (_gate)
								_observer.OnError(e);
						});
			}
		}
	}
}