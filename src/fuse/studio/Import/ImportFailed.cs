using System;

namespace Outracks.Fuse.Import
{
	public class ImportFailed : Exception
	{
		public ImportFailed(string message, Exception innerException = null)
			: base(message, innerException)
		{ }
	}
}