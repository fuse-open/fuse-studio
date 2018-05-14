using System;
using System.IO;

namespace Outracks.Simulator.Protocol
{
	public class ReifyRequired : Exception, IBinaryMessage
	{
		public static readonly string MessageType = "ReifyRequired";
		public string Type { get { return MessageType; } }

		public ReifyRequired() : base("Reify is required")
		{
		}

		public void WriteDataTo(BinaryWriter writer)
		{
		}

		public static ReifyRequired ReadDataFrom(BinaryReader reader)
		{
			return new ReifyRequired();
		}
	}
}