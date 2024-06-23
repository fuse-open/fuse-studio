using Uno;
using System.IO;
using Uno.Compiler.ExportTargetInterop;

namespace System.IO
{
	[DotNetType]
	public class BinaryWriter : IDisposable
	{
		readonly Uno.IO.BinaryWriter _writer;

		public void Write(float value) { _writer.Write(value); }
		public void Write(double value) { _writer.Write(value); }

		public void Write(string value) { _writer.Write(value); }
		public void Write(int value) { _writer.Write(value); }
		public void Write(char value) { _writer.Write(value); }
		public void Write(bool value) { _writer.Write(value); }
		public void Write(byte[] bytes) { _writer.Write(bytes); }

		public BinaryWriter(Uno.IO.Stream stream) { _writer = new Uno.IO.BinaryWriter(stream); }
		public void Dispose() { _writer.Dispose(); }
	}
}

namespace System.IO
{
	[DotNetType]
	public class BinaryReader : IDisposable
	{
		readonly Uno.IO.BinaryReader _reader;

		public float ReadSingle() { return _reader.ReadFloat(); }
		public double ReadDouble() { return _reader.ReadDouble(); }

		public string ReadString() { return _reader.ReadString(); }
		public int ReadInt32() { return _reader.ReadInt(); }
		public char ReadChar() { return _reader.ReadChar(); }
		public bool ReadBoolean() { return _reader.ReadBoolean(); }
		public byte[] ReadBytes(int length) { return _reader.ReadBytes(length); }
		public BinaryReader(Uno.IO.Stream stream) { _reader = new Uno.IO.BinaryReader(stream); }

		public void Dispose() { _reader.Dispose(); }
	}
}

namespace Outracks.Simulator
{
	public static class GuidSerializer
	{
		// TODO: Uno.Guid lacks to/from byte array. Should have used that.
		// TODO2: Uno lacks the ability to add overloads by means of extension methods, so this can't be named Write(). Should have been named that.
		public static void WriteGuid(this BinaryWriter writer, Guid guid)
		{
			writer.Write(guid.ToString());
		}

		public static Guid ReadGuid(this BinaryReader reader)
		{
			return new Guid(reader.ReadString());
		}
	}

	static class Serialization
	{
		public static Func<BinaryReader, string> ReadString = _ReadString;

		public static string _ReadString(this BinaryReader reader)
		{
			return reader.ReadString();
		}
	}

	public class DummyApplication : Application
	{
	}
}
