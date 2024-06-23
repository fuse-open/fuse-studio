using System.IO;
using Uno;
using Outracks.Simulator.Bytecode;

namespace Outracks.Simulator.Protocol
{
	public sealed class Error : IBinaryMessage
	{
		public static readonly string MessageType = "Error";

		public string Type { get { return MessageType; } }

		public ExceptionInfo Exception { get; private set; }

		public Error(ExceptionInfo exception)
		{
			Exception = exception;
		}
		Error() { }

		public void WriteDataTo(BinaryWriter writer)
		{
			ExceptionInfo.Write(Exception, writer);
		}

		public static Error ReadDataFrom(BinaryReader reader)
		{
			debug_log "Read Error";

			var exception = ExceptionInfo.Read(reader);
			return new Error(exception);
		}

		public override string ToString()
		{
			return "Error: " + Exception.Message.ToString();
		}
	}
}