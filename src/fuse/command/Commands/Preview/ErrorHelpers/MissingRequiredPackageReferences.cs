using Fuse.Preview;
using Uno.Build;

namespace Outracks.Fuse
{
	public class MissingRequiredPackageReferences : IErrorHelper
	{
		public void OnBuildFailed(BuildResult build)
		{
			/*
			var errorSummary = build.Log.GetErrorSummary();

			var isMissingApp =
				.OfType<BuildIssueDetected>()
				.Any(issue =>
					issue.Message.Contains("'Fuse' does not contain type or namespace 'App'") ||
					issue.Message.Contains("There is no identifier named 'App' accessible in this scope."));

			var requiredPackage =
				build.Target == BuildTarget.iOS ? "Fuse.iOS" :
				build.Target == BuildTarget.Android ? "Fuse.Android" :
				"Fuse.Desktop";

			var isMissingPlatformPackage =
				build.HasPackageReference(requiredPackage) == false;

			if (isMissingApp && isMissingPlatformPackage)
			{
				build.AddError(
					code: "F1001",
					message: "The required package reference '" + requiredPackage + "' is missing from '" + build.ProjectPath.NativePath + "'",
					source: build.ProjectSource);
			}
			 */
		}

	}
}