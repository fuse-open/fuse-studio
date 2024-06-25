using System;
using Outracks.Fusion.Fenwick;

namespace Outracks.Fusion
{
	public partial class SumTree<T>
	{
		/// <summary>
		/// Return a new tree with no nodes and the same operations as this tree. O(1).
		/// </summary>
		public SumTree<T> Clear()
		{
			return Empty(_unit, _operation);
		}

		/// <summary>
		/// Get the element at index. O(log n).
		/// </summary>
		public T this[int index]
		{
			get
			{
				if (index < 0 || index >= Count)
					throw new IndexOutOfRangeException("index");

				return _tree.MatchWith(tree => GetInner(0, tree, index),
					() =>
					{
						throw new Exception("SumTree.Get: Impossible");
					});
			}
		}

		static T GetInner(int leftIndex, Tree<T> tree, int index)
		{
			return tree.Match(
				leaf: leafValue => leafValue,
				node: (left, leftLength, nodeValue, right) =>
				{
					var rightIndex = leftIndex + leftLength;
					return index < rightIndex
						? GetInner(leftIndex, left, index)
						: GetInner(rightIndex, right, index);
				});
		}

		/// <summary>
		/// Return a new tree with value inserted at index. O(log n).
		/// </summary>
		public SumTree<T> Insert(int index, T value)
		{
			if (index < 0 || index > Count)
				throw new IndexOutOfRangeException("index");

			return _tree.MatchWith(
				tree => NonEmpty(InsertInner(0, tree, Count, index, value), Count + 1),
				() => NonEmpty(Tree.Leaf(value), 1));
		}

		Tree<T> InsertInner(int leftIndex, Tree<T> tree, int length, int index, T value)
		{
			return tree.Match(
				leaf: leafValue => index == leftIndex
					? Tree.Node(
						Tree.Leaf(value),
						1,
						_operation(value, leafValue),
						tree)
					: Tree.Node(
						tree,
						1,
						_operation(leafValue, value),
						Tree.Leaf(value)),
				node: (left, leftLength, nodeValue, right) =>
				{
					var rightLength = length - leftLength;
					var rightIndex = leftIndex + leftLength;
					if (index < rightIndex)
					{
						var newLeft = InsertInner(leftIndex, left, leftLength, index, value);
						return BalanceNode(newLeft, leftLength + 1, right, rightLength);
					}
					else
					{
						var newRight = InsertInner(rightIndex, right, rightLength, index, value);
						return BalanceNode(left, leftLength, newRight, rightLength + 1);
					}
				});
		}

		/// <summary>
		/// Return a new tree with the value at index removed. O(log n).
		/// </summary>
		public SumTree<T> RemoveAt(int index)
		{
			if (index < 0 || index >= Count)
				throw new IndexOutOfRangeException("index");

			return _tree.MatchWith(
				tree => FromOptional(RemoveAtInner(0, tree, Count, index), Count - 1),
				() =>
				{
					throw new Exception("SumTree.Remove: Impossible");
				});
		}

		Optional<Tree<T>> RemoveAtInner(int leftIndex, Tree<T> tree, int length, int index)
		{
			return tree.Match(
				leaf: leafValue => Optional.None<Tree<T>>(),
				node: (left, leftLength, nodeValue, right) =>
				{
					var rightLength = length - leftLength;
					var rightIndex = leftIndex + leftLength;
					if (index < rightIndex)
					{
						var maybeNewLeft = RemoveAtInner(leftIndex, left, leftLength, index);
						return maybeNewLeft.MatchWith(
							newLeft => BalanceNode(newLeft, leftLength + 1, right, rightLength),
							() => right);
					}
					else
					{
						var maybeNewRight = RemoveAtInner(rightIndex, right, rightLength, index);
						return maybeNewRight.MatchWith(
							newRight => BalanceNode(left, leftLength, newRight, rightLength + 1),
							() => left);
					}
				});
		}

