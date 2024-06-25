using System;
using Outracks.Fuse.Protocol;
using Outracks.IO;

namespace Outracks.Fuse.Auth
{
	public interface ILicense
	{
		bool IsValidValue { get; }

		bool IsRemote { get; }
		IObservable<bool> IsValid { get; }
		IObservable<bool> IsExpired { get; }
		IObservable<bool> IsRegistered { get; }
		IObservable<bool> IsTrial { get; }
		IObservable<int> DaysLeft { get; }
		IObservable<string> Status { get; }
		IObservable<string> Type { get; }
		IObservable<string> Name { get; }
		IObservable<string> Notification { get; }
		IObservable<string> Session { get; }
		IObservable<LicenseData> Data { get; }

		event EventHandler<LicenseStatus> Updated;
		event EventHandler Error;

		LicenseStatus ActivateLicense(string license, IMessagingService daemon);
		LicenseStatus ActivateSession(string session, IMessagingService daemon);
		void Deactivate(IMessagingService daemon);

		void Subscribe(AbsoluteFilePath fuse, IMessagingService daemon);
	}
}
