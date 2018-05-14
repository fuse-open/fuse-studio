using System;
using System.IO;
using Uno;

namespace Outracks.Simulator.Protocol
{
	using Bytecode;

	public sealed class UnhandledException : IBinaryMessage
	{
		public static readonly string MessageType = "UnhandledException";

		public string Type { get { return MessageType; } }

		public string DeviceId { get; private set; }
		public string DeviceName { get; private set; }
		public string Message { get; private set; }
		public string StackTrace { get; private set; }
		public string ExceptionType { get; private set; }

		public UnhandledException(
			string deviceId,
			string deviceName,
			string message,
			string stackTrace,
			string type)
		{
			DeviceId = deviceId;
			DeviceName = deviceName;
			Message = message;
			StackTrace = stackTrace;
			ExceptionType = type;
		}
		UnhandledException() { }
		
		public void WriteDataTo(BinaryWriter writer)
		{
			writer.Write(DeviceId);
			writer.Write(DeviceName);
			writer.Write(Message);
			writer.Write(StackTrace);
			writer.Write(Type);
		}

		public static UnhandledException ReadDataFrom(BinaryReader reader)
		{
			var deviceId = reader.ReadString();
			var deviceName = reader.ReadString();
			var message = reader.ReadString();
			var stackTrace = reader.ReadString();
			var type = reader.ReadString();
			return new UnhandledException(deviceId, deviceName, message, stackTrace, type);
		}
	}

}