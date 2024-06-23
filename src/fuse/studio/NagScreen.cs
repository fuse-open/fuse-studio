using System;
using System.Diagnostics;
using Outracks.Fuse.Auth;
using Outracks.Fusion;
using Outracks.Fusion.Dialogs;

namespace Outracks.Fuse
{
	static class NagScreen
	{
		static bool IsOpen;
		static int ReifyCount;

		static ILicenseState _state;

		public static void Initialize(ILicenseState state)
		{
			_state = state;
		}

		public static void Update()
		{
			try
			{
				if (!Condition())
					return;

				try
				{
					IsOpen = true;

					if (MessageBox.ShowConfirm(Strings.NagScreen_Text + "\n\n" + 
						                           Strings.NagScreen_Question,
						                       Strings.NagScreen_Caption,
						                       MessageBoxType.Information))
						Process.Start(WebLinks.SignIn);
				}
				finally
				{
					IsOpen = false;
				}
			}
			catch (Exception e)
			{
				Console.Error.WriteLine(e);
			}
		}

		static bool Condition()
		{
			if (IsOpen)
				return false;

			return _state == null || ++ReifyCount % 8 == 3 && !_state.IsLicenseValid();
		}
	}
}
