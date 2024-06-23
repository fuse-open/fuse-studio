using System;

namespace Fuse.Preview
{
	public class RunFailed : Exception
	{
		public RunFailed(Exception innerException)
			: base(innerException.Message)
		{ }
	}
}