using System;
using Uno;
using System.IO;

namespace Outracks
{

	public static partial class Optional
	{
		public static Optional<T> Read<T>(BinaryReader reader, Func<T> readSome)
		{
			return reader.ReadBoolean()
				? Optional.Some(readSome())
				: Optional.None<T>();
		}

		public static Optional<T> Read<T>(BinaryReader reader, Func<BinaryReader, T> readSome)
		{
			return reader.ReadBoolean()
				? Optional.Some(readSome(reader))
				: Optional.None<T>();
		}

		public static void Write<T>(BinaryWriter writer, Optional<T> optional, Action<BinaryWriter, T> writeSome)
		{
			if (optional.HasValue)
			{
				writer.Write(true);
				writeSome(writer, optional.Value);
			}
			else
			{
				writer.Write(false);
			}
		}

		public static void Write<T>(BinaryWriter writer, Optional<T> optional, Action<T, BinaryWriter> writeSome)
		{
			if (optional.HasValue)
			{
				writer.Write(true);
				writeSome(optional.Value, writer);
			}
			else
			{
				writer.Write(false);
			}
		}

		public static void Write<T>(BinaryWriter writer, Optional<T> optional, Action<T> writeSome)
		{
			if (optional.HasValue)
			{
				writer.Write(true);
				writeSome(optional.Value);
			}
			else
			{
				writer.Write(false);
			}
		}
	}
}