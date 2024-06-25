using System.IO;
using Uno;
using Outracks.Simulator.Bytecode;

namespace Outracks.Simulator.Protocol
{
	public sealed class ExceptionInfo
	{
		public readonly TypeName Type;
		public readonly string Message;
		public readonly string StackTrace;
		public readonly ImmutableList<ExceptionInfo> InnerExceptions;

		public ExceptionInfo(
			TypeName type,
			string message,
			string stackTrace,
			ImmutableList<ExceptionInfo> innerExceptions)
		{
			Type = type;
			Message = message;
			StackTrace = stackTrace;
			InnerExceptions = innerExceptions;
		}
		public static ExceptionInfo Capture(Exception e)
		{
			return new ExceptionInfo(
				TypeName.Parse(e.GetType().ToString()),
				e.Message,
				e.StackTrace,
				ImmutableList<ExceptionInfo>.Empty);
			//new ImmutableList<ExceptionInfo>(
				//	e.InnerException == null
				//		? new Error[0]
				//		: new Error[] { ExceptionInfo.Capture(e.InnerException) }));
		}

		public static void Write(ExceptionInfo e, BinaryWriter writer)
		{
			TypeName.Write(e.Type, writer);
			writer.Write(e.Message);
			writer.Write(e.StackTrace);
			List.Write<ExceptionInfo>(writer, e.InnerExceptions, ExceptionInfo.Write);
		}

		public static ExceptionInfo Read(BinaryReader reader)
		{
			var type = TypeName.Read(reader);
			var message = reader.ReadString();
			var stackTrace = reader.ReadString();
			var innerExceptions = List.Read<ExceptionInfo>(reader, ExceptionInfo.Read);
			return new ExceptionInfo(type, message, stackTrace, innerExceptions);
		}
	}
}