using System.IO;
using System.IO.Compression;

namespace Outracks.Fuse.Auth.Utilities
{
	public static class GZip
	{
		public static byte[] CompressGZip(this byte[] raw, CompressionLevel level = CompressionLevel.Fastest)
		{
			using (var memory = new MemoryStream())
			{
				using (var gzip = new GZipStream(memory, level, true))
					gzip.Write(raw, 0, raw.Length);

				return memory.ToArray();
			}
		}

		public static byte[] DecompressGZip(this byte[] gzip)
		{
			using (var stream = new GZipStream(new MemoryStream(gzip), CompressionMode.Decompress))
			{
				const int size = 4096;
				var buffer = new byte[size];

				using (var memory = new MemoryStream())
				{
					int count;

					do
					{
						count = stream.Read(buffer, 0, size);
						if (count > 0)
							memory.Write(buffer, 0, count);
					}
					while (count > 0);

					return memory.ToArray();
				}
			}
		}
	}
}
