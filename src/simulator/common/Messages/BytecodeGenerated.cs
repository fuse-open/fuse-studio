using System.IO;
using Uno;
using Uno.Collections;

namespace Outracks.Simulator.Protocol
{
	using Bytecode;

	public sealed class BytecodeGenerated : IBinaryMessage
	{
		public static readonly string MessageType = "BytecodeGenerated";

		public string Type { get { return MessageType; } }

		public ProjectBytecode Bytecode { get; private set; }

		public BytecodeGenerated(ProjectBytecode bytecode)
		{
			Bytecode = bytecode;
		}

		BytecodeGenerated() { }

		public static string BinaryFormat
		{
			get { return "bc4"; } // change this whenever write changes
		}

		public void WriteDataTo(BinaryWriter writer)
		{
			Bytecode.WriteDataTo(writer);
		}

		public static BytecodeGenerated ReadDataFrom(BinaryReader reader)
		{
			var bytecode = ProjectBytecode.ReadDataFrom(reader);
			return new BytecodeGenerated(bytecode);
		}

		public override string ToString()
		{
			return "BytecodeGenerated: " + Bytecode.Reify.ToString();
		}
	}
}