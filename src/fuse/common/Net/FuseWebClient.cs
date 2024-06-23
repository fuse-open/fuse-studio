using Outracks.Fuse.Auth;
using System;
using System.IO;
using System.Net;

namespace Outracks.Fuse.Net
{
	public class FuseWebClient : WebClient
	{
		readonly string UserAgent = "fuse/" +
			SystemInfoFactory.GetBuildVersion(typeof(FuseWebClient).Assembly) +
			" (" + Uno.Diagnostics.PlatformDetection.SystemString + ")";

		public FuseWebClient()
		{
			Credentials = CredentialCache.DefaultCredentials;
		}

		public FuseWebClient(LicenseStatus status)
			: this()
		{
			if (status != LicenseStatus.OK)
				UserAgent += " " + status;
		}

		protected override WebRequest GetWebRequest(Uri address)
		{
#if DEBUG
			Console.WriteLine(nameof(FuseWebClient) + ": " + address);
#endif
			var request = (HttpWebRequest)WebRequest.Create(address);
			request.AllowAutoRedirect = true;
			request.UserAgent = UserAgent;
			return request;
		}
	}
}
