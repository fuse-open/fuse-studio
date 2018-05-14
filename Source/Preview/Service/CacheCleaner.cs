using System;
using System.IO;
using Outracks.IO;

namespace Fuse.Preview
{
	public class CacheCleaner
	{
		readonly IFileSystem _fs;		
		readonly Version _version;

		public CacheCleaner(IFileSystem fs, Version version)
		{
			_fs = fs;
			_version = version;
		}

		public void CleanIfNecessary(AbsoluteDirectoryPath cacheDirectory)
		{
			var versionFile = cacheDirectory / new FileName(".fuse-version");
			if (ShouldClean(versionFile))
			{
				TryClean(versionFile, cacheDirectory);
			}
		}

		bool ShouldClean(AbsoluteFilePath versionFile)
		{
			if (!_fs.Exists(versionFile))
				return true;

			Version version;
			if (!Version.TryParse(_fs.ReadAllText(versionFile, 3), out version))
				return true;

			if (version != _version)
				return true;

			return false;
		}

		void TryClean(AbsoluteFilePath versionFile, AbsoluteDirectoryPath cacheDirectory)
		{
			try
			{
				_fs.Delete(cacheDirectory);
				_fs.Create(cacheDirectory);
				WriteVersion(versionFile, _version);	
			}
			catch (IOException) { }
			catch (UnauthorizedAccessException) { }		
		}

		/// <exception cref="IOException"></exception>
		/// <exception cref="UnauthorizedAccessException"></exception>
		void WriteVersion(AbsoluteFilePath versionFile, Version version)
		{
			using(var stream = _fs.Create(versionFile))
			using (var streamWriter = new StreamWriter(stream))
			{
				streamWriter.WriteLine(version);
			}
		}

	}
}