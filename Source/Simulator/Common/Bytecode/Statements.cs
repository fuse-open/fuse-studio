using System.IO;
using Uno;

namespace Outracks.Simulator.Bytecode
{
	public class NoOperation : Statement
	{
		public readonly Optional<string> Description;

		public NoOperation(Optional<string> description = default(Optional<string>))
		{
			Description = description;
		}

		public override string ToString()
		{
			return "// " + Description;
		}

		protected override void WriteStatement(BinaryWriter writer)
		{
			Optional.Write(writer, Description, writer.Write);
		}

		new public static NoOperation Read(BinaryReader reader)
		{
			return new NoOperation(Optional.Read(reader, Serialization.ReadString));
		}

		public override char StatementId { get { return StatementIdRegistry.NoOperation; } }
	}

	public class Return : Statement
	{
		public readonly Expression Value;
		public Return(Expression value)
		{
			Value = value;
		}

		public override string ToString()
		{
			return "return " + Value;
		}

		protected override void WriteStatement(BinaryWriter writer)
		{
			Expression.Write(Value, writer);
		}

		new public static Return Read(BinaryReader reader)
		{
			return new Return(Expression.Read(reader));
		}

		public override char StatementId { get { return StatementIdRegistry.Return; } }
	}


}