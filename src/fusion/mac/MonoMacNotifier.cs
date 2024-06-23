using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Foundation;

namespace Outracks.Fusion.Mac
{
	class MonoMacNotifier : INotifier
	{
		readonly Subject<IImmutableSet<Guid>> _currentNotifications = new Subject<IImmutableSet<Guid>>();
		readonly Subject<Guid> _notificationActivated = new Subject<Guid>();
		readonly NSTimer _timer;
		readonly NSUserNotificationCenter _center;
		readonly IReport _report;

		public MonoMacNotifier(NSUserNotificationCenter center, IReport report)
		{
			_center = center;
			_report = report;
			_center.ShouldPresentNotification = (c, n) => true; // Show notification even if app is TopMost.
			_center.DidActivateNotification += (s, a) =>
				_notificationActivated.OnNext(GetId(a.Notification));

			_timer = NSTimer.CreateRepeatingScheduledTimer(
				TimeSpan.FromMilliseconds(500),
				timer =>
				{
					try
					{
						var currentNotifications =
							_center.DeliveredNotifications.Select(GetId).ToImmutableHashSet();

						_currentNotifications.OnNext(currentNotifications);
					}
					catch (Exception e)
					{
						_report.Exception("Updating current notifications failed", e);
					}
				});
		}

		public void Dispose()
		{
			try
			{
				_timer.Invalidate();
				_timer.Dispose();
			}
			catch (Exception e)
			{
				_report.Exception("MonoMacNotifier.Dispose()", e);
			}
		}

		public async Task<NotificationFeedback> Show(Notification notification)
		{
			var guid = Guid.NewGuid();
			var not = new NSUserNotification
			{
				Title = notification.Title,
				InformativeText = notification.Description,
				DeliveryDate = NSDate.Now,
				SoundName = NSUserNotification.NSUserNotificationDefaultSoundName,
				ActionButtonTitle = notification.PrimaryAction.Or(""),
				HasActionButton = notification.PrimaryAction.HasValue,
				UserInfo = NSDictionary.FromObjectAndKey(new NSString(guid.ToString()), Key),
			};

			// Approach described by http://stackoverflow.com/questions/21110714/mac-os-x-nsusernotificationcenter-notification-get-dismiss-event-callback/21365269#21365269
			var dismissed = _currentNotifications
				.SkipWhile(s => !s.Contains(guid))
				.SkipWhile(s => s.Contains(guid))
				.Take(1);

			var activated = _notificationActivated
				.Where(n => n.Equals(guid));

			var status = Observable.Merge(
				dismissed.Select(_ => NotificationFeedback.Dismissed),
				activated.Select(_ => NotificationFeedback.PrimaryActionTaken));

			var task = status.FirstAsync().ToTask();

			await MainThread.BeginInvoke(() => _center.DeliverNotification(not));

			return await task;
		}

		static Guid GetId(NSUserNotification notification)
		{
			var userInfo = notification.UserInfo;
			if (userInfo == null)
				return Guid.NewGuid();

			NSObject value;
			if (!userInfo.TryGetValue(Key, out value))
				return Guid.NewGuid();

			return Guid.Parse((NSString)value);
		}

		static readonly NSString Key = new NSString("FuseNotificationId");
	}
}