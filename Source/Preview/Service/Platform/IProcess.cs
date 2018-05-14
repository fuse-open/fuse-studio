using System.Collections.Generic;
using System.IO;

namespace Fuse.Preview
{
	public interface IProcess
	{
		Stream OpenStream(string name);
		IList<string> Arguments { get; }
	}
}