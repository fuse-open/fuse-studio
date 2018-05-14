using System;
using Outracks;
using Outracks.Simulator;
using Uno.UX.Markup.Common;

namespace Fuse.Preview
{
	
	public class MarkupErrorLog : IMarkupErrorLog
	{
		readonly Guid _buildId;
		readonly IObserver<IBinaryMessage> _observer;

		public MarkupErrorLog(IObserver<IBinaryMessage> observer, Guid buildId)
		{
			_buildId = buildId;
			_observer = observer;
		}

		public void ReportError(string message)
		{
			_observer.OnNext(
				new BuildIssueDetected(
					BuildIssueType.Error, "", message,
					Optional.None<SourceReference>(),
					_buildId));
		}

		public void ReportWarning(string message)
		{
			_observer.OnNext(
				new BuildIssueDetected(
					BuildIssueType.Warning, "", message,
					Optional.None<SourceReference>(),
					_buildId));
		}

		public void ReportError(string path, int line, string message)
		{
			_observer.OnNext(
				new BuildIssueDetected(
					BuildIssueType.Error, "", message,
					new SourceReference((path), new TextPosition(new LineNumber(line), new CharacterNumber(1))),
					_buildId));
		}

		public void ReportWarning(string path, int line, string message)
		{
			_observer.OnNext(
				new BuildIssueDetected(
					BuildIssueType.Warning, "", message,
					new SourceReference((path), new TextPosition(new LineNumber(line), new CharacterNumber(1))),
					_buildId));
		}
	}
}