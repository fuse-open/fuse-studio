using System;
using System.Collections.Generic;
using System.IO;

namespace Outracks.IO
{
	public partial class AbsoluteFilePath : IAbsolutePath, IFilePath, IEquatable<AbsoluteFilePath>
	{
		public AbsoluteDirectoryPath ContainingDirectory { get; private set; }
		
		public FileName Name { get; private set; }
		string IAbsolutePath.Name { get { return Name.ToString(); } }

		internal AbsoluteFilePath(FileName name, AbsoluteDirectoryPath containingDirectory)
		{
			ContainingDirectory = containingDirectory;
			Name = name;
		}

		public IEnumerable<IName> Parts
		{
			get
			{
				return ContainingDirectory == null
					? new[] { (IName)Name }
					: ContainingDirectory.Parts.ConcatOne((IName)Name);
			}
		}
		public string NativePath
		{
			get { return Path.Combine(ContainingDirectory.NativePath, Name.ToString()); }
		}

		public override bool Equals(object obj)
		{
			var other = obj as AbsoluteFilePath;
			return other != null && Equals(other);
		}

		public bool Equals(AbsoluteFilePath other)
		{
			return ContainingDirectory == other.ContainingDirectory && Name == other.Name;
		}

		public override int GetHashCode()
		{
			return (ContainingDirectory == null ? 0 : ContainingDirectory.GetHashCode()) ^ Name.GetHashCode();
		}

		public override string ToString()
		{
			return ContainingDirectory == null ? Name.ToString() : ContainingDirectory + "/" + Name;
		}

		public static bool operator ==(AbsoluteFilePath a, AbsoluteFilePath b)
		{
			return ReferenceEquals(a, b) || (!ReferenceEquals(a, null) && !ReferenceEquals(b, null) && a.Equals(b));
		}

		public static bool operator !=(AbsoluteFilePath a, AbsoluteFilePath b)
		{
			return !(a == b);
		}
	}
}
