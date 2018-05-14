using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.ViewModel;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;

namespace Fuse.Installer.Gui
{
	public class MainWindowModel : NotificationObject
	{
		public enum InstallState
		{
			Initializing,
			Present,
			NotPresent,
			Applying,
			Cancelled
		}

		InstallState state;
		string message;
		readonly BootstrapperApplicationModel model;
		public ICommand InstallCommand { get; private set; }
		public ICommand UninstallCommand { get; private set; }
		public ICommand CancelCommand { get; private set; }

		FrameworkElement _innerContent;
		public FrameworkElement InnerContent
		{
			get { return _innerContent; }
			set
			{
				_innerContent = value;
				RaisePropertyChanged(() => InnerContent);
			}
		}

		public string Message
		{
			get { return message; }
			set
			{
				if (message != value)
				{
					message = value;
					RaisePropertyChanged(() => Message);
				}
			}
		}

		public InstallState State
		{
			get { return state; }
			set
			{
				if (state != value)
				{
					state = value;
					Message = state.ToString();
					RaisePropertyChanged(() => State);
					Refresh();
				}
			}
		}

		readonly ProgressViewModel _progressModel;
		public MainWindowModel(BootstrapperApplicationModel model)
		{
			this.model = model;
			State = InstallState.Initializing;
			WireUpEventHandlers();

			CancelCommand = new DelegateCommand(() =>
			{
				this.model.LogMessage("Cancelling...");
				FuseBootstrapperApplication.Dispatcher.InvokeShutdown();
			}, () => State != InstallState.Cancelled);

			_progressModel = new ProgressViewModel(CancelCommand);
			InstallCommand = new DelegateCommand(() =>
			{
				_progressModel.Title = "Installing Fuse";
				InnerContent = new ProgressView(_progressModel);
				this.model.PlanAction(LaunchAction.Install);
			}, () => true);
			UninstallCommand = new DelegateCommand(() =>
			{
				_progressModel.Title = "Uninstalling Fuse";
				InnerContent = new ProgressView(_progressModel);
				this.model.PlanAction(LaunchAction.Uninstall);
			}, () => true);
			
			InnerContent = model.BootstrapperApplication.Command.Action == LaunchAction.Uninstall 
				? (FrameworkElement)new UninstallView(new UninstallViewModel(UninstallCommand))
				: (FrameworkElement)new InstallerView(new InstallerViewModel(InstallCommand, CancelCommand));
		}

		protected void DetectPackageComplete(object sender, DetectPackageCompleteEventArgs e)
		{
			Console.WriteLine(e.PackageId + " " + e.State.ToString());
			State = InstallState.NotPresent;
			if (e.PackageId.Equals("MyInstaller.msi", StringComparison.Ordinal))
				if (e.State == PackageState.Present) State = InstallState.Present;
				
		}

		protected void PlanComplete(object sender, PlanCompleteEventArgs e)
		{
			if (State == InstallState.Cancelled)
			{
				FuseBootstrapperApplication.Dispatcher.InvokeShutdown();
				return;
			}
			model.ApplyAction();
		}

		protected void ApplyBegin(object sender, ApplyBeginEventArgs e)
		{
			State = InstallState.Applying;
		}

		protected void ExecutePackageBegin(object sender, ExecutePackageBeginEventArgs e)
		{
			if (State == InstallState.Cancelled)
				e.Result = Result.Cancel;
		}

		protected void ExecutePackageComplete(object sender, ExecutePackageCompleteEventArgs e)
		{
			if (State == InstallState.Cancelled)
				e.Result = Result.Cancel;
		}

		protected void ApplyComplete(object sender, ApplyCompleteEventArgs e)
		{
			model.FinalResult = e.Status;
			if(e.Status == 0)
				FuseBootstrapperApplication.Dispatcher.InvokeShutdown();
		}

		void Refresh()
		{
			FuseBootstrapperApplication.Dispatcher.Invoke((Action) (() =>
			                                              {
				                                              ((DelegateCommand) InstallCommand).RaiseCanExecuteChanged();
				                                              ((DelegateCommand) UninstallCommand).RaiseCanExecuteChanged();
				                                              ((DelegateCommand) CancelCommand).RaiseCanExecuteChanged();
			                                              }));
		}

		void WireUpEventHandlers() 
		{
			model.BootstrapperApplication.DetectPackageComplete += DetectPackageComplete;
			model.BootstrapperApplication.PlanComplete += PlanComplete;
			model.BootstrapperApplication.ApplyComplete += ApplyComplete;
			model.BootstrapperApplication.ApplyBegin += ApplyBegin;
			model.BootstrapperApplication.ExecutePackageBegin += ExecutePackageBegin;
			model.BootstrapperApplication.ExecutePackageComplete += ExecutePackageComplete;
			model.BootstrapperApplication.ExecuteProgress += (sender, args) => _progressModel.Progress = args.OverallPercentage;
			model.BootstrapperApplication.Error += (sender, args) => FuseBootstrapperApplication.Dispatcher.InvokeAsync(() => InnerContent = new ErrorView(new ErrorViewModel(CancelCommand, args.ErrorMessage)));
		}
	}
}