		/// <summary>
		/// Return a new tree with the value at index replaced with value. O(log n).
		/// </summary>
		public SumTree<T> ReplaceAt(int index, T value)
		{
			if (index < 0 || index >= Count)
				throw new IndexOutOfRangeException("index");

			return _tree.MatchWith(
				tree => NonEmpty(ReplaceAtInner(0, tree, Count, index, value), Count),
				() =>
				{
					throw new Exception("SumTree.ReplaceAt: Impossible");
				});
		}

		Tree<T> ReplaceAtInner(int leftIndex, Tree<T> tree, int length, int index, T value)
		{
			return tree.Match(
				leaf: leafValue => Tree.Leaf(value),
				node: (left, leftLength, nodeValue, right) =>
				{
					var rightLength = length - leftLength;
					var rightIndex = leftIndex + leftLength;
					if (index < rightIndex)
					{
						var newLeft = ReplaceAtInner(leftIndex, left, leftLength, index, value);
						return Tree.Node(newLeft, leftLength, _operation(newLeft.Value, right.Value), right);
					}
					else
					{
						var newRight = ReplaceAtInner(rightIndex, right, rightLength, index, value);
						return Tree.Node(left, leftLength, _operation(left.Value, newRight.Value), newRight);
					}
				});
		}

		/// <summary>
		/// Get the sum of all the elements in this tree. O(1).
		/// </summary>
		public T Sum()
		{
			return _tree.MatchWith(tree => tree.Value, () => _unit);
		}

		/// <summary>
		/// Get the prefix sum up to (but not including) index. O(log n).
		/// </summary>
		public T Sum(int index)
		{
			if (index < 0 || index > Count)
				throw new IndexOutOfRangeException("index");

			return _tree.MatchWith(
				tree => SumInner(0, tree, Count, index),
				() => _unit);
		}

		T SumInner(int leftIndex, Tree<T> tree, int length, int index)
		{
			return tree.Match(
				leaf: leafValue => index == leftIndex
					? _unit
					: leafValue,
				node: (left, leftLength, nodeValue, right) =>
				{
					var rightIndex = leftIndex + leftLength;
					var rightLength = length - leftLength;

					if (index == leftIndex + length)
						return nodeValue;
					else if (index < rightIndex)
						return SumInner(leftIndex, left, leftLength, index);
					else
						return _operation(left.Value, SumInner(rightIndex, right, rightLength, index));

				});
		}

		// Create a new balanced node from the left and right subtrees. O(1).
		//
		// As long as the left and right subtrees maintain the following invariant,
		// the resulting tree node will also maintain it:
		//
		// rightLength <= leftLength * 2
		// leftLength <= rightLength * 2
		Tree<T> BalanceNode(Tree<T> left, int leftLength, Tree<T> right, int rightLength)
		{
			if (rightLength > leftLength * 2)
				return RotateLeft(left, leftLength, right);
			if (leftLength > rightLength * 2)
				return RotateRight(left, leftLength, right);
			return Tree.Node(left, leftLength, _operation(left.Value, right.Value), right);
		}

		Tree<T> RotateLeft(Tree<T> left, int leftLength, Tree<T> r)
		{
			return r.Match(
				leaf: rightValue => Tree.Node(left, leftLength, _operation(left.Value, rightValue), r),
				node: (mid, midLength, midRightValue, right) =>
				{
					var leftMidLength = leftLength + midLength;
					var leftMidValue = _operation(left.Value, mid.Value);
					return Tree.Node(
						Tree.Node(left, leftLength, leftMidValue, mid),
						leftMidLength,
						_operation(left.Value, midRightValue),
						right);
				});
		}

		Tree<T> RotateRight(Tree<T> l, int lLength, Tree<T> right)
		{
			return l.Match(
				leaf: leftValue => Tree.Node(l, lLength, _operation(leftValue, right.Value), right),
				node: (left, leftLength, leftMidValue, mid) =>
				{
					var leftMidLength = lLength;
					var midLength = leftMidLength - leftLength;
					var midRightValue = _operation(mid.Value, right.Value);
					return Tree.Node(
						left,
						leftLength,
						_operation(leftMidValue, right.Value),
						Tree.Node(mid, midLength, midRightValue, right));
				});
		}
	}
}
