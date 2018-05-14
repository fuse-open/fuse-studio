using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;

namespace Fuse.Installer.Gui.Domain
{
	public class Apply
	{
		public static void Register(FuseBootstrapperApplication a)
		{
			a.ApplyBegin += OnApplyBegin;

			a.Elevate += OnElevate;

			a.RegisterBegin += OnRegisterBegin;
			a.RegisterComplete += OnRegisterComplete;

			a.UnregisterBegin += OnUnregisterBegin;
			a.UnregisterComplete += OnUnregisterComplete;

			a.CacheBegin += OnCacheBegin;

			a.CachePackageBegin += OnCachePackageBegin;

			a.CacheAcquireBegin += OnCacheAcquireBegin;
			a.CacheAcquireProgress += OnCacheAcquireProgress;

			a.ResolveSource += OnResolveSource;

			a.CacheAcquireComplete += OnCacheAcquireComplete;

			a.CacheVerifyBegin += OnCacheVerifyBegin;
			a.CacheVerifyComplete += OnCacheVerifyComplete;

			a.CachePackageComplete += OnCachePackageComplete;

			a.CacheComplete += OnCacheComplete;

			a.ExecuteBegin += OnExecuteBegin;
			a.ExecutePackageBegin += OnExecutePackageBegin;
			a.ExecutePatchTarget += OnExecutePatchTarget;

			a.Error += OnError;
			a.Progress += OnProgress;

			a.ExecuteMsiMessage += OnExecuteMsiMessage;
			a.ExecuteFilesInUse += OnExecuteFilesInUse;
			a.ExecutePackageComplete += OnExecutePackageComplete;
			a.ExecuteComplete += OnExecuteComplete;

			a.RestartRequired += OnRestartRequired;

			a.ApplyComplete += OnApplyComplete;
		}

		/// Fired when the engine has begun installing the bundle.
		static void OnApplyBegin(object sender, ApplyBeginEventArgs e)
		{
			Logger.Instance.Trace("" + e.Result);
		}

		/// Fired when the engine is about to start the elevated process.
		static void OnElevate(object sender, ElevateEventArgs e)
		{
			Logger.Instance.Trace("" + e.Result);
		}

		/// Fired when the engine has begun registering the location and visibility of the bundle.
		static void OnRegisterBegin(object sender, RegisterBeginEventArgs e)
		{
			Logger.Instance.Trace("");
		}

		/// Fired when the engine has completed registering the location and visibility of the bundle.
		static void OnRegisterComplete(object sender, RegisterCompleteEventArgs e)
		{
			Logger.Instance.Trace("");
		}

		/// Fired when the engine has begun removing the registration for the location and visibility of the bundle.
		static void OnUnregisterBegin(object sender, UnregisterBeginEventArgs e)
		{
			Logger.Instance.Trace("");
		}

		/// Fired when the engine has completed removing the registration for the location and visibility of the bundle.
		static void OnUnregisterComplete(object sender, UnregisterCompleteEventArgs e)
		{
			Logger.Instance.Trace("" + e.Status);
		}

		/// Fired when the engine has begun caching the installation sources.
		static void OnCacheBegin(object sender, CacheBeginEventArgs e)
		{
			Logger.Instance.Trace("");
		}

		/// Fired when the engine has begun caching a specific package.
		static void OnCachePackageBegin(object sender, CachePackageBeginEventArgs e)
		{
			Logger.Instance.Trace("" + e.PackageId + " " + e.CachePayloads + " " + e.PackageCacheSize + " " + e.Result);
		}

		/// Fired when the engine has begun acquiring the installation sources.
		static void OnCacheAcquireBegin(object sender, CacheAcquireBeginEventArgs e)
		{
			Logger.Instance.Trace("");
		}

		/// Fired when the engine has progress acquiring the installation sources.
		static void OnCacheAcquireProgress(object sender, CacheAcquireProgressEventArgs e)
		{
			Logger.Instance.Trace("" + e.PackageOrContainerId + " " + e.PayloadId + " " + e.Result + " " + e.Progress + "/" + e.Total + " " + e.OverallPercentage);
		}

