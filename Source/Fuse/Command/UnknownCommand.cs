using System;

namespace Outracks.Fuse
{
	public class UnknownCommand : Exception
	{
		public readonly string Command;

		public UnknownCommand(string command)
		{
			Command = command;
		}
	}
}