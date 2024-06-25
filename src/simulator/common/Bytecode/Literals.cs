using System.IO;
using Uno;

namespace Outracks.Simulator.Bytecode
{
	public class BlobLiteral : Literal
	{
		public readonly byte[] Bytes;

		public override char LiteralId { get { return LiteralIdRegistry.BlobLiteral; } }

		public BlobLiteral(byte[] bytes)
		{
			Bytes = bytes;
		}

		public override string ToString()
		{
			return "new byte[" + Bytes.Length + "]";
		}

		public static void Write(BlobLiteral blob, BinaryWriter writer)
		{
			writer.Write(blob.Bytes.Length);
			writer.Write(blob.Bytes);
		}

		protected override void WriteLiteral(BinaryWriter writer)
		{
			BlobLiteral.Write(this, writer);
		}

		new public static BlobLiteral Read(BinaryReader reader)
		{
			var length = reader.ReadInt32();
			var bytes = reader.ReadBytes(length);
			return new BlobLiteral(bytes);
		}
	}

	public class BooleanLiteral : Literal
	{
		public readonly bool BooleanValue;

		public override char LiteralId { get { return LiteralIdRegistry.BooleanLiteral; } }

		public BooleanLiteral(bool booleanValue)
		{
			BooleanValue = booleanValue;
		}

		public override string ToString()
		{
			return BooleanValue.ToString();
		}

		public static void Write(BooleanLiteral l, BinaryWriter writer)
		{
			writer.Write(l.BooleanValue);
		}

		protected override void WriteLiteral(BinaryWriter writer)
		{
			BooleanLiteral.Write(this, writer);
		}

		new public static BooleanLiteral Read(BinaryReader reader)
		{
			return new BooleanLiteral(
				reader.ReadBoolean());
		}
	}

	public class StringLiteral : Literal
	{
		public readonly string StringValue;

		public override char LiteralId { get { return LiteralIdRegistry.StringLiteral; } }

		public StringLiteral(string stringValue)
		{
			StringValue = stringValue;
		}

		public override string ToString()
		{
			return "\"" + StringValue + "\"";
		}

		public static void Write(StringLiteral l, BinaryWriter writer)
		{
			Optional.Write(writer, l.StringValue.ToOptional(), WriteString);
		}

		protected override void WriteLiteral(BinaryWriter writer)
		{
			StringLiteral.Write(this, writer);
		}

		static void WriteString(string str, BinaryWriter writer)
		{
			writer.Write(str);
		}

		new public static StringLiteral Read(BinaryReader reader)
		{
			var maybeString = Optional.Read(reader, (Func<BinaryReader, string>)ReadString);
			return new StringLiteral(
				maybeString.HasValue
					? maybeString.Value
					: null);
		}

		static string ReadString(BinaryReader reader)
		{
			return reader.ReadString();
		}
	}

	public enum NumberType
	{
		Double,
		Float,
		Int,
		UInt,
		Short,
		UShort,
		SByte,
		Byte,
	}

	public class NumberLiteral : Literal
	{
		public readonly NumberType NumberType;
		public readonly double DoubleValue;

		public override char LiteralId { get { return LiteralIdRegistry.NumberLiteral; } }

		public NumberLiteral(NumberType numberType, double doubleValue)
		{
			DoubleValue = doubleValue;
			NumberType = numberType;
		}

		public override string ToString()
		{
			return DoubleValue.ToString();
		}

		public static void Write(NumberLiteral l, BinaryWriter writer)
		{
			writer.Write((int) l.NumberType);
			writer.Write(l.DoubleValue);
		}

		protected override void WriteLiteral(BinaryWriter writer)
		{
			NumberLiteral.Write(this, writer);
		}

		new public static NumberLiteral Read(BinaryReader reader)
		{
			var nubmerType =(NumberType)reader.ReadInt32();
			var doubleValue =reader.ReadDouble();
			return new NumberLiteral(
				nubmerType,
				doubleValue);
		}
	}

	public class EnumLiteral : Literal
	{
		public readonly StaticMemberName Value;

		public override char LiteralId { get { return LiteralIdRegistry.EnumLiteral; } }

		public EnumLiteral(StaticMemberName value)
		{
			Value = value;
		}

		public override string ToString()
		{
			return Value.ToString();
		}

		public static void Write(EnumLiteral l, BinaryWriter writer)
		{
			StaticMemberName.Write(l.Value, writer);
		}

		protected override void WriteLiteral(BinaryWriter writer)
		{
			EnumLiteral.Write(this, writer);
		}

		new public static EnumLiteral Read(BinaryReader reader)
		{
			return new EnumLiteral(
				StaticMemberName.Read(reader));
		}
	}
}