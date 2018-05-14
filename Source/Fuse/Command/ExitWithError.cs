using System;

namespace Outracks.Fuse
{
	public class ExitWithError : Exception
	{
		public readonly byte ExitCode;
		public readonly string ErrorOutput;

		public ExitWithError(string errorOutput, byte exitCode = 1)
		{
			ExitCode = exitCode;
			ErrorOutput = errorOutput;
		}
	}
}