using Uno;
using System.IO;

namespace Outracks.Simulator.Bytecode
{
	public abstract class Literal : Expression
//		, IMatchTypes<
//			BooleanLiteral,
//			StringLiteral,
//			NumberLiteral,
//			EnumLiteral,
//			BlobLiteral>
	{
		public abstract char LiteralId { get; }
		protected abstract void WriteLiteral(BinaryWriter writer);

		public override char ExpressionId { get { return ExpressionIdRegistry.Literal; } }

		public T Match<T>(
			Func<BooleanLiteral, T> a1,
			Func<StringLiteral, T> a2,
			Func<NumberLiteral, T> a3,
			Func<EnumLiteral, T> a4,
			Func<BlobLiteral, T> a5)
		{
			var self = this;
			var t1 = self as BooleanLiteral; if (t1 != null) return a1(t1);
			var t2 = self as StringLiteral; if (t2 != null) return a2(t2);
			var t3 = self as NumberLiteral; if (t3 != null) return a3(t3);
			var t4 = self as EnumLiteral; if (t4 != null) return a4(t4);
			var t5 = self as BlobLiteral; if (t5 != null) return a5(t5);
			throw new NonExhaustiveMatch();
		}

		public void Match(
			Action<BooleanLiteral> a1,
			Action<StringLiteral> a2,
			Action<NumberLiteral> a3,
			Action<EnumLiteral> a4,
			Action<BlobLiteral> a5)
		{
			var self = this;
			var t1 = self as BooleanLiteral; if (t1 != null) { a1(t1); return; }
			var t2 = self as StringLiteral; if (t2 != null) { a2(t2); return; }
			var t3 = self as NumberLiteral; if (t3 != null) { a3(t3); return; }
			var t4 = self as EnumLiteral; if (t4 != null) { a4(t4); return; }
			var t5 = self as BlobLiteral; if (t5 != null) { a5(t5); return; }
			throw new NonExhaustiveMatch();
		}

		public static void Write(Literal literal, BinaryWriter writer)
		{
			writer.Write(literal.LiteralId);
			literal.WriteLiteral(writer);
		}

		protected override void WriteExpression(BinaryWriter writer)
		{
			Literal.Write(this, writer);
		}

		new public static Literal Read(BinaryReader reader)
		{
			var token = reader.ReadChar();
			switch (token)
			{
				case LiteralIdRegistry.BooleanLiteral: return BooleanLiteral.Read(reader);
				case LiteralIdRegistry.StringLiteral: return StringLiteral.Read(reader);
				case LiteralIdRegistry.NumberLiteral: return NumberLiteral.Read(reader);
				case LiteralIdRegistry.EnumLiteral: return EnumLiteral.Read(reader);
				case LiteralIdRegistry.BlobLiteral: return BlobLiteral.Read(reader);
			}
			throw new InvalidDataException();
		}
	}
}