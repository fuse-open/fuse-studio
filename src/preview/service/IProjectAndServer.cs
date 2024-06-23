using System;
using Outracks.Simulator.Protocol;

namespace Fuse.Preview
{
	public interface IProjectAndServer
	{
		IObservable<BuildProject> BuildProject { get; }
		ProjectPreview Preview { get; }

	}
}