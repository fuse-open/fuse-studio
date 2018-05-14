using System.IO;

namespace Fuse.Preview
{
	public static class PlatformFactory
	{
		public static IPlatform Create()
		{
			if (Path.DirectorySeparatorChar == '\\')
				return new WindowsPlatform();
			else
				return new MacPlatform();  
		}
	}
}