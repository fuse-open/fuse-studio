using System;
using System.Globalization;
using System.Threading;

namespace Outracks.Extensions
{
	public static class SetInvariantCultureExtension
	{
		public static void SetInvariantCulture(this Thread thread)
		{
			try
			{
				CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
				CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
			}
			// ReSharper disable once EmptyGeneralCatchClause
			catch (Exception)
			{
				//Not supported until mono 3.2.7
			}

			thread.CurrentCulture = CultureInfo.InvariantCulture;
			thread.CurrentUICulture = CultureInfo.InvariantCulture;
		}
	}
}
