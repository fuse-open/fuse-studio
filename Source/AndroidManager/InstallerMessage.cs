using System;

namespace Outracks.AndroidManager
{
	public abstract class InstallerEvent
	{
	}

	public class InstallerStep : InstallerEvent
	{
		public readonly string Step;

		public InstallerStep(string step)
		{
			Step = step;
		}

		public override string ToString()
		{
			return Step;
		}
	}

	public enum InstallerMessageType
	{
		Verbose,
		Warning,
		Error
	}

	public class InstallerError : Exception
	{
		public InstallerError(Exception e)
			: base("An error occurred during installation", e)
		{
		}

		public InstallerError(string message)
			: base("An error occurred during installation: " + message)
		{
		}
	}

	public class InstallerMessage : InstallerEvent
	{
		public InstallerMessageType Type;
		public readonly string Message;

		public InstallerMessage(string message, InstallerMessageType type = InstallerMessageType.Verbose)
		{
			Message = message;
			Type = type;
		}

		public override string ToString()
		{
			return Message;
		}
	}
}