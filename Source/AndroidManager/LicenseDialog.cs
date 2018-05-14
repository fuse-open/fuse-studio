namespace Outracks.AndroidManager
{
	public static class LicenseDialog
	{
		public static DialogYesNoResult ShowLicenseDialog(this IDialog dialog, string licenseText)
		{
			return dialog.Start(
				new DialogQuestion<DialogYesNoResult>(licenseText + "\nDo you accept the license?"));
		}
	}
}