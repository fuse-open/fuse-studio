using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Outracks.Fusion
{
	public delegate void ListChange<out T>(Action<int, T> insert, Action<int, T> replace, Action<int> remove, Action clear);

	public static class ListChange
	{
		public static ListChange<T> None<T>()
		{
			return (insert, replace, remove, clear) => { };
		}

		public static ListChange<T> Insert<T>(int index, T item)
		{
			return (insert, replace, remove, clear) => insert(index, item);
		}

		public static ListChange<T> Replace<T>(int index, T item)
		{
			return (insert, replace, remove, clear) => replace(index, item);
		}

		public static ListChange<T> Remove<T>(int index)
		{
			return (insert, replace, remove, clear) => remove(index);
		}

		public static ListChange<T> Clear<T>()
		{
			return (insert, replace, remove, clear) => clear();
		}

		public static ListChange<T> Combine<T>(IEnumerable<ListChange<T>> changes)
		{
			return Combine(changes.ToArray());
		}

		public static ListChange<T> Combine<T>(params ListChange<T>[] changes)
		{
			if (changes.Length == 0)
				return None<T>();
			if (changes.Length == 1)
				return changes[0];
			return (insert, replace, remove, clear) =>
			{
				foreach (var change in changes)
					change(insert, replace, remove, clear);
			};
		}

		public static ListChange<TResult> Select<TSource, TResult>(this ListChange<TSource> self, Func<TSource, TResult> select)
		{
			return (insert, replace, remove, clear) =>
				self(
					insert: (i, x) => insert(i, select(x)),
					replace: (i, x) => replace(i, select(x)),
					remove: remove,
					clear: clear);
		}

		public static string ToString<T>(this ListChange<T> self)
		{
			var results = new List<string>();

			self(
				insert: (index, item) => results.Add("ListChange.Insert(" + index + ", " + item + ")"),
				replace: (index, item) => results.Add("ListChange.Replace(" + index + ", " + item + ")"),
				remove: index => results.Add("ListChange.Remove(" + index + ")"),
				clear: () => results.Add("ListChange.Clear()"));

			if (results.Count == 0)
				return "ListChange.None()";
			if (results.Count == 1)
				return results[0];
			return "ListChange.Combine(" + results.Join(", ") + ")";
		}

		/// <summary>
		/// Get the changes, if any, necessary to perform on oldElements to get it
		/// to the same shape as newList, using the update function to check and
		/// perform updates to the old list elements using the new elements, and
		/// using the create function to create new elements from the new.
		/// </summary>
		/// <param name="oldElements"></param>
		/// <param name="newList"></param>
		/// <param name="update">
		/// Should return true when the T can be updated from TNew and, if necessary,
		/// update (by mutation) the T using TNew
		/// </param>
		/// <param name="create">
		/// Creates a T element from a TNew element in newList for which no matching
		/// element was found in oldElements.
		/// </param>
		public static Optional<ListChange<T>> IncrementalChanges<T, TNew>(
			IEnumerable<T> oldElements,
			IReadOnlyList<TNew> newList,
			Func<T, TNew, bool> update,
			Func<TNew, T> create)
		{
			var oldList = oldElements.ToList();

			if (!oldList.IsEmpty() && newList.IsEmpty())
				return Clear<T>();

			var updates = new BitArray(newList.Count);

			var result = new List<ListChange<T>>();

			{
				var oldIndex = 0;
				var newIndex = 0;
				while (oldIndex < oldList.Count)
				{
					var found = false;
					for (var i = newIndex; i < newList.Count; ++i)
					{
						var didUpdate = update(oldList[oldIndex], newList[i]);

						// Matching element found: Keep it
						if (didUpdate)
						{
							updates[i] = true;
							newIndex = i + 1;
							found = true;
							break;
						}
					}

					if (found)
					{
						++oldIndex;
					}
					else
					{
						// No element found: Remove it
						result.Add(Remove<T>(oldIndex));
						oldList.RemoveAt(oldIndex);
					}
				}
			}

			// Add all new elements
			for (var i = 0; i < updates.Count; ++i)
			{
				if (!updates[i])
				{
					result.Add(Insert(i, create(newList[i])));
				}
			}

			return result.Count > 0
				? Combine(result)
				: Optional.None<ListChange<T>>();
		}

		public static Optional<ListChange<T>> IncrementalChanges<T>(
			IEnumerable<T> oldElements,
			IReadOnlyList<T> newList)
		{
			return IncrementalChanges(oldElements, newList, (x, y) => x.Equals(y), t => t);
		}

		public static ImmutableList<T> Apply<T>(this ListChange<T> changes, ImmutableList<T> list)
		{
			var result = list;
			changes(
				insert: (i, x) => result = result.Insert(i, x),
				replace: (i, x) => result = result.SetItem(i, x),
				remove: i => result = result.RemoveAt(i),
				clear: () => result = ImmutableList<T>.Empty);
			return result;
		}

		public static SumTree<T> Apply<T>(this ListChange<T> changes, SumTree<T> tree)
		{
			var result = tree;
			changes(
				insert: (i, x) => result = result.Insert(i, x),
				replace: (i, x) => result = result.ReplaceAt(i, x),
				remove: i => result = result.RemoveAt(i),
				clear: () => result = result.Clear());
			return result;
		}

		public static void Apply<T>(this ListChange<T> changes, IList<T> list)
		{
			changes(
				insert: list.Insert,
				replace: (i, x) => list[i] = x,
				remove: list.RemoveAt,
				clear: list.Clear);
		}

		public static void ApplyLegacy<T>(this ListChange<T> changes, IList list)
		{
			changes(
				insert: (i, x) => list.Insert(i, x),
				replace: (i, x) => list[i] = x,
				remove: list.RemoveAt,
				clear: list.Clear);
		}
	}
}