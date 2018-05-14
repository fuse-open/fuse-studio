using System;
using System.IO;

namespace Outracks
{
	public static class GuidSerializer
	{
		public static void Write(this BinaryWriter writer, Guid guid)
		{
			writer.Write(guid.ToString());
		}

		public static Guid ReadGuid(this BinaryReader reader)
		{
			return new Guid(reader.ReadString());
		}
	}
}