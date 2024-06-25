using System;
using System.Collections.Generic;

namespace Outracks.Fusion
{
	public static partial class ObservableList
	{
		public static IObservableList<T> SeparateBy<T>(this IObservableList<T> self, Func<T> separator)
		{
			return self
				.SelectWithState(
					0,
					(changes, res) =>
					{
						var newChanges = new List<ListChange<T>>();
						var count = res;
						changes(
							insert: (i, item) =>
							{
								if (i == count) // inserting at the end
								{
									if (count == 0) // inserting first element: skip the separator
									{
										newChanges.Add(ListChange.Insert(0, item));
									}
									else // add separator before item
									{
										newChanges.Add(ListChange.Insert(i * 2 - 1, separator()));
										newChanges.Add(ListChange.Insert(i * 2, item));
									}
								}
								else // add separator after item
								{
									newChanges.Add(ListChange.Insert(i * 2, item));
									newChanges.Add(ListChange.Insert(i * 2 + 1, separator()));
								}
								++count;
							},
							replace: (i, item) =>
							{
								newChanges.Add(ListChange.Replace(i * 2, item));
							},
							remove: i =>
							{
								if (i == count - 1) // removing at the end
								{
									if (i == 0) // removing first element: no separator to remove
									{
										newChanges.Add(ListChange.Remove<T>(0));
									}
									else  // remove separator before item
									{
										newChanges.Add(ListChange.Remove<T>(i * 2));
										newChanges.Add(ListChange.Remove<T>(i * 2 - 1));
									}
								}
								else // remove separator after item
								{
									newChanges.Add(ListChange.Remove<T>(i * 2 + 1));
									newChanges.Add(ListChange.Remove<T>(i * 2));
								}
								--count;
							},
							clear: () =>
							{
								newChanges.Add(ListChange.Clear<T>());
								count = 0;
							});

						return SelectResult.Create(ListChange.Combine(newChanges), count);
					})
				.UnsafeAsObservableList();
		}
	}
}