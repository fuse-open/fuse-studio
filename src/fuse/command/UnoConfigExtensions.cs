using Outracks.IO;
using Uno.Configuration;

namespace Outracks.Fuse
{
	public static class UnoConfigExtensions
	{
        public static AbsoluteDirectoryPath GetTemplatesDir(this UnoConfig unoConfig)
        {
            return AbsoluteDirectoryPath.Parse(unoConfig.GetFullPath("Fuse.Templates"));
        }
	}
}