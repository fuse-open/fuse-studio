using System;
using System.IO;
using Outracks.Fuse.Protocol;
using Outracks.IO;

namespace Outracks.Fuse.Daemon
{
	public class EnsureSingleUser
    {
		readonly TextWriter _errorWriter;
		readonly IShell _fs;
		readonly AbsoluteFilePath _userFile;

		public EnsureSingleUser(
			TextWriter errorWriter,
			IShell fs,
			AbsoluteFilePath userFile)
		{
			_errorWriter = errorWriter;
			_fs = fs;
			_userFile = userFile;
		}

		public string GetUserName()
		{
			return Environment.UserName;
		}

		public string GetUserWhoRanDaemonLastTime()
		{
			try
			{
				if (!_fs.Exists(_userFile))
					return GetUserName();

				return _fs.ReadAllText(_userFile, 10);
			}
			catch (ArgumentException e)
			{
				_errorWriter.WriteLine(e.InnerException);
				return GetUserName();
			}
			catch (Exception e)
			{
				_errorWriter.WriteLine(e);
				return GetUserName();
			}
		}

		public void SetUserThatRunsDaemon(string currentUser)
		{
			try
			{
				var isOwner = !_fs.Exists(_userFile);

				if (isOwner)
					_fs.CreateIfNotExists(_userFile.ContainingDirectory);

				_fs.ReplaceText(_userFile, currentUser);

				// We only have permission to set permission when we are owner.
				if (isOwner)
				{
					_fs.SetPermission(
						_userFile,
						FileSystemPermission.Read | FileSystemPermission.Write,
						FileSystemGroup.Everyone);
				}
			}
			catch (Exception e)
			{
				_errorWriter.WriteLine(e.ToString());
			}
		}

		public DaemonKey GetDaemonKey()
		{
			return DaemonKey.GetDaemonKey();
		}
    }
}
