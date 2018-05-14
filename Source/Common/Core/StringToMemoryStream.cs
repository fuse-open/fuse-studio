using System.IO;

namespace Outracks.IO
{
	public static class StringToMemoryStream
	{
		public static Stream ToMemoryStream(this string data)
		{
			var stream = new MemoryStream();
			var writer = new StreamWriter(stream);
			writer.Write(data);
			writer.Flush();
			stream.Position = 0;
			return stream;
		}
	}
}