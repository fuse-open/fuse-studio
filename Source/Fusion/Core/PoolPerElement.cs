using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Outracks.Fusion
{

	public static partial class Control
	{
		public static IObservable<IImmutableList<TItem>> PoolPerElement<TOwner, TItem>(
			this IObservable<IEnumerable<TOwner>> owners,
			Func<int, IObservable<TOwner>, TItem> createNewItem)
		{
			return Observable.Defer(() =>
			{
				var pool = ImmutableList<Tuple<BehaviorSubject<TOwner>, TItem>>.Empty;

				return owners.Select(o =>
				{
					var ownersArray = o.ToArray();

					var split = Math.Min(ownersArray.Length, pool.Count);

					for (int i = 0; i < split; i++)
					{
						pool[i].Item1.OnNext(ownersArray[i]);
					}

					for (int i = split; i < ownersArray.Length; i++)
					{
						var owner = new BehaviorSubject<TOwner>(ownersArray[i]);
						var item = createNewItem(i, owner.DistinctUntilChanged());
						pool = pool.Add(Tuple.Create(owner, item));
					}

					return pool
						.Select(p => p.Item2)
						.Take(ownersArray.Length)
						.ToImmutableList();
				});
			})
			.DistinctUntilChanged(list => list.Count);
		}

		public static IObservable<IImmutableList<TItem>> PoolPerElement<TOwner, TItem>(
			this IObservable<IEnumerable<TOwner>> owners,
			Func<IObservable<TOwner>, TItem> createNewItem)
		{
			return PoolPerElement(owners, (index, owner) => createNewItem(owner));
		}
	}
}
