using System;

namespace Outracks.Simulator
{
	public abstract class BuildFailed : Exception
	{
		protected BuildFailed(string reason, Exception innerException = null)
			: base(reason, innerException)
		{ }

		protected BuildFailed()
		{ }
	}
}