using System.IO;
using Uno;
using Outracks.Simulator.Bytecode;

namespace Outracks.Simulator.Protocol
{
	public sealed class BytecodeUpdated : IBinaryMessage
	{
		public static readonly string MessageType = "BytecodeUpdated";

		public string Type { get { return MessageType; } }

		public Lambda Function { get; private set; }

		public BytecodeUpdated(Lambda function)
		{
			Function = function;
		}

		BytecodeUpdated() { }

		public void WriteDataTo(BinaryWriter writer)
		{
			Lambda.Write(Function, writer);
		}

		public static BytecodeUpdated ReadDataFrom(BinaryReader reader)
		{
			var function = Lambda.Read(reader);

			return new BytecodeUpdated(function);
		}

		public override string ToString()
		{
			return "BytecodeUpdated: " + Function.ToString();
		}
	}
}