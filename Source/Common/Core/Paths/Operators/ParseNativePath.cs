using System;
using System.IO;
using Newtonsoft.Json;

namespace Outracks.IO
{
	[JsonConverter(typeof(PathJsonConverter))]
	public partial class AbsoluteDirectoryPath
	{
		public static IValidationResult<AbsoluteDirectoryPath> Validate(string nativePath)
		{
			return TryParse(nativePath).AsValidationResult("Invalid path");
		}

		public static Optional<AbsoluteDirectoryPath> TryParse(string nativePath)
		{
			try
			{
				return Parse(nativePath);
			}
			catch (Exception)
			{
				return Optional.None();
			}
		}

		/// <exception cref="InvalidPath"></exception>
		/// <exception cref="ArgumentException"></exception>
		public static IO.AbsoluteDirectoryPath Parse(string nativePath)
		{
			if (string.IsNullOrEmpty(nativePath))
				throw new ArgumentException("Expected nativePath to not be null or empty.", "nativePath");

			try
			{
				var parent = Path.GetDirectoryName(nativePath);
				if (parent != null && Path.GetFileName(nativePath) == "")
				{
					nativePath = parent;
					parent = Path.GetDirectoryName(parent);
				}

				return string.IsNullOrEmpty(parent)
					? new AbsoluteDirectoryPath(nativePath)
					: new AbsoluteDirectoryPath(
						Path.GetFileName(nativePath),
						Parse(parent));
			}
			catch (Exception e)
			{
				throw new InvalidPath(nativePath, e);
			}
		}
	}

	[JsonConverter(typeof(PathJsonConverter))]
	public partial class RelativeDirectoryPath
	{
		public static Optional<RelativeDirectoryPath> TryParse(string nativePath)
		{
			try
			{
				return Parse(nativePath);
			}
			catch (Exception)
			{
				return Optional.None();
			}
		}

		/// <exception cref="InvalidPath"></exception>
		public static RelativeDirectoryPath Parse(string nativePath)
		{
			if (string.IsNullOrEmpty(nativePath)) return null;

			try
			{
				return new RelativeDirectoryPath(
					new DirectoryName(Path.GetFileName(nativePath)),
					Parse(Path.GetDirectoryName(nativePath)));
			}
			catch (Exception e)
			{
				throw new InvalidPath(nativePath, e);
			}
		}

	}

	[JsonConverter(typeof(PathJsonConverter))]
	public partial class AbsoluteFilePath
	{
		public static Optional<AbsoluteFilePath> TryParse(string nativePath)
		{
			// Early out for common cases for now instead of fixing things
			if (string.IsNullOrWhiteSpace(nativePath))
				return Optional.None();

			if (nativePath.IndexOf(Path.DirectorySeparatorChar) == -1 && 
				nativePath.IndexOf(Path.AltDirectorySeparatorChar) == -1)
				return Optional.None();
			
			try
			{
				return Parse(nativePath);
			}
			catch (Exception)
			{
				return Optional.None();
			}
		}

		/// <exception cref="InvalidPath"></exception>
		/// <exception cref="ArgumentException"></exception>
		public static AbsoluteFilePath Parse(string nativePath)
		{
			if (string.IsNullOrEmpty(nativePath)) 
				throw new ArgumentException("nativePath");

			try
			{
				return new AbsoluteFilePath(
					new FileName(Path.GetFileName(nativePath)),
					AbsoluteDirectoryPath.Parse(Path.GetDirectoryName(nativePath)));
			}
			catch (Exception e)
			{
				throw new InvalidPath(nativePath, e);
			}
		}
	}

	[JsonConverter(typeof(PathJsonConverter))]
	public partial class RelativeFilePath
	{
		public static Optional<RelativeFilePath> TryParse(string nativePath)
		{
			try
			{
				return Parse(nativePath);
			}
			catch (Exception)
			{
				return Optional.None();
			}
		}

		/// <exception cref="InvalidPath"></exception>
		public static RelativeFilePath Parse(string nativePath)
		{
			if (string.IsNullOrEmpty(nativePath)) return null;

			try
			{
				return new RelativeFilePath(
					new FileName(Path.GetFileName(nativePath)),
					RelativeDirectoryPath.Parse(Path.GetDirectoryName(nativePath)));
			}
			catch (Exception e)
			{
				throw new InvalidPath(nativePath, e);
			}
		}
	}

	public static class UriExtensionMethods
	{
		public static AbsoluteFilePath ToAbsoluteFilePath(this Uri uri)
		{
			return AbsoluteFilePath.Parse(Uri.UnescapeDataString(uri.AbsolutePath));
		}

		public static AbsoluteDirectoryPath ToAbsoluteDirectoryPath(this Uri uri)
		{
			return AbsoluteDirectoryPath.Parse(Uri.UnescapeDataString(uri.LocalPath));
		}
	}
}
