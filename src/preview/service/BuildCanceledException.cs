using System;

namespace Outracks.Building
{
	public class BuildCanceledException : Exception
	{
		public BuildCanceledException() : base("Build was canceled")
		{ }
	}
}