using System.IO;

namespace Outracks
{
    public static class FileHelpers 
    {
        public static long GetFileTime(string filename)
        {
            return File.GetLastWriteTime(filename).ToFileTime();
        }

        public static string ToUnixPath(this string str)
        {
            return str.Replace('\\', '/');
        }
    }
}