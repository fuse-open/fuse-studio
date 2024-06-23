using System;
using Outracks.Simulator;

namespace Fuse.Preview
{
	public class InternalBuildError : BuildFailed
	{
		public InternalBuildError(Exception innerException)
			: base(/*Language.Current.InternalBuildError*/"Internal build error", innerException)
		{
		}
	}
}