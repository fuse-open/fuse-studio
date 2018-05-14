using System.Collections.Generic;
using System.Linq;

namespace Outracks.IO
{
	public static class IsRootedInExtension
	{
		public static bool SharesRootWith(this IAbsolutePath self, IAbsolutePath other)
		{
			return Equals(self.GetRoot(), other.GetRoot());
		}

		public static IAbsolutePath GetRoot(this IAbsolutePath self)
		{
			while (true)
			{
				if (self.ContainingDirectory == null)
					return self;

				self = self.ContainingDirectory;
			}
		}

		public static bool IsRootOf(this IAbsolutePath self, IAbsolutePath other)
		{
			return IsRootedIn(other, self);
		}

		public static bool IsRootedIn(this IAbsolutePath self, IAbsolutePath other)
		{
			return !Equals(self, other) && IsOrIsRootedIn(self, other);
		}

		public static bool IsOrIsRootedIn(this IAbsolutePath self, IAbsolutePath other)
		{
			return
				Equals(self, other) ||
				(self.ContainingDirectory != null && self.ContainingDirectory.IsOrIsRootedIn(other));
		}


		public static IEnumerable<IAbsolutePath> FindRoots(this IEnumerable<IAbsolutePath> paths)
		{
			return paths.Where(path => paths.None(other => other.IsRootOf(path)));
		}
	}
}