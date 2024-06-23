using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Outracks.Fuse.Net;
using Outracks.Fuse.Protocol;
using Outracks.Fuse.Protocol.Auth;
using Outracks.IO;

namespace Outracks.Fuse.Auth
{
	public class License : ILicense
	{
		readonly BehaviorSubject<string> _notification = new BehaviorSubject<string>(null);
		readonly BehaviorSubject<string> _session = new BehaviorSubject<string>(null);
		readonly BehaviorSubject<LicenseStatus> _status = new BehaviorSubject<LicenseStatus>(LicenseStatus.Unregistered);
		readonly BehaviorSubject<LicenseData> _data = new BehaviorSubject<LicenseData>(LicenseData.Null);
		readonly Storage _storage;

		public bool IsValidValue => _status.Value.IsValid();

		public bool IsRemote { get; }
		public IObservable<bool> IsValid { get; }
		public IObservable<bool> IsExpired { get; }
		public IObservable<bool> IsRegistered { get; }
		public IObservable<bool> IsTrial { get; }
		public IObservable<int> DaysLeft { get; }
		public IObservable<string> Status { get; }
		public IObservable<string> Type { get; }
		public IObservable<string> Name { get; }
		public IObservable<string> Email { get; }
		public IObservable<string> Company { get; }
		public IObservable<string> Authority { get; }
		public IObservable<string> Expires { get; }
		public IObservable<string> Notification => _notification;
		public IObservable<string> Session => _session;
		public IObservable<LicenseData> Data => _data;

		public event EventHandler<LicenseStatus> Updated;
		public event EventHandler Error;

		public License(AbsoluteDirectoryPath userData, bool remote)
		{
			_storage = Storage.Get(userData);
			IsRemote = remote;
			IsValid = _status.Select(value => value.IsValid());
			IsExpired = _status.Select(value => value == LicenseStatus.Expired);
			IsRegistered = _data.Select(value => !string.IsNullOrEmpty(value.Name));
			IsTrial = _data.Select(value => value.License != null && value.License.Contains("Trial"));
			DaysLeft = _data.Select(value => Math.Max(0, (int)((value.UtcExpires - DateTime.UtcNow).TotalDays + 0.5)));
			Status = _status.Select(value => value == LicenseStatus.OK ? null : value.ToString().ToUpper());
			Type = _status.CombineLatest(_data).Select(tuple => tuple.Item2.License + (
						tuple.Item1 == LicenseStatus.OK ? "" : " [" + tuple.Item1.ToString().ToUpper() + "]"));
			Name = _data.Select(value => value.Name);
			_notification.Subscribe(s => {
				if (!string.IsNullOrEmpty(s))
					Console.WriteLine(s);
			});
			Load(comesFromBroadcast: false);
		}

		public LicenseStatus ActivateLicense(string license, IMessagingService daemon)
		{
			_storage.StoreSession(null);
			return string.IsNullOrEmpty(license)
				? ActivateInner(null, daemon)
				: GetRemoteSession(license)
					.ContinueWith(remoteSession => {
						if (remoteSession.Exception != null)
						{
							Console.Error.WriteLine(remoteSession.Exception);
							daemon.Broadcast(new LicenseEvent { Error = true });
							return LicenseStatus.Expired;
						}

						var session = remoteSession.Result;
						return ActivateSession(session, daemon);
					}).Result;
		}

		public LicenseStatus ActivateSession(string session, IMessagingService daemon)
		{
			_storage.StoreSession(session);
			return string.IsNullOrEmpty(session)
				? ActivateInner(null, daemon)
				: GetRemoteLicense(session)
					.ContinueWith(remoteLicense => {
						if (remoteLicense.Exception != null)
						{
							Console.Error.WriteLine(remoteLicense.Exception);
							daemon.Broadcast(new LicenseEvent { Error = true });
							return LicenseStatus.Expired;
						}

						var license = remoteLicense.Result;

						if (string.IsNullOrEmpty(license))
							return LicenseStatus.Expired;

						return ActivateInner(license, daemon);
					}).Result;
		}

		public LicenseStatus ActivateInner(string license, IMessagingService daemon)
		{
			var status = Verify(LicenseData.Decode(license), DateTime.UtcNow);
			_storage.StoreLicense(license);
			daemon.Broadcast(new LicenseEvent());
			return status;
		}

		public void Deactivate(IMessagingService daemon)
		{
			_storage.StoreSession(null);
			_storage.StoreLicense(null);
			daemon.Broadcast(new LicenseEvent());
		}

		public void Subscribe(AbsoluteFilePath fuse, IMessagingService daemon)
		{
			UriHandler.Register(fuse);
			daemon.BroadcastedEvents<LicenseEvent>(false)
				.Subscribe(e => {
					if (e.Error)
					{
						Error?.Invoke(this, new EventArgs());
						return;
					}
					Load(comesFromBroadcast: true);
				});
		}

