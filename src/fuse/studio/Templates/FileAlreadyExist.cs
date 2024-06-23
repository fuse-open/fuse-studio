using System;

namespace Outracks.Fuse.Templates
{
	public class FileAlreadyExist : Exception
	{
		public FileAlreadyExist(string message) : base(message)
		{ }
	}
}
