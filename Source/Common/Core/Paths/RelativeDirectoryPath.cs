using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Outracks.IO
{
	public partial class RelativeDirectoryPath : IRelativePath, IDirectoryPath, IEquatable<RelativeDirectoryPath>
	{
		public static RelativeDirectoryPath Empty
		{
			get { return null; } // we are using null for this for now, considering replacing
		} 

		public RelativeDirectoryPath BasePath { get; private set; }

		public DirectoryName Name { get; private set; }

		internal RelativeDirectoryPath(DirectoryName name, RelativeDirectoryPath basePath = null)
		{
			BasePath = basePath;
			Name = name;
		}

		public IEnumerable<DirectoryName> Parts
		{
			get
			{
				return BasePath == null
					? new[] { Name }
					: BasePath.Parts.ConcatOne(Name);
			}
		}

		public override bool Equals(object obj)
		{
			var other = obj as RelativeDirectoryPath;
			return other != null && Equals(other);
		}

		public bool Equals(RelativeDirectoryPath other)
		{
			return BasePath == other.BasePath && Name == other.Name;
		}

		public override int GetHashCode()
		{
			return (BasePath == null ? 0 : BasePath.GetHashCode()) ^ Name.GetHashCode();
		}

		public override string ToString()
		{
			return BasePath == null ? Name.ToString() : BasePath + "/" + Name;
		}

		public string NativeRelativePath
		{
			get { return Path.Combine(Parts.Select(p => p.ToString()).ToArray()); }
		}

		public static bool operator ==(RelativeDirectoryPath a, RelativeDirectoryPath b)
		{
			return ReferenceEquals(a, b) || (!ReferenceEquals(a, null) && !ReferenceEquals(b, null) && a.Equals(b));
		}

		public static bool operator !=(RelativeDirectoryPath a, RelativeDirectoryPath b)
		{
			return !(a == b);
		}
	}
}