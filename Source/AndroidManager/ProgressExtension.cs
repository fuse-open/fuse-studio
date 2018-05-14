using System;

namespace Outracks.AndroidManager
{
	public static class ProgressExtensions
	{	
		public static void DoInstallerStep(this IProgress<InstallerEvent> installerEvent, string message, Action action, Action<Exception> rollbackAction)
		{
			try
			{
				installerEvent.Report(new InstallerStep(message));
				action();
			}
			catch (Exception e)
			{
				rollbackAction(e);
				if (e is InstallerError)
					throw;
				else
					throw new InstallerError(e);
			}
		}
	}
}