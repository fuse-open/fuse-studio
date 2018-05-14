using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using Outracks;

namespace Fuse.Preview
{

	public class WindowsPlatform : IPlatform
	{
		public IProcess StartProcess(Assembly assembly, params string[] args)
		{
			var exePath = assembly.CodeBase.StripPrefix("file:///");

			var startInfo = new ProcessStartInfo
			{
				UseShellExecute = false,
				RedirectStandardInput = true,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				CreateNoWindow = true,
				FileName = exePath,
				Arguments = ProcessArguments.PackList(args),
			};

			Process.Start(startInfo);
			return new WindowsProcess(args);
		}

		public Stream CreateStream(string name)
		{
			var stream = new NamedPipeServerStream(GetProcessIdentifier() + name);
			stream.WaitForConnection();
			return stream;
		}

		public IEnsureSingleInstance EnsureSingleInstance()
		{
			return new EnsureSingleInstanceWin(GetProcessIdentifier());
		}

		public IProcess StartSingleProcess(Assembly assembly, params string[] args)
		{
			using (var ensureSingleInstance = new EnsureSingleInstanceWin(args.Join("-")))
			{
				if (ensureSingleInstance.IsAlreadyRunning())
					return new WindowsProcess(args);
			}

			return StartProcess(assembly, args);
		}

		static string GetProcessIdentifier()
		{
			return Environment.GetCommandLineArgs().Skip(1).Join("-");
		}
	}

	class WindowsProcess : IProcess
	{
		public WindowsProcess(IList<string> arguments)
		{
			Arguments = arguments;
		}

		public IList<string> Arguments { get; private set; }

		public Stream OpenStream(string name)
		{
			var stream = new NamedPipeClientStream(GetProcessIdentifier() + name);
			stream.Connect();
			return stream;
		}

		public string GetProcessIdentifier()
		{
			return Arguments.Join("-");
		}
	}

}