namespace Fuse.Simulator
{
	static class Logger
	{
		public static void Info(string message)
		{
			if defined(!RELEASE)
				debug_log(message);
		}

		public static void Error(string message)
		{
			if defined(!RELEASE)
				debug_log(message);
		}
	}
}