using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using Mono.Unix;
using Mono.Unix.Native;

namespace Outracks.IPC
{
	public enum SocketUsage
	{
		Host,
		Client
	}

	public class UnixSocketStream : NetworkStream
	{
		public UnixSocketStream(PipeName pipeName, SocketUsage socketUsage)
			:base(
				Init(socketUsage, pipeName),
				FileAccess.ReadWrite,
				true)
		{}

		static Socket Init(SocketUsage socketUsage, PipeName pipeName)
		{
			switch (socketUsage)
			{
				case SocketUsage.Client:
					return Connect(pipeName, isBlocking: true);
				case SocketUsage.Host:
					return Host(pipeName);
				default:
					throw new ArgumentException("socketUsage");
			}
		}

		public static bool SocketExists(PipeName pipeName)
		{
			try
			{
				using (Connect(pipeName, isBlocking: false))
					return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		static Socket Connect(PipeName pipeName, bool isBlocking)
		{
			var endPoint = CreateEndPoint(pipeName);
			var path = CreatePath(pipeName);
			var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);

			while(true)
			{
				if (!File.Exists(path))
				{
					if (isBlocking)
					{
						Thread.Sleep(1);
						continue;
					}
					else
						throw new FileNotFoundException("Failed to find Unix socket.", path);
				}

				try
				{
					socket.Connect(endPoint);
					return socket;
				}
				catch(SocketException)
				{
					if (isBlocking)
						Thread.Sleep(10);
					else
						throw;
				}
			}
		}

		static Socket Host(PipeName pipeName)
		{
			var endPoint = CreateEndPoint(pipeName);

			using(var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified))
			{
				socket.Bind(endPoint);
				socket.Listen(1);

				var handler = socket.Accept();
				return handler;
			}
		}

		public static void Unlink(PipeName pipeName)
		{
			var endPoint = CreateEndPoint(pipeName);
			Syscall.unlink(endPoint.Filename);
		}

		static UnixEndPoint CreateEndPoint(PipeName pipeName)
		{
			var path = CreatePath(pipeName);
			var endPoint = new UnixEndPoint(path);
			return endPoint;
		}

		static string CreatePath(PipeName pipeName)
		{
			var path = SocketDirectory;
			Directory.CreateDirectory(path);
			return Path.Combine(path, pipeName.ToString());
		}

		public static string SocketDirectory
		{
			get
			{
				return "/tmp/.fuse-" + Environment.UserName;
			}
		}
	}
}