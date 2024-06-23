using System.Linq;
using Outracks.IO;

namespace Outracks.Fuse.Templates
{
	public static class DirectoryToNamespace
	{
		public static Optional<NamespaceName> ToNamespace(this RelativeDirectoryPath directoryPath, Optional<NamespaceName> rootNamespace)
		{
			if (directoryPath == null)
				return rootNamespace;

			var relativeNamespace = new NamespaceName(
				directoryPath.Parts
					.Select(p => p.ToString()
						.Replace(".", "_")
						.Replace(" ", "_")).Join("."));

			return rootNamespace.MatchWith(
				some: root => new NamespaceName(root.FullName + (root.FullName == "" ? "" : ".") + relativeNamespace.FullName),
				none: () => relativeNamespace);
		}
	}
}