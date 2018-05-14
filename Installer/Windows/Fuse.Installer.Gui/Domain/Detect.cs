using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;

namespace Fuse.Installer.Gui.Domain
{
	public class Detect
	{
		private static BootstrapperApplication _bootstrapperApplication;

		public static void Register(FuseBootstrapperApplication a)
		{
			_bootstrapperApplication = a;
			a.DetectBegin += OnDetectBegin;
			a.DetectForwardCompatibleBundle += OnDetectForwardCompatibleBundle;
			a.DetectUpdateBegin += OnDetectUpdateBegin;
			a.DetectUpdate += OnDetectUpdate;
			a.DetectUpdateComplete += OnDetectUpdateComplete;
			a.DetectPriorBundle += OnDetectPriorBundle;
			a.DetectRelatedBundle += OnDetectRelatedBundle;
			a.DetectPackageBegin += OnDetectPackageBegin;
			a.DetectCompatiblePackage += OnDetectCompatiblePackage;
			a.DetectRelatedMsiPackage += OnDetectRelatedMsiPackage;
			a.DetectTargetMsiPackage += OnDetectTargetMsiPackage;
			a.DetectMsiFeature += OnDetectMsiFeature;
			a.DetectPackageComplete += OnDetectPackageComplete;
			a.DetectComplete += OnDetectComplete;
		}

		/// Fired when the overall detection phase has begun.
		static void OnDetectBegin(object sender, DetectBeginEventArgs e)
		{
			Logger.Instance.Trace("");
		}

		/// Fired when a forward compatible bundle is detected.
		static void OnDetectForwardCompatibleBundle(object sender, DetectForwardCompatibleBundleEventArgs e)
		{
			Logger.Instance.Trace("");
		}

		/// Fired when the update detection phase has begun.
		static void OnDetectUpdateBegin(object sender, DetectUpdateBeginEventArgs e)
		{
			Logger.Instance.Trace("");
		}

		/// Fired when the update detection has found a potential update candidate.
		static void OnDetectUpdate(object sender, DetectUpdateEventArgs e)
		{
			Logger.Instance.Trace("");
		}

		/// Fired when the update detection phase has completed.
		static void OnDetectUpdateComplete(object sender, DetectUpdateCompleteEventArgs e)
		{
			Logger.Instance.Trace("");
		}

		/// Fired when the detection for a prior bundle has begun.
		static void OnDetectPriorBundle(object sender, DetectPriorBundleEventArgs e)
		{
			Logger.Instance.Trace("");
		}

		/// Fired when a related bundle has been detected for a bundle.
		static void OnDetectRelatedBundle(object sender, DetectRelatedBundleEventArgs e)
		{
			Logger.Instance.Trace("");
		}

		/// Fired when the detection for a specific package has begun.
		static void OnDetectPackageBegin(object sender, DetectPackageBeginEventArgs e)
		{
			Logger.Instance.Trace("");
		}

		/// Fired when a package was not detected but a package using the same provider key was.
		static void OnDetectCompatiblePackage(object sender, DetectCompatiblePackageEventArgs e)
		{
			Logger.Instance.Trace("");
		}

		/// Fired when a related MSI package has been detected for a package.
		static void OnDetectRelatedMsiPackage(object sender, DetectRelatedMsiPackageEventArgs e)
		{
			Logger.Instance.Trace("");
		}

		/// Fired when an MSP package detects a target MSI has been detected.
		static void OnDetectTargetMsiPackage(object sender, DetectTargetMsiPackageEventArgs e)
		{
			Logger.Instance.Trace("");
		}

		/// Fired when a feature in an MSI package has been detected.
		static void OnDetectMsiFeature(object sender, DetectMsiFeatureEventArgs e)
		{
			Logger.Instance.Trace("");
		}

		/// Fired when the detection for a specific package has completed.
		static void OnDetectPackageComplete(object sender, DetectPackageCompleteEventArgs e)
		{
			Logger.Instance.Trace("");
		}

		/// Fired when the detection phase has completed.
		static void OnDetectComplete(object sender, DetectCompleteEventArgs e)
		{
			Logger.Instance.Trace("");
			if (_bootstrapperApplication.Command.Action == LaunchAction.Uninstall 
				&& _bootstrapperApplication.Command.Display == Display.Embedded)
			{
				Logger.Instance.Trace("Schedule uninstall action when triggered from another bundle");
				_bootstrapperApplication.Engine.Plan(LaunchAction.Uninstall);
			}
		}
	}
}