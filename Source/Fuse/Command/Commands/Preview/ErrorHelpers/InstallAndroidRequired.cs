using Outracks.Fuse.Components;
using Outracks.Fuse.Setup;
using Fuse.Preview;
using Uno;
using Uno.Build;

namespace Outracks.Fuse
{
	public class InstallAndroidRequired : IErrorHelper
	{
		public void OnBuildFailed(BuildResult build)
		{
			if (build.Target.Identifier != "Android")
				return;

			var isAndroidStepIssue = build.Log.GetErrorSummary().Contains("E0200");

			if (isAndroidStepIssue && new AndroidInstaller().Status != ComponentStatus.Installed)
			{
				build.Log.Error(Source.Unknown, "F0001", "Looks like Android is not installed. Please run 'fuse install android' first.");
			}
		}
	}
}