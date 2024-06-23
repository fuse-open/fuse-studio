using System.IO;
using System.Threading.Tasks;

namespace Outracks.IPC
{
	public interface IPipeImpl
	{
		Task<Stream> Connect(PipeName name);

		Task<Stream> Host(PipeName name);
	}
}