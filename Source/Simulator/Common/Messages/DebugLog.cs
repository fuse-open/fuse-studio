using System;
using System.IO;
using Uno;

namespace Outracks.Simulator.Protocol
{
	using Bytecode;

	public sealed class DebugLog : IBinaryMessage
	{
		public static readonly string MessageType = "DebugLog";

		public string Type { get { return MessageType; } }

		public string DeviceId { get; private set; }
		public string DeviceName { get; private set; }
		public string Message { get; private set; }

		public DebugLog(string deviceId, string deviceName, string message)
		{
			DeviceId = deviceId;
			DeviceName = deviceName;
			Message = message;
		}
		DebugLog() { }

		public void WriteDataTo(BinaryWriter writer)
		{
			writer.Write(DeviceId);
			writer.Write(DeviceName);
			writer.Write(Message);
		}

		public static DebugLog ReadDataFrom(BinaryReader reader)
		{
			var deviceId = reader.ReadString();
			var deviceName = reader.ReadString();
			var message = reader.ReadString();
			return new DebugLog(deviceId, deviceName, message);
		}
	}


	public sealed class TriggerProgress : IBinaryMessage
	{
		public static readonly string MessageType = "TriggerProgress";

		public string Type { get { return MessageType; } }
		public double Progress { get; private set; }

		public TriggerProgress(double progress)
		{
			Progress = progress;
		}
		TriggerProgress() { }

		public void WriteDataTo(BinaryWriter writer)
		{
			writer.Write(Progress);
		}

		public static TriggerProgress ReadDataFrom(BinaryReader reader)
		{
			var progress = reader.ReadDouble();
			return new TriggerProgress(progress);
		}
	}
}