using System;
using System.Windows;
using System.Windows.Interop;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;

namespace Fuse.Installer.Gui
{
	public class BootstrapperApplicationModel
	{
		IntPtr _hwnd;

		public BootstrapperApplicationModel(FuseBootstrapperApplication bootstrapperApplication)
		{
			BootstrapperApplication = bootstrapperApplication;

			_hwnd = IntPtr.Zero;
		}

		public FuseBootstrapperApplication BootstrapperApplication { get; private set; }
		public int FinalResult { get; set; }

		public void SetWindowHandle(Window view)
		{
			_hwnd = new WindowInteropHelper(view).Handle;
		}

		public void PlanAction(LaunchAction action)
		{
			BootstrapperApplication.Engine.Plan(action);
		}

		public void ApplyAction()
		{
			BootstrapperApplication.Engine.Apply(_hwnd);
		}

		public void LogMessage(string message)
		{
			if (BootstrapperApplication.Engine != null) BootstrapperApplication.Engine.Log(LogLevel.Verbose, message);
		}
	}

	public class TestCasd : BootstrapperApplication
	{
		/// <summary>
		/// Entry point that is called when the bootstrapper application is ready to run.
		/// </summary>
		protected override void Run() {}
	}
}