using System.Collections.Immutable;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Outracks.Fusion.Tests
{
	[TestFixture]
	public class ListChangeTests
	{
		void OrderedCombinations(int length, List<ImmutableList<int>> result)
		{
			if (length == 0)
			{
				result.Add(ImmutableList<int>.Empty);
				return;
			}

			OrderedCombinations(length - 1, result);

			foreach (var list in result.ToList())
			{
				result.Add(list.Add(length));
			}
		}

		/// <summary>
		/// Returns all ordered combinations of elements from the set {1,...,length}
		/// </summary>
		List<ImmutableList<int>> OrderedCombinations(int length)
		{
			var result = new List<ImmutableList<int>>();
			OrderedCombinations(length, result);
			return result;
		}

		[Test]
		public void IncrementalUpdate_returns_None_on_identical_lists()
		{
			var lists = OrderedCombinations(4);

			foreach (var list in lists)
			{
				Assert.IsFalse(ListChange.IncrementalChanges(list, list).HasValue);
			}
		}

		[Test]
		public void Applying_IncrementalUpdate_returns_correct_result()
		{
			var lists = OrderedCombinations(5);

			foreach (var oldList in lists)
			foreach (var newList in lists)
			{
				var maybeChanges = ListChange.IncrementalChanges(oldList, newList);

				var result = oldList;

				foreach (var changes in maybeChanges)
				{
					result = changes.Apply(result);
				}
				CollectionAssert.AreEqual(newList, result);
			}
		}

		[Test]
		public void Applying_IncrementalUpdate_on_List_returns_correct_result()
		{
			var lists = OrderedCombinations(5);

			foreach (var oldList in lists)
			foreach (var newList in lists)
			{
				var maybeChanges = ListChange.IncrementalChanges(oldList, newList);

				var result = oldList.ToList();

				foreach (var changes in maybeChanges)
				{
					changes.Apply(result);
				}
				CollectionAssert.AreEqual(newList, result);
			}
		}

		int CountChanges<T>(ListChange<T> changes)
		{
			var count = 0;
			changes(
				insert: (index, item) => ++count,
				replace: (index, item) => ++count,
				remove: index => ++count,
				clear: () => ++count);
			return count;
		}

		[Test]
		public void IncrementalUpdate_makes_correct_number_of_changes()
		{
			Assert.AreEqual(
				2,
				CountChanges(
					ListChange.IncrementalChanges(
						new[] { 1, 2, 3 },
						new[] { 1, 2, 3, 4, 5 }).Or(ListChange.None<int>())));

			Assert.AreEqual(
				2,
				CountChanges(
					ListChange.IncrementalChanges(
						new[] { 1, 2, 5 },
						new[] { 1, 2, 3, 4, 5 }).Or(ListChange.None<int>())));

			Assert.AreEqual(
				2,
				CountChanges(
					ListChange.IncrementalChanges(
						new[] { 1, 2, 3, 4, 5 },
						new[] { 1, 2, 3 }).Or(ListChange.None<int>())));

			Assert.AreEqual(
				2,
				CountChanges(
					ListChange.IncrementalChanges(
						new[] { 1, 2, 3, 4, 5 },
						new[] { 1, 2, 5 }).Or(ListChange.None<int>())));
		}
	}
}