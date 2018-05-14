using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;

namespace Fuse.Installer.Gui.Domain
{
	public class Plan
	{
		public static void Register(FuseBootstrapperApplication a)
		{
			a.PlanBegin += OnPlanBegin;
			a.PlanRelatedBundle += OnPlanRelatedBundle;

			a.PlanPackageBegin += OnPlanPackageBegin;
			a.PlanCompatiblePackage += OnPlanCompatiblePackage;
			a.PlanTargetMsiPackage += OnPlanTargetMsiPackage;
			a.PlanMsiFeature += OnPlanMsiFeature;
			a.PlanPackageComplete += OnPlanPackageComplete;

			a.PlanComplete += OnPlanComplete;
		}

		/// Fired when the engine has begun planning the installation.
		static void OnPlanBegin(object sender, PlanBeginEventArgs e)
		{
			Logger.Instance.Trace("PackageCount: " + e.PackageCount);
		}

		/// Fired when the engine has begun planning for a related bundle.
		static void OnPlanRelatedBundle(object sender, PlanRelatedBundleEventArgs e)
		{
			Logger.Instance.Trace("" + e.BundleId + " " + e.State + " " + e.Result);
		}

		/// Fired when the engine has begun planning the installation of a specific package.
		static void OnPlanPackageBegin(object sender, PlanPackageBeginEventArgs e)
		{
			Logger.Instance.Trace("" + e.PackageId + " " + e.State + " " + e.Result);
		}

		/// Fired when the engine plans a new, compatible package using the same provider key.
		static void OnPlanCompatiblePackage(object sender, PlanCompatiblePackageEventArgs e)
		{
			Logger.Instance.Trace("" + e.PackageId + " " + e.State + " " + e.Result);
		}

		/// Fired when the engine is about to plan the target MSI of a MSP package.
		static void OnPlanTargetMsiPackage(object sender, PlanTargetMsiPackageEventArgs e)
		{
			Logger.Instance.Trace("" + e.PackageId + " " + e.ProductCode + " " + e.State + " " + e.Result);
		}

		/// Fired when the engine is about to plan a feature in an MSI package.
		static void OnPlanMsiFeature(object sender, PlanMsiFeatureEventArgs e)
		{
			Logger.Instance.Trace("" + e.PackageId + " " + e.FeatureId + " " + e.State + " " + e.Result);
		}

		/// Fired when the engine has completed planning the installation of a specific package.
		static void OnPlanPackageComplete(object sender, PlanPackageCompleteEventArgs e)
		{
			Logger.Instance.Trace("" + e.PackageId + " " + e.State + " " + e.Execute);
		}

		/// Fired when the engine has completed planning the installation.
		static void OnPlanComplete(object sender, PlanCompleteEventArgs e)
		{
			Logger.Instance.Trace("" + e.Status.ToString());
		}
	}
}