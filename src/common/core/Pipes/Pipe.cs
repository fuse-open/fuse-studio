using System.IO;
using System.Threading.Tasks;

namespace Outracks.IPC
{
	public static class Pipe
	{
		public static Task<Stream> Connect(PipeName name)
		{
			return GetPipeImpl().Connect(name);
		}

		public static Task<Stream> Host(PipeName name)
		{
			return GetPipeImpl().Host(name);
		}

		static IPipeImpl GetPipeImpl()
		{
			if (NamedPipes.AreSupported)
				return new NamedPipes();

			return new UnixSocketPipes();
		}
	}
}