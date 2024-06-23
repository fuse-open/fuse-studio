using System;
using System.IO;
using Uno;

namespace Outracks.Simulator.Protocol
{
	using Bytecode;

	public sealed class Ready : IBinaryMessage
	{
		public static readonly string MessageType = "Ready";

		public string Type { get { return MessageType; } }

		public void WriteDataTo(BinaryWriter writer)
		{
		}

		public static Ready ReadDataFrom(BinaryReader reader)
		{
			return new Ready();
		}
	}

	public sealed class RegisterName : IBinaryMessage
	{
		public static readonly string MessageType = "RegisterName";

		public string Type { get { return MessageType; } }

		public string DeviceId { get; private set; }
		public string DeviceName { get; private set; }

		public RegisterName(string deviceId, string deviceName)
		{
			DeviceId = deviceId;
			DeviceName = deviceName;
		}

		RegisterName() {}

		public void WriteDataTo(BinaryWriter writer)
		{
			writer.Write(DeviceId);
			writer.Write(DeviceName);
		}

		public static RegisterName ReadDataFrom(BinaryReader reader)
		{
			var deviceId = reader.ReadString();
			var deviceName = reader.ReadString();
			return new RegisterName(deviceId, deviceName);
		}
	}
}