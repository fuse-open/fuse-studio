using System;
using System.Collections.Immutable;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Outracks.Fusion.Tests
{
	[TestFixture]
	public class SumTreeTests
	{
		// Lists are the free monoid, so if stuff works for lists it works for any monoid
		SumTree<ImmutableList<T>> Empty<T>()
		{
			return SumTree<ImmutableList<T>>.Empty(ImmutableList<T>.Empty, (x, y) => x.Concat(y).ToImmutableList());
		}

		SumTree<ImmutableList<T>> FromEnumerable<T>(IEnumerable<T> items)
		{
			var result = Empty<T>();
			foreach (var item in items)
				result = result.Insert(result.Count, new [] { item }.ToImmutableList());
			return result;
		}

		readonly IReadOnlyList<int>[] _testLists =
		{
			new int[] { },
			new [] { 1 },
			new [] { 1, 2 },
			new [] { 1, 2, 3 },
		};

		[Test]
		public void Sum_of_Clear_returns_unit()
		{
			foreach (var list in _testLists)
			{
				CollectionAssert.AreEqual(ImmutableList<int>.Empty, FromEnumerable(list).Clear().Sum());
			}
		}

		[Test]
		public void Get_matches_list()
		{
			foreach (var list in _testLists)
			{
				var tree = FromEnumerable(list);
				for (var i = 0; i < list.Count; ++i)
				{
					CollectionAssert.AreEqual(new[] { list[i] }.ToImmutableList(), tree[i]);
				}
			}
		}

		[Test]
		public void Insert_matches_list()
		{
			var item = new[] { 0 }.ToImmutableList();
			foreach (var list in _testLists)
			{
				var tree = FromEnumerable(list);
				for (var index = 0; index <= list.Count; ++index)
				{
					var mlist = list.ToList();
					mlist.Insert(index, 0);
					var mtree = tree.Insert(index, item);
					for (var i = 0; i < mlist.Count; ++i)
						CollectionAssert.AreEqual(new[] { mlist[i] }.ToImmutableList(), mtree[i]);
				}
			}
		}

		[Test]
		public void Sum_after_Insert_matches_list()
		{
			var item = new[] { 10 }.ToImmutableList();
			foreach (var list in _testLists)
			{
				var tree = FromEnumerable(list);
				for (var index = 0; index <= list.Count; ++index)
				{
					var mlist = list.ToList();
					mlist.Insert(index, 10);
					var mtree = tree.Insert(index, item);

					CollectionAssert.AreEqual(mlist.ToImmutableList(), mtree.Sum());
				}
			}
		}

		[Test]
		public void RemoveAt_matches_list()
		{
			foreach (var list in _testLists)
			{
				var tree = FromEnumerable(list);
				for (var index = 0; index < list.Count; ++index)
				{
					var mlist = list.ToList();
					mlist.RemoveAt(index);
					var mtree = tree.RemoveAt(index);
					for (var i = 0; i < mlist.Count; ++i)
						CollectionAssert.AreEqual(new[] { mlist[i] }.ToImmutableList(), mtree[i]);
				}
			}
		}

		[Test]
		public void Sum_after_RemoveAt_matches_list()
		{
			foreach (var list in _testLists)
			{
				var tree = FromEnumerable(list);
				for (var index = 0; index < list.Count; ++index)
				{
					var mlist = list.ToList();
					mlist.RemoveAt(index);
					var mtree = tree.RemoveAt(index);

					CollectionAssert.AreEqual(mlist.ToImmutableList(), mtree.Sum());
				}
			}
		}

		[Test]
		public void ReplaceAt_matches_list()
		{
			var item = new[] { 10 }.ToImmutableList();
			foreach (var list in _testLists)
			{
				var tree = FromEnumerable(list);
				for (var index = 0; index < list.Count; ++index)
				{
					var mlist = list.ToList();
					mlist[index] = 10;
					var mtree = tree.ReplaceAt(index, item);
					for (var i = 0; i < mlist.Count; ++i)
						CollectionAssert.AreEqual(new[] { mlist[i] }.ToImmutableList(), mtree[i]);
				}
			}
		}

		[Test]
		public void Sum_after_ReplaceAt_matches_list()
		{
			var item = new[] { 10 }.ToImmutableList();
			foreach (var list in _testLists)
			{
				var tree = FromEnumerable(list);
				for (var index = 0; index < list.Count; ++index)
				{
					var mlist = list.ToList();
					mlist[index] = 10;
					var mtree = tree.ReplaceAt(index, item);

					CollectionAssert.AreEqual(mlist.ToImmutableList(), mtree.Sum());
				}
			}
		}

		[Test]
		public void Prefix_sum_matches_list_prefix_sum()
		{
			foreach (var list in _testLists)
			{
				var tree = FromEnumerable(list);
				for (var index = 0; index < list.Count; ++index)
				{
					CollectionAssert.AreEqual(list.Take(index).ToImmutableList(), tree.Sum(index));
				}
			}
		}

		[Test]
		public void out_of_bounds_operations_throw()
		{
			var item = new[] { 10 }.ToImmutableList();
			foreach (var list in _testLists)
			{
				var tree = FromEnumerable(list);
				Assert.Throws<IndexOutOfRangeException>(() => tree.Insert(-1, item));
				Assert.Throws<IndexOutOfRangeException>(() => tree.Insert(tree.Count + 1, item));
				Assert.Throws<IndexOutOfRangeException>(() => tree.RemoveAt(-1));
				Assert.Throws<IndexOutOfRangeException>(() => tree.RemoveAt(tree.Count));
				Assert.Throws<IndexOutOfRangeException>(() => tree.ReplaceAt(-1, item));
				Assert.Throws<IndexOutOfRangeException>(() => tree.ReplaceAt(tree.Count, item));
				Assert.Throws<IndexOutOfRangeException>(() => tree.Sum(-1));
				Assert.Throws<IndexOutOfRangeException>(() => tree.Sum(tree.Count + 1));
				Assert.Throws<IndexOutOfRangeException>(() =>
				{
					var unused = tree[-1];
				});
				Assert.Throws<IndexOutOfRangeException>(() =>
				{
					var unused = tree[tree.Count];
				});
			}
		}
	}
}