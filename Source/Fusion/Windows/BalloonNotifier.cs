using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Outracks.Fusion.Windows
{
	class ShowingNotification
	{
		public readonly TaskCompletionSource<NotificationFeedback> TaskCompletionSource = new TaskCompletionSource<NotificationFeedback>();
		public readonly Notification Notification;

		//public Optional<DateTime> ShownAt;

		public ShowingNotification(Notification notification)
		{
			Notification = notification;
		}
	}
	class BalloonNotifier : INotifier, IDisposable
	{
		readonly NotifyIcon _icon;
		readonly IDisposable[] _subscriptions;

		readonly BehaviorSubject<IImmutableList<ShowingNotification>> _notifications = new BehaviorSubject<IImmutableList<ShowingNotification>>(ImmutableList<ShowingNotification>.Empty);
 
		public BalloonNotifier(NotifyIcon icon, System.Windows.Threading.Dispatcher scheduler)
		{
			_icon = icon;

			var clicked = DataBinding.ObservableFromNativeEvent<EventArgs>(_icon, "BalloonTipClicked");
			var closed = DataBinding.ObservableFromNativeEvent<EventArgs>(_icon, "BalloonTipClosed");

			var current = _notifications.Select(n => n.LastOrNone()).NotNone();

			_subscriptions = new[]
			{
				current
					.ObserveOn(scheduler)
					.Subscribe(n =>
					{
						var timeout = n.Notification.Timeout.Or(TimeSpan.FromHours(3));
						_icon.ShowBalloonTip(
							(int)timeout.TotalMilliseconds,
							n.Notification.Title,
							n.Notification.Description,
							n.Notification.Type == NotifyType.Error ? ToolTipIcon.Error : 
							n.Notification.Type == NotifyType.Info ? ToolTipIcon.Info :
							ToolTipIcon.None);
					}),

				clicked.WithLatestFromBuffered(current, (_, c) => c)
					.ObserveOn(TaskPoolScheduler.Default)
					.Subscribe(c =>
					{
						_notifications.OnNext(_notifications.Value.Remove(c));
						c.TaskCompletionSource.TrySetResult(NotificationFeedback.PrimaryActionTaken);
					}),

				closed.WithLatestFromBuffered(current, (_, c) => c)
					.ObserveOn(TaskPoolScheduler.Default)
					.Subscribe(c =>
					{
						_notifications.OnNext(_notifications.Value.Remove(c));
						c.TaskCompletionSource.TrySetResult(NotificationFeedback.Dismissed);
					}),
			};

		}

		public Task<NotificationFeedback> Show(Notification notification)
		{
			var sn = new ShowingNotification(notification);
			_notifications.OnNext(_notifications.Value.Add(sn));
			return sn.TaskCompletionSource.Task;
		}

		public void Dispose()
		{
			foreach (var sub in _subscriptions.Reverse())
				sub.Dispose();
		}
	}
}