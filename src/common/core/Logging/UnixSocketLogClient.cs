using System;
using System.Net.Sockets;
using System.Text;
using Mono.Unix;

namespace Outracks
{
	class UnixSocketLogClient : ILogClient
	{
		public void Send(string message)
		{
			using (var sock = new Socket (AddressFamily.Unix, SocketType.Dgram, ProtocolType.Unspecified)) {
				var logserver = Environment.GetEnvironmentVariable("HOME") + "/.fuse/logserver";
				var endpoint = new UnixEndPoint (logserver);
				sock.Connect (endpoint);
				sock.Send (Encoding.UTF8.GetBytes (message));
			}
		}

	}
}

