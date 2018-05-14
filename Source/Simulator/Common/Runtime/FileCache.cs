using Uno;
using Uno.UX;
using Uno.Collections;

namespace Outracks.Simulator.Runtime
{
	public class FileCache
	{
		static readonly Dictionary<string, UxFileSource> _cache = new Dictionary<string, UxFileSource>();

		public static void Update(string path, byte[] bytes)
		{
			UxFileSource fs = null;
			if (_cache.TryGetValue(path, out fs))
				fs.Update(bytes);
			else
				_cache[path] = new UxFileSource(path, bytes);
		}

		public static UxFileSource GetFileSource(string path)
		{
			return _cache[path];
		}
	}
}
