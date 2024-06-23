using System;
using Uno;
using System.IO;
using Uno.Compiler.ExportTargetInterop;

namespace Outracks
{
	public interface IBinaryMessage
	{
		string Type { get; }

		void WriteDataTo(BinaryWriter writer);
	}

	public static class BinaryMessage
	{
		public static Optional<T> TryParse<T>(this IBinaryMessage message, string type, Func<BinaryReader, T> action)
		{
			if (type != message.Type)
				return Optional.None<T>();

			using (var buffer = new Uno.IO.MemoryStream())
			using (var bufferWriter = new BinaryWriter(buffer))
			using (var bufferReader = new BinaryReader(buffer))
			{
				message.WriteDataTo(bufferWriter);
				buffer.Seek(0, Uno.IO.SeekOrigin.Begin);
				return action(bufferReader);
			}

			throw new Exception("It has happened.");
		}

		public static T[] SubArray<T>(this T[] data, int index, int length)
		{
			T[] result = new T[length];
			Array.Copy(data, index, result, 0, length);
			return result;
		}

		public static extern IBinaryMessage ReadFrom(BinaryReader reader)
		{
			var type = reader.ReadString();
			var length = reader.ReadInt32();
			var data = reader.ReadBytes(length);
			return Compose(type, new WriteClosure { Data = data }.Execute);
		}

		class WriteClosure
		{
			public byte[] Data;

			public void Execute(BinaryWriter writer)
			{
				writer.Write(Data);
			}
		}

		public static extern void WriteTo(this IBinaryMessage message, BinaryWriter writer)
		{
			using (var buffer = new Uno.IO.MemoryStream())
			using (var bufferWriter = new BinaryWriter(buffer))
			{
				message.WriteDataTo(bufferWriter);
				var length = (int) buffer.Position;

				writer.Write(message.Type);
				writer.Write(length);
				writer.Write(buffer.GetBuffer().SubArray(0, length));
			}
		}

		public static extern IBinaryMessage Compose(string type, Action<BinaryWriter> write)
		{
			return new ComposedMessage()
			{
				Type = type,
				Write = write,
			};
		}

		class ComposedMessage : IBinaryMessage
		{
			public string Type { get; set; }
			public Action<BinaryWriter> Write { get; set; }

			public void WriteDataTo(BinaryWriter writer)
			{
				Write(writer);
			}
		}
	}
}
