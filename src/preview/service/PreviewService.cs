using System;
using Outracks;
using Outracks.Fuse.Auth;
using Outracks.IO;

namespace Fuse.Preview
{
	public class PreviewService : IDisposable
	{
		readonly ReplayQueueSubject<string> _logMessages = new ReplayQueueSubject<string>();
		public IObservable<string> LogMessages
		{
			get { return _logMessages; }
		}

		readonly IShell _shell;
		readonly BuildOutputDirGenerator _buildOutputDirGenerator;
		readonly ProxyServer _proxy;

		public PreviewService()
		{
			_shell = new Shell();
			_buildOutputDirGenerator = new BuildOutputDirGenerator(_shell);
			_proxy = ProxyServer.Start(_logMessages.OnNext);
		}

		public void UpdateReversedPorts(bool shouldUpdate)
		{
			_proxy.UpdateReversedPorts(shouldUpdate);
		}

		public ProjectPreview StartPreview(IProjectLike project)
		{
			return new ProjectPreview(project, _shell, _buildOutputDirGenerator, _proxy);
		}

		public void Dispose()
		{
			_proxy.Dispose();
		}
	}
}
