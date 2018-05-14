using System.IO;
using System.Reflection;

namespace Fuse.Preview
{
	public interface IPlatform
	{
		IProcess StartProcess(Assembly assembly, params string[] args);
		
		IProcess StartSingleProcess(Assembly assembly, params string[] args);

		Stream CreateStream(string name);

		IEnsureSingleInstance EnsureSingleInstance();
	}
}