		void Load(bool comesFromBroadcast)
		{
			try
			{
				var session = _storage.LoadSession();
				_session.OnNext(session);

				// Sync license from remote server when signed in.
				if (IsRemote && !string.IsNullOrEmpty(session))
				{
					Console.WriteLine($"Fetching license for session {session}");
					GetRemoteLicense(session).ContinueWith(remoteLicense => {
						if (remoteLicense.Exception != null)
						{
							Console.Error.WriteLine(remoteLicense.Exception);
							_storage.StoreLicense("EXPIRED");
							return;
						}

						_storage.StoreLicense(remoteLicense.Result);
						LoadInner(remoteLicense.Result, comesFromBroadcast);
					});
				}

				var license = _storage.LoadLicense();
				LoadInner(license, comesFromBroadcast);

				// Request a session anonymously (free trial period).
				if (IsRemote && string.IsNullOrEmpty(license) && string.IsNullOrEmpty(session))
				{
					Console.WriteLine($"Fetching session for device {Hardware.UID}");
					GetRemoteSession().ContinueWith(remoteSession => {
						if (remoteSession.Exception != null)
						{
							Console.Error.WriteLine(remoteSession.Exception);
							return;
						}

						session = remoteSession.Result;
						_storage.StoreSession(session);

						if (!string.IsNullOrEmpty(session))
							Load(comesFromBroadcast);
					});
				}
			}
			catch (Exception e)
			{
				Console.Error.WriteLine(e);
			}
		}

		bool _lastComesFromBroadcast;
		string _lastLicense;
		DateTime _lastTime;

		void LoadInner(string license, bool comesFromBroadcast)
		{
			try
			{
				// Early-out if we have recently checked the exact same license.
				if (_lastComesFromBroadcast == comesFromBroadcast &&
					_lastLicense == license &&
					DateTime.UtcNow.Subtract(_lastTime).TotalSeconds < 30)
				{
					if (IsRemote)
						Console.WriteLine($"Already checked license {license.GetHashCode()}");

					return;
				}

				if (IsRemote)
					Console.WriteLine($"Checking license {license.GetHashCode()}");

				var isExpired = license == "EXPIRED";
				var data = isExpired ? LicenseData.Null : LicenseData.Decode(license);
				var status = isExpired ? LicenseStatus.Expired : Verify(data, DateTime.UtcNow);
				_data.OnNext(data);
				_status.OnNext(status);

				_lastLicense = license;
				_lastComesFromBroadcast = comesFromBroadcast;
				_lastTime = DateTime.UtcNow;
				
				// Not all instances need to ping the remote server.
				if (!IsRemote)
					return;

				GetRemoteUtc(status).ContinueWith(utc => {
					if (utc.Exception != null)
					{
						if (status == LicenseStatus.OK)
							_status.OnNext(status = LicenseStatus.Offline);

						Console.Error.WriteLine(utc.Exception);
					}
					else if (utc.Result > data.UtcExpires || utc.Result < data.UtcIssued)
					{
						if (status == LicenseStatus.OK)
							_status.OnNext(status = LicenseStatus.Expired);
					}
					else if (data != LicenseData.Null)
					{
						if (status == LicenseStatus.Expired)
							_status.OnNext(status = LicenseStatus.OK);
					}

					if (comesFromBroadcast)
					{
						_notification.OnNext($"License updated: {status}.");
						Updated?.Invoke(this, status);
					}
				});
			}
			catch (Exception e)
			{
				_data.OnNext(LicenseData.Null);
				_status.OnNext(LicenseStatus.Unregistered);
				_notification.OnNext("An error occurred while updating license.");
				Console.Error.WriteLine(e);
			}
		}

		LicenseStatus Verify(LicenseData license, DateTime utcNow)
		{
			if (string.IsNullOrEmpty(license.Name))
				return LicenseStatus.Unregistered;

			if (!string.IsNullOrEmpty(license.Device) && license.Device != Hardware.UID)
				return LicenseStatus.Expired;

			if (utcNow > license.UtcExpires || utcNow < license.UtcIssued)
				return LicenseStatus.Expired;

			return LicenseStatus.OK;
		}

		async Task<DateTime> GetRemoteUtc(LicenseStatus status)
		{
			using (var client = new FuseWebClient(status))
			{
				var body = await client.DownloadStringTaskAsync(WebLinks.Utc);
				return DateTime.Parse(body);
			}
		}

		async Task<string> GetRemoteLicense(string session)
		{
			using (var client = new FuseWebClient(LicenseStatus.OK))
			{
				var body = await client.DownloadStringTaskAsync(WebLinks.License +
					"?session=" + Uri.EscapeDataString(session) +
					"&device=" + Uri.EscapeDataString(Hardware.UID));
				return body?.Trim();
			}
		}

		async Task<string> GetRemoteSession(string license = null)
		{
			using (var client = new FuseWebClient(LicenseStatus.OK))
			{
				var body = await client.DownloadStringTaskAsync(WebLinks.Session +
					"?device=" + Uri.EscapeDataString(Hardware.UID) + (
					!string.IsNullOrEmpty(license)
						? "&license=" + Uri.EscapeDataString(license)
						: string.Empty
					));
				return body?.Trim();
			}
		}
	}
}
