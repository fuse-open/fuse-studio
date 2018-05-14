using System;
using Outracks.Fusion.Fenwick;

namespace Outracks.Fusion
{
	/// <summary>
	/// An indexed (as in array) immutable data structure that offers log-time
	/// insertion, removal, prefix sum, and constant time total sum of elements.
	///
	/// Parameterised by a domain T and a binary operation on T.
	/// Assumes that the operation is associative and has a unit
	/// (i.e. forms a monoid).
	///
	/// Implemented as a self-balancing Fenwick tree.
	/// </summary>
	public partial class SumTree<T>
	{
		readonly T _unit;
		readonly Func<T, T, T> _operation;
		readonly Optional<Tree<T>> _tree;
		public readonly int Count;

		SumTree(T unit, Func<T, T, T> associativeOperation, Optional<Tree<T>> tree, int count)
		{
			_unit = unit;
			_operation = associativeOperation;
			_tree = tree;
			Count = count;
		}

		public static SumTree<T> Empty(T unit, Func<T, T, T> associativeOperation)
		{
			return new SumTree<T>(unit, associativeOperation, Optional.None<Tree<T>>(), 0);
		}

		SumTree<T> NonEmpty(Tree<T> tree, int length)
		{
			return new SumTree<T>(_unit, _operation, tree, length);
		}

		SumTree<T> FromOptional(Optional<Tree<T>> maybeTree, int length)
		{
			return maybeTree.MatchWith(tree => NonEmpty(tree, length), Clear);
		}
	}

	namespace Fenwick
	{
		class Tree<T>
		{
			public readonly T Value;
			public readonly Optional<Leaves<T>> Leaves;

			public Tree(T value, Optional<Leaves<T>> leaves)
			{
				Value = value;
				Leaves = leaves;
			}

			public TResult Match<TResult>(Func<T, TResult> leaf, Func<Tree<T>, int, T, Tree<T>, TResult> node)
			{
				return Leaves.HasValue
					? node(Leaves.Value.Left, Leaves.Value.LeftLength, Value, Leaves.Value.Right)
					: leaf(Value);
			}
		}

		struct Leaves<T>
		{
			public readonly Tree<T> Left;
			public readonly int LeftLength;
			public readonly Tree<T> Right;

			public Leaves(Tree<T> left, int leftLength, Tree<T> right)
			{
				Left = left;
				LeftLength = leftLength;
				Right = right;
			}
		}

		static class Tree
		{
			public static Tree<T> Leaf<T>(T value)
			{
				return new Tree<T>(value, Optional.None<Leaves<T>>());
			}

			public static Tree<T> Node<T>(Tree<T> left, int leftLength, T value, Tree<T> right)
			{
				return new Tree<T>(value, new Leaves<T>(left, leftLength, right));
			}
		}
	}
}
