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
	
	public enum InputMode
	{
		Interactive,
		Design,
	}

	public static class Context
	{
		public static InputMode InputMode { get; private set; }
		public static FakeApp App { get; private set; }
		public static IPEndPoint[] ProxyEndpoints { get; private set; }
		public static string Project { get; private set; }
		public static string[] Defines { get; private set; }

		public static IReflection Reflection { get; set; }

		public static void SetApp(FakeApp app)
		{
			App = app;
		}

		public static void SetGlobals(IPEndPoint[] proxyEndpoints, string project, string[] defines)
		{
			ProxyEndpoints = proxyEndpoints;
			Project = project;
			Defines = defines;
		}
	}
	
}
