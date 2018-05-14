using System;
using System.IO;
using System.Threading.Tasks;

namespace Outracks.IPC
{
	public class UnixSocketPipes : IPipeImpl
	{
		public Task<Stream> Connect(PipeName name)
		{
            return Task.Run(() => (Stream)new UnixSocketStream(name, SocketUsage.Client));
		}

		public Task<Stream> Host(PipeName name)
		{
            return Task.Run(() => (Stream)new UnixSocketStream(name, SocketUsage.Host));
		}
	}
}