using System;
using System.IO;
using System.Linq;

namespace Outracks.IO
{
	public static class AddExtensionExtension
	{
		public static AbsoluteFilePath AddExtension(this AbsoluteFilePath self, string extension)
		{
			return self.ContainingDirectory / self.Name.AddExtension(extension);
		}

		public static RelativeFilePath AddExtension(this RelativeFilePath self, string extension)
		{
			return self.BasePath / self.Name.AddExtension(extension);
		}

		public static AbsoluteFilePath RemoveExtension(this AbsoluteFilePath self, string extension)
		{
			return self.ContainingDirectory / self.Name.RemoveExtension(extension);
		}

		public static RelativeFilePath RemoveExtension(this RelativeFilePath self, string extension)
		{
			return self.BasePath / self.Name.RemoveExtension(extension);
		}
	}


	public class FileName : IEquatable<FileName>, IName, IComparable<FileName>
	{
		readonly string _string;

		public FileName(string fileName)
		{
            if (fileName == null)
                throw new ArgumentException("file name can not be null");

            if (fileName.IsEmpty())
                throw new ArgumentException("file name can not be empty");

            if (fileName.Any(Path.GetInvalidFileNameChars().Contains))
                throw new ArgumentException("file name '" + fileName + "' contains invalid characters");

			_string = fileName;
		}

		public static IValidationResult<FileName> Validate(string fileName)
		{
			if(fileName == null)
				return ValidationResult.Invalid<FileName>("file name can not be null");

			if (fileName.IsEmpty())
				return ValidationResult.Invalid<FileName>("file name can not be empty");

			if (fileName.Any(Path.GetInvalidFileNameChars().Contains))
				return ValidationResult.Invalid<FileName>("file name contains invalid characters");

			return ValidationResult.Valid(new FileName(fileName));
		}

		public static FileName GetRandomFileName()
		{
			return new FileName(Path.GetRandomFileName());
		}

		public string Extension
		{
			get
			{
				var dot = _string.LastIndexOf('.');
				return dot == -1 ? String.Empty : _string.Substring(dot);
			}
		}

		public FileName WithoutExtension
		{
			get
			{
				var dot = _string.LastIndexOf('.');
				return new FileName(dot == -1 ? _string : _string.Substring(0, dot));
			}
		}

		public bool HasExtension(string extension)
		{
			return String.Equals(Extension, EnsureHasDot(extension), StringComparison.CurrentCultureIgnoreCase);
		}

		public FileName StripExtension(string extension)
		{
			return HasExtension(extension)
				? WithoutExtension
				: this;
		}

		public FileName AddExtension(string newExtension)
		{
			return Add(EnsureHasDot(newExtension));
		}

		public FileName RemoveExtension(string oldExtension)
		{
			return new FileName(_string.StripSuffix("." + oldExtension));
		}

		public FileName Add(string suffix)
		{
			return new FileName(_string + suffix);
		}

		public FileName Replace(string what, string with)
		{
			return new FileName(_string.Replace(what, with));
		}

		public bool Contains(string what)
		{
			return _string.Contains(what);
		}

		static string EnsureHasDot(string extension)
		{
			if (extension == null) throw new ArgumentNullException("extension");
			return extension.StartsWith(".") ? extension : "." + extension;
		}

		public override bool Equals(object obj)
		{
			var other = obj as FileName;
			return other != null && Equals(other);
		}

		public bool Equals(FileName other)
		{
			return _string == other._string;
		}

		public override int GetHashCode()
		{
			return (_string != null ? _string.GetHashCode() : 0);
		}

		public int CompareTo(FileName other)
		{
			return _string.CompareTo(other._string);
		}

		public override string ToString()
		{
			return _string;
		}

		public static bool operator ==(FileName a, FileName b)
		{
			return ReferenceEquals(a, b) || (!ReferenceEquals(a, null) && !ReferenceEquals(b, null) && a.Equals(b));
		}

		public static bool operator !=(FileName a, FileName b)
		{
			return !(a == b);
		}
	}
}
