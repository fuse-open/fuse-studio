using System;
using System.IO;
using System.Linq;

namespace Outracks.IO
{
	public class DirectoryName : IEquatable<DirectoryName>, IName, IComparable<DirectoryName>
	{
		public static DirectoryName ParentDirectory = new DirectoryName("..");
		public static DirectoryName CurrentDirectory = new DirectoryName(".");

		public static IValidationResult<DirectoryName> Validate(string directoryName)
		{
			if (string.IsNullOrEmpty(directoryName))
				return ValidationResult.Invalid<DirectoryName>("directory name is null or empty");

			if (directoryName.Any(Path.GetInvalidPathChars().Contains))
				return ValidationResult.Invalid<DirectoryName>("directory name contains invalid characters");

			return ValidationResult.Valid(new DirectoryName(directoryName));
		}

		public static DirectoryName GetRandomDirectoryName()
		{
			return new DirectoryName(Path.GetRandomFileName());
		}

		readonly string _string;

		public DirectoryName(string directoryName)
		{
			_string = directoryName;
		}

		public bool IsParentDirectory
		{
			get { return this == ParentDirectory; }
		}

		public bool IsCurrentDirectory
		{
			get { return this == CurrentDirectory; }
		}

		public DirectoryName Add(string suffix)
		{
			return new DirectoryName(_string + suffix);
		}

		public static implicit operator DirectoryName(string str)
		{
			return new DirectoryName(str);
		}

		public override bool Equals(object obj)
		{
			var other = obj as DirectoryName;
			return other != null && Equals(other);
		}

		public bool Equals(DirectoryName other)
		{
			return _string == other._string;
		}

		public override int GetHashCode()
		{
			return (_string != null ? _string.GetHashCode() : 0);
		}

		public int CompareTo(DirectoryName other)
		{
			return _string.CompareTo(other._string);
		}

		public override string ToString()
		{
			return _string;
		}

		public static bool operator ==(DirectoryName a, DirectoryName b)
		{
			return ReferenceEquals(a, b) || (!ReferenceEquals(a, null) && !ReferenceEquals(b, null) && a.Equals(b));
		}

		public static bool operator !=(DirectoryName a, DirectoryName b)
		{
			return !(a == b);
		}
	}
}