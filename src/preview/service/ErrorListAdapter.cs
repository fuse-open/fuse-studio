using System;
using Uno;
using Uno.Logging;

namespace Fuse.Preview
{
	class ErrorListAdapter : IErrorList
	{
		readonly Guid _buildId;
		readonly IProgress<BuildIssueDetected> _events;

		public ErrorListAdapter(Guid buildId, IProgress<BuildIssueDetected> events)
		{
			_buildId = buildId;
			_events = events;
		}

		public void AddFatalError(Source src, string code, string msg)
		{
			Report(new BuildIssueDetected(BuildIssueType.FatalError, code, msg, src.ToSourceReference(), _buildId));
		}

		public void AddError(Source src, string code, string msg)
		{
			Report(new BuildIssueDetected(BuildIssueType.Error, code, msg, src.ToSourceReference(), _buildId));
		}

		public void AddWarning(Source src, string code, string msg)
		{
			Report(new BuildIssueDetected(BuildIssueType.Warning, code, msg, src.ToSourceReference(), _buildId));
		}

		public void AddMessage(Source src, string code, string msg)
		{
			Report(new BuildIssueDetected(BuildIssueType.Message, code, msg, src.ToSourceReference(), _buildId));
		}

		void Report(BuildIssueDetected ev)
		{
			_events.Report(ev);
		}
	}
}