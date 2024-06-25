using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Outracks.IO
{
	public partial class AbsoluteDirectoryPath : IAbsolutePath, IDirectoryPath, IEquatable<AbsoluteDirectoryPath>
	{
		public AbsoluteDirectoryPath ContainingDirectory { get; private set; }

		public DirectoryName Name { get; private set; }

		string IAbsolutePath.Name {  get { return Name.ToString(); } }

		internal AbsoluteDirectoryPath(DirectoryName name, AbsoluteDirectoryPath basePath = null)
		{
			Name = name;
			ContainingDirectory = basePath;
		}

		public IEnumerable<DirectoryName> Parts
		{
			get
			{
				return ContainingDirectory == null
					? new[] { Name }
					: ContainingDirectory.Parts.ConcatOne(Name);
			}
		}

		public string NativePath
		{
			get { return Path.Combine(Parts.Select(p => p.ToString()).ToArray()); }
		}

		public override bool Equals(object obj)
		{
			var other = obj as AbsoluteDirectoryPath;
			return other != null && Equals(other);
		}

		public bool Equals(AbsoluteDirectoryPath other)
		{
			return ContainingDirectory == other.ContainingDirectory && Name == other.Name;
		}

		public override int GetHashCode()
		{
			return (ContainingDirectory == null ? 0 : ContainingDirectory.GetHashCode()) ^ Name.GetHashCode();
		}

		public override string ToString()
		{
			return ContainingDirectory == null ? Name.ToString() : Path.Combine(ContainingDirectory.ToString(), Name.ToString());
		}

		public static bool operator ==(AbsoluteDirectoryPath a, AbsoluteDirectoryPath b)
		{
			return ReferenceEquals(a, b) || (!ReferenceEquals(a, null) && !ReferenceEquals(b, null) && a.Equals(b));
		}

		public static bool operator !=(AbsoluteDirectoryPath a, AbsoluteDirectoryPath b)
		{
			return !(a == b);
		}
	}
}
