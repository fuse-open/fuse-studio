using System;
using System.Reactive.Linq;
using Fuse.Preview;
using Outracks.UnoHost;

namespace Outracks.Fuse.Stage
{
	using Fusion;
	using Simulator.Protocol;

	class ViewportFactory
	{
		readonly ProjectPreview _preview;
		readonly IProperty<bool> _selectionEnabled;
		readonly IObservable<AssemblyBuilt> _assembly;
		readonly int _port;
		readonly IProject _project;
		readonly IFuse _fuse;
		readonly IObserver<OpenGlVersion> _glVersion;

		public ViewportFactory(
			ProjectPreview preview,
			IProperty<bool> selectionEnabled,
			int port, 
			IProject project, 
			IFuse fuse,
			IObserver<OpenGlVersion> glVersion)
		{
			_preview = preview;
			_selectionEnabled = selectionEnabled;
			_assembly = preview.Assembly.Replay(1).RefCount();
			_port = port;
			_project = project;
			_fuse = fuse;
			_glVersion = glVersion;
		}

		public IViewport Create(
			VirtualDevice initialDevice,
			Action<IViewport> onFocus,
			Action<IViewport> onClose,
			Func<IViewport, Menu> menu)
		{
			return new ViewportController(
				initialDevice, onFocus, onClose, menu,
				_preview, _assembly, _port, _fuse,
				unoHost => Gizmos.Initialize(unoHost, _selectionEnabled, _project),
				_glVersion);
		}

	}
}