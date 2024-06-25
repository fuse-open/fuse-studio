using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Outracks.Fuse.Setup
{
	public abstract class SoftwareStatus
	{
		readonly ISubject<InstallStatus> _status = new Subject<InstallStatus>();
		readonly IReport _report;

		public string Name { get; private set; }

		public IObservable<InstallStatus> Status { get; private set; }

		protected SoftwareStatus(string name, IReport report)
		{
			Name = name;
			_report = report;
			Status = Observable
				.Defer(() => Observable
					.Start(() => CheckStatusSafe())
					.Concat(Observable.Empty<InstallStatus>().Delay(TimeSpan.FromSeconds(2))))
				.Repeat()
				.Merge(_status)
				.DistinctUntilChanged()
				.Replay(1).RefCount();
		}

		InstallStatus CheckStatusSafe()
		{
			try
			{
				return CheckStatus();
			}
			catch (Exception e)
			{
				_report.Exception("Failed to check software status", e, ReportTo.LogAndUser | ReportTo.Headquarters);
				return InstallStatus.Unknown;
			}
		}

		protected abstract InstallStatus CheckStatus();

		public void Install()
		{
			try
			{
				TryInstall();
			}
			catch (Exception e)
			{
				_report.Exception("Failed to install software", e, ReportTo.LogAndUser | ReportTo.Headquarters);
			}
		}

		protected abstract void TryInstall();

		protected void SetStatus(InstallStatus status)
		{
			_status.OnNext(status);
		}
	}
}