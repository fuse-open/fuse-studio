using System;
using Fuse.Preview;
using Outracks.Simulator.Protocol;

namespace Outracks.Fuse.Designer
{
	class ProjectHost : IDisposable, IProjectAndServer
	{
		public IObservable<BuildProject> BuildProject { get; private set; }
		public ProjectPreview Preview { get; private set; }
		public IProject Project { get; private set; }
		readonly Action<ProjectHost> _dispose;

		public ProjectHost(IObservable<BuildProject> buildProject, ProjectPreview preview, IProject project, Action<ProjectHost> dispose)
		{
			BuildProject = buildProject;
			Preview = preview;
			Project = project;
			_dispose = dispose;
		}

		public void Dispose()
		{
			_dispose(this);
		}
	}
}