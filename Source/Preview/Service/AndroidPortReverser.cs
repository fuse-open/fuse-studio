using System;
using System.Diagnostics;
using System.Reactive;
using System.Text;
using Outracks;
using System.IO;
using Uno.Configuration;

namespace Fuse.Preview
{
	public class AndroidReversePortFailed : Exception
	{
		public readonly int RemotePort;
		public readonly int LocalPort;
		public readonly string Output;

		public AndroidReversePortFailed(int remotePort, int localPort, string output, string error)
			: base("Tried to reverse adb port 'tcp:" + remotePort + "' 'tcp:" + localPort + "'. However failed with: " + error)
		{
			RemotePort = remotePort;
			LocalPort = localPort;
			Output = output;
		}
	}

	public class AndroidPortReverser
	{
		/// <exception cref="AndroidReversePortFailed"></exception>
		public void ReversePort(int remotePort, int localPort)
		{
			try
			{
				var sdkDir = UnoConfig.Current.GetFullPath("Android.SDK.Directory", "AndroidSdkDirectory");
				var p = Process.Start(new ProcessStartInfo(Path.Combine(sdkDir, "platform-tools", "adb"), "reverse tcp:" + remotePort + " tcp:" + localPort)
				{
					CreateNoWindow = true,
					UseShellExecute = false
				});
				// TODO: Use the adb runner directly, which isn't possible at this point since the class is private.
				p.WaitForExit();
				if (p.ExitCode != 0)
				{
					throw new AndroidReversePortFailed(remotePort, localPort, "", "");
				}
			}
			catch (AndroidReversePortFailed)
			{
				throw;
			}
			catch (Exception e)
			{
				throw new AndroidReversePortFailed(remotePort, localPort, "", e.Message);
			}
		}
	}

	public static class AndroidPortReverserExtensions
	{
		public static void ReversePortOrLogErrors(this AndroidPortReverser portReverser, IReport log, int remotePort, int localPort)
		{
			try
			{
				portReverser.ReversePort(remotePort, localPort);
			}
			catch (AndroidReversePortFailed e)
			{
				log.Exception("Failed to reverse port", e);
			}		
		}
	}
}