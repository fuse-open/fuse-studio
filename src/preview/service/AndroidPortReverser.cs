using System;
using System.Diagnostics;
using System.IO;
using Outracks;
using Uno.Build.Adb;
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
				if (new AdbRunner(Uno.Diagnostics.Shell.Default)
						.Run("reverse tcp:" + remotePort + " tcp:" + localPort) != 0)
					throw new AndroidReversePortFailed(remotePort, localPort, "", "");
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
