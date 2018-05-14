using Uno;
using Uno.UX;
using Uno.IO;
using Uno.Collections;

namespace Outracks.Simulator.Runtime
{
	public class UxFileSource : FileSource
	{
		byte[] _bytes;

		public UxFileSource(string path, byte[] bytes)
			: base(path)
		{
			_bytes = bytes;
		}

		public void Update(byte[] newBytes)
		{
			_bytes = newBytes;
			OnDataChanged();
		}

		public override Stream OpenRead()
		{
			// TODO: this should be MemoryStream. ArrayStream is temp and defined in Simulator.Common
			return new ArrayStream(_bytes);
		}

		public override byte[] ReadAllBytes()
		{
			return _bytes;
		}
	}
}
