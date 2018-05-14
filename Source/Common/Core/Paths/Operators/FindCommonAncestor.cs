using System;
using System.Linq;

namespace Outracks.IO
{
	public partial class AbsoluteDirectoryPath
	{
		public static AbsoluteDirectoryPath FindCommonAncestor(AbsoluteDirectoryPath path1, AbsoluteDirectoryPath path2)
		{
			var orderedPaths = OrderByDepth(path1, path2);
			var balancedPaths = Balance(orderedPaths);
			if (balancedPaths == null) 
				return null;

			return FindCommonAncestorBalanced(balancedPaths.Item1, balancedPaths.Item2);
		}

		static Tuple<AbsoluteDirectoryPath, AbsoluteDirectoryPath> Balance(OrderedNodes paths)
		{
			var tmpPath = paths.Longest;
			var tmpDepth = paths.LongestDepth;
			while (paths.ShortestDepth < tmpDepth)
			{
				if (tmpPath == null)
					return null;
				tmpPath = tmpPath.ContainingDirectory;
				tmpDepth--;
			}
			return Tuple.Create(paths.Shortest, tmpPath);
		}

		static OrderedNodes OrderByDepth(AbsoluteDirectoryPath left, AbsoluteDirectoryPath right)
		{
			int leftDepth = left.Depth;
			int rightDepth = right.Depth;

			return leftDepth <= rightDepth
				? new OrderedNodes(left, leftDepth, right, rightDepth)
				: new OrderedNodes(right, rightDepth, left, leftDepth);
		}

		static AbsoluteDirectoryPath FindCommonAncestorBalanced(AbsoluteDirectoryPath left, AbsoluteDirectoryPath right)
		{
			while (left != null && right != null)
			{
				if (left == right)
					return left;
				left = left.ContainingDirectory;
				right = right.ContainingDirectory;
			}

			return null;
		}

		int Depth
		{
			get { return Parts.Count(); }
		}
	}

	struct OrderedNodes
	{
		public readonly AbsoluteDirectoryPath Shortest;
		public readonly AbsoluteDirectoryPath Longest;
		public readonly int ShortestDepth;
		public readonly int LongestDepth;
		public OrderedNodes(AbsoluteDirectoryPath shortest, int shortestDepth, AbsoluteDirectoryPath longest, int longestDepth)
		{
			Shortest = shortest;
			ShortestDepth = shortestDepth;
			Longest = longest;
			LongestDepth = longestDepth;
		}
	}

}