		/// Fired by the engine to allow the user experience to change the source
		///             using <see cref="M:Engine.SetLocalSource"/> or <see cref="M:Engine.SetDownloadSource"/>.
		static void OnResolveSource(object sender, ResolveSourceEventArgs e)
		{
			/*Logger.Instance.Trace("" + e.PackageOrContainerId + " " + e.PayloadId + " " + e.Result + " " + e.DownloadSource);
			int retries;
			_downloadRetries.TryGetValue(e.PackageOrContainerId, out retries);
			_downloadRetries[e.PackageOrContainerId] = retries + 1;

			e.Result = retries < 3 && !string.IsNullOrEmpty(e.DownloadSource) ? Result.Download : Result.Ok;
			Logger.Instance.Trace("" + e.PackageOrContainerId + " " + e.PayloadId + " " + e.Result + " " + e.LocalSource);*/
			e.Result = Result.Download;
		}

		/// Fired when the engine has completed the acquisition of the installation sources.
		static void OnCacheAcquireComplete(object sender, CacheAcquireCompleteEventArgs e)
		{
			Logger.Instance.Trace("" + e.PackageOrContainerId + " " + e.PayloadId + " " + e.Result + " " + e.Status);
		}

		/// Fired when the engine begins the verification of the acquired installation sources.
		static void OnCacheVerifyBegin(object sender, CacheVerifyBeginEventArgs e)
		{
			Logger.Instance.Trace("");
		}

		/// Fired when the engine complete the verification of the acquired installation sources.
		static void OnCacheVerifyComplete(object sender, CacheVerifyCompleteEventArgs e)
		{
			Logger.Instance.Trace("" + e.PackageId);
		}

		/// Fired when the engine has completed caching a specific package.
		static void OnCachePackageComplete(object sender, CachePackageCompleteEventArgs e)
		{
			Logger.Instance.Trace("" + e.PackageId + " " + e.Result + " " + e.Status);
		}

		/// Fired after the engine has cached the installation sources.
		static void OnCacheComplete(object sender, CacheCompleteEventArgs e)
		{
			Logger.Instance.Trace("" + e.Status);
		}

		/// Fired when the engine has begun installing packages.
		static void OnExecuteBegin(object sender, ExecuteBeginEventArgs e)
		{
			Logger.Instance.Trace("");
		}

		/// Fired when the engine has begun installing a specific package.
		static void OnExecutePackageBegin(object sender, ExecutePackageBeginEventArgs e)
		{
			Logger.Instance.Trace("");
		}

		/// Fired when the engine executes one or more patches targeting a product.
		static void OnExecutePatchTarget(object sender, ExecutePatchTargetEventArgs e)
		{
			Logger.Instance.Trace("");
		}

		/// Fired when the engine has encountered an error.
		static void OnError(object sender, ErrorEventArgs e)
		{
			Logger.Instance.Trace("" + e.ErrorCode + " " + e.ErrorType + " " + e.ErrorMessage + " " + e.PackageId + e.Data);
		}

		/// Fired when the engine has changed progress for the bundle installation.
		static void OnProgress(object sender, ProgressEventArgs e)
		{
			Logger.Instance.Trace("");
		}

		/// Fired when Windows Installer sends an installation message.
		static void OnExecuteMsiMessage(object sender, ExecuteMsiMessageEventArgs e)
		{
			Logger.Instance.Trace("");
		}

		/// Fired when Windows Installer sends a files in use installation message.
		static void OnExecuteFilesInUse(object sender, ExecuteFilesInUseEventArgs e)
		{
			Logger.Instance.Trace("");
		}

		/// Fired when the engine has completed installing a specific package.
		static void OnExecutePackageComplete(object sender, ExecutePackageCompleteEventArgs e)
		{
			Logger.Instance.Trace("");
		}

		/// Fired when the engine has completed installing packages.
		static void OnExecuteComplete(object sender, ExecuteCompleteEventArgs e)
		{
			Logger.Instance.Trace("");
		}

		/// Fired by the engine to request a restart now or inform the user a manual restart is required later.
		static void OnRestartRequired(object sender, RestartRequiredEventArgs e)
		{
			Logger.Instance.Trace("");
		}

		/// Fired when the engine has completed installing the bundle.
		static void OnApplyComplete(object sender, ApplyCompleteEventArgs e)
		{
			Logger.Instance.Trace("" + e.Result + " " + e.Status + " " + e.Restart);
		}
	}
}