using Uno;
using Uno.IO;
using Uno.UX;
using Uno.Net;
using Uno.Collections;
using Fuse;
using Uno.Diagnostics;
using Fuse.Controls;
using Fuse.Elements;

namespace Outracks.Simulator.Client
{
	using Bytecode;
	using Protocol;
	using Runtime;
	using Fuse;
	using Fuse.Input;

	public class FakeApp : Panel, Fuse.IRootVisualProvider
	{
		readonly App _app;
		
		public AppBase App { get { return _app; } }

		Visual Fuse.IRootVisualProvider.Root { get { return RootViewport; } }

		public FakeApp(App app)
		{
			_app = app;
		}

		new public float4 Background
		{
			get { return _app.Background; }
			set { _app.Background = value; }
		}

		new public IList<Resource> Resources
		{
			get { return _app.Resources; }
		}

		public float4 ClearColor
		{
			get { return _app.Background; }
			set { _app.Background = value; }
		}

		public RootViewport RootViewport
		{
			get { return _app.RootViewport; }
		}
	}
}