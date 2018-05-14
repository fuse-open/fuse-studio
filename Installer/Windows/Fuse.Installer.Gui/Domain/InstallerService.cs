using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;

namespace Fuse.Installer.Gui.Domain
{
	public class InstallerService
	{

		public InstallerService(FuseBootstrapperApplication a)
		{
			a.Startup += OnStartup;
			a.Shutdown += OnShutdown;
			
			a.SystemShutdown += OnSystemShutdown;

			Detect.Register(a);

			Plan.Register(a);

			Apply.Register(a);

			a.ExecuteProgress += OnExecuteProgress;

			a.LaunchApprovedExeBegin += LaunchApprovedExeBegin;

			a.LaunchApprovedExeComplete += LaunchApprovedExeComplete;

		}

		/// Fired when the engine has completed launching the preapproved executable.
		void LaunchApprovedExeComplete(object sender, LaunchApprovedExeCompleteArgs e)
		{
			Logger.Instance.Trace("ProcessId: " + e.ProcessId + " Status " + e.Status);
		}

		/// Fired when the engine is about to launch the preapproved executable.
		void LaunchApprovedExeBegin(object sender, LaunchApprovedExeBeginArgs e)
		{
			Logger.Instance.Trace("");
		}

		/// Fired by the engine while executing on payload.
		void OnExecuteProgress(object sender, ExecuteProgressEventArgs e)
		{
			Logger.Instance.Trace("PackageId: " + e.PackageId + " " + e.ProgressPercentage + "/" + e.OverallPercentage);
		}

		/// Fired when the system is shutting down or user is logging off.
		/// 
		/// <remarks>
		/// 
		/// <para>
		/// To prevent shutting down or logging off, set <see cref="P:Microsoft.Tools.WindowsInstallerXml.Bootstrapper.ResultEventArgs.Result"/> to
		///             <see cref="F:Microsoft.Tools.WindowsInstallerXml.Bootstrapper.Result.Cancel"/>; otherwise, set it to <see cref="F:Microsoft.Tools.WindowsInstallerXml.Bootstrapper.Result.Ok"/>.
		/// </para>
		/// 
		/// <para>
		/// By default setup will prevent shutting down or logging off between
		///             <see cref="E:Microsoft.Tools.WindowsInstallerXml.Bootstrapper.BootstrapperApplication.ApplyBegin"/> and <see cref="E:Microsoft.Tools.WindowsInstallerXml.Bootstrapper.BootstrapperApplication.ApplyComplete"/>.
		///             Derivatives can change this behavior by overriding <see cref="M:Microsoft.Tools.WindowsInstallerXml.Bootstrapper.BootstrapperApplication.OnSystemShutdown(Microsoft.Tools.WindowsInstallerXml.Bootstrapper.SystemShutdownEventArgs)"/>
		///             or handling <see cref="E:Microsoft.Tools.WindowsInstallerXml.Bootstrapper.BootstrapperApplication.SystemShutdown"/>.
		/// </para>
		/// 
		/// <para>
		/// If <see cref="P:Microsoft.Tools.WindowsInstallerXml.Bootstrapper.SystemShutdownEventArgs.Reasons"/> contains <see cref="F:Microsoft.Tools.WindowsInstallerXml.Bootstrapper.EndSessionReasons.Critical"/>
		///             the bootstrapper cannot prevent the shutdown and only has a few seconds to save state or perform any other
		///             critical operations before being closed by the operating system.
		/// </para>
		/// 
		/// <para>
		/// This event may be fired on a different thread.
		/// </para>
		/// 
		/// </remarks>
		void OnSystemShutdown(object sender, SystemShutdownEventArgs e)
		{
			Logger.Instance.Trace(e.Reasons.ToString());
		}

		/// Fired when the engine is shutting down the bootstrapper application.
		void OnShutdown(object sender, ShutdownEventArgs e)
		{
			Logger.Instance.Trace(e.Result.ToString());
		}

		/// Fired when the engine is starting up the bootstrapper application.
		void OnStartup(object sender, StartupEventArgs e)
		{
			Logger.Instance.Trace("");
		}
	}
}