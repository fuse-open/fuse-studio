namespace Outracks.Fusion
{
	public enum DialogResult
	{
		None = 0,
		Ok = 1,
		Cancel = 2,
		Yes = 6,
		No = 7,
		//Abort,
		//Ignore,
		//Retry,
	}

	public enum MessageBoxButtons
	{
		Ok = 0,
		OkCancel = 1,
		YesNoCancel = 3,
		YesNo = 4,
	}

	public enum MessageBoxType
	{
		Information,
		Error
	}

	public enum MessageBoxDefaultButton
	{
		Default,
		Cancel,
		Ok,
		No,
		Yes
	}

}