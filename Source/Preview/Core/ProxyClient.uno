using System;
using Uno;
using Uno.Collections;
using System.IO;
using Uno.Net;
using Uno.Net.Sockets;

namespace Outracks.Simulator
{

	public class FailedToConnectToProxy : Exception
	{
		public readonly ImmutableList<Exception> InnerExceptions;

		public FailedToConnectToProxy(IEnumerable<Exception> innerExceptions)
			: base("Failed to connect to proxy:\n" + innerExceptions.ToIndentedLines()) // TODO: move this view logic away from here
		{
			InnerExceptions = innerExceptions.ToImmutableList();
		}
	}

	public class DesignerNotRunning : Exception
	{

	}

	public class ProxyClient
	{
		public static Task<IPEndPoint[]> GetSimulatorEndpoint(IEnumerable<IPEndPoint> proxyEndpoints, string project, IEnumerable<string> defines)
		{
			var tasks = new List<Task<IPEndPoint[]>>();

			foreach (var endpoint in proxyEndpoints)
				tasks.Add(
					Tasks.Run<IPEndPoint[]>(
						new GetSimulatorEndpoint(endpoint, project, defines.ToArray()).Execute));

			return Tasks.WaitForFirstResult<IPEndPoint[]>(tasks, OnNoResult);
		}

		static IPEndPoint[] OnNoResult(IEnumerable<Exception> exceptions)
		{
			foreach (var exception in exceptions)
			{
				if (exception is DesignerNotRunning)
				{
					throw new DesignerNotRunning();
				}
			}
			throw new FailedToConnectToProxy(exceptions);
		}
	}

	class GetSimulatorEndpoint
	{

		readonly IPEndPoint proxy;
		readonly string project;
		readonly string[] defines;

		public GetSimulatorEndpoint(IPEndPoint proxy, string project, string[] defines)
		{
			this.proxy = proxy;
			this.project = project;
			this.defines = defines;
		}

		public IPEndPoint[] Execute()
		{
			try
			{
				var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

				socket.Connect(proxy);

				using (var stream = new NetworkStream(socket))
				using (var writer = new BinaryWriter(stream))
				using (var reader = new BinaryReader(stream))
				{
					writer.Write(project);
					writer.Write(string.Join(" ", defines));

					var initialState = reader.ReadString();

					if ("DESIGNER_NOT_RUNNING".Equals(initialState))
						throw new DesignerNotRunning();

					if ("SUCCESS".Equals(initialState) == false)
						throw new Exception("Failed to request host.");

					var endpointCount = reader.ReadInt32();
					var endpoints = new IPEndPoint[endpointCount];
					for (int i = 0; i < endpoints.Length; i++)
					{
						var simulatorAddress = reader.ReadString();
						var simulatorPort = reader.ReadInt32();

						endpoints[i] = new IPEndPoint(IPAddress.Parse(simulatorAddress), simulatorPort);
					}

					try
					{
						socket.Shutdown(SocketShutdown.Both);
					}
					catch (Exception e)
					{
						// We may already be connected
					}

					return endpoints;
				}

				// UnoBug: this code is unreachable, but uno disagrees
				throw new Exception("Call Tom Curise");

			}
			catch (DesignerNotRunning)
			{
				throw;
			}
			catch (Exception e)
			{
				throw new FailedToConnectToEndPoint(proxy, e);
			}
		}
	}

}