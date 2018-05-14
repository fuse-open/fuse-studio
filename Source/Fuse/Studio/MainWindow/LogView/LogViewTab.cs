using System;
using System.Reactive.Linq;
using Outracks.Fusion;

namespace Outracks.Fuse.Designer
{
	public class LogViewTab : IDisposable
	{
		public readonly string Title;
		public readonly IControl Content;
		public readonly IObservable<bool> NotifyUser;

		readonly IDisposable _changeSub;

		public LogViewTab(string title, IControl content, IObservable<bool> notifyUser)
		{
			Title = title;
			Content = content;
			var notify = notifyUser.Publish();
			NotifyUser = notify;
			_changeSub = notify.Connect();
		}

		public void Dispose()
		{
			_changeSub.Dispose();
		}
	}
}