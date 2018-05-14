using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Outracks.IO
{
	public partial class RelativeFilePath : IRelativePath, IFilePath, IEquatable<RelativeFilePath>
	{
		public RelativeDirectoryPath BasePath { get; private set; }
		
		public FileName Name { get; private set; }

		internal RelativeFilePath(FileName name, RelativeDirectoryPath basePath = null)
		{
			BasePath = basePath;
			Name = name;
		}

		public IEnumerable<IName> Parts
		{
			get
			{
				return BasePath == null 
					? new [] { (IName)Name }
					: BasePath.Parts.ConcatOne((IName)Name);
			}
		}

		public override bool Equals(object obj)
		{
			var other = obj as RelativeFilePath;
			return other != null && Equals(other);
		}

		public bool Equals(RelativeFilePath other)
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

		public static bool operator ==(RelativeFilePath a, RelativeFilePath b)
		{
			return ReferenceEquals(a, b) || (!ReferenceEquals(a, null) && !ReferenceEquals(b, null) && a.Equals(b));
		}

		public static bool operator !=(RelativeFilePath a, RelativeFilePath b)
		{
			return !(a == b);
		}
	}
}