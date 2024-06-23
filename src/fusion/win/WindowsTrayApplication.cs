using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Outracks.Fusion.Windows
{
	public class WindowsTrayApplication : ITrayApplication
	{
		readonly BalloonNotifier _notifier;
		readonly NotifyIcon _notifyIcon;
		readonly System.Windows.Threading.Dispatcher _scheduler;

		public WindowsTrayApplication(IObservable<string> title, Menu menu, IObservable<Icon> icon)
		{
			System.Windows.Forms.Application.EnableVisualStyles();
			System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

			var container = new Container();
			_notifyIcon = new NotifyIcon(container)
			{
				ContextMenu = new System.Windows.Forms.ContextMenu()
			};

			_scheduler = System.Windows.Threading.Dispatcher.CurrentDispatcher;

			_notifier = new BalloonNotifier(_notifyIcon, _scheduler);

			UserClicked =
				Observable.Merge(
					DataBinding.ObservableFromNativeEvent<MouseEventArgs>(_notifyIcon, "MouseDoubleClick"),
					DataBinding.ObservableFromNativeEvent<MouseEventArgs>(_notifyIcon, "MouseClick"))
				.Where(a => a.Button != MouseButtons.Right)
				.Select(a => a.Clicks);

			_notifyIcon.Visible = true;

			if (menu != default(Menu))
			{
				_notifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu();
				WinFormMenuBuilder.Populate(menu, _notifyIcon.ContextMenu);
			}

			if (icon != null)
				icon.ObserveOn(_scheduler).Subscribe(i =>
				{
					using (var stream = i.GetStream())
						_notifyIcon.Icon = new System.Drawing.Icon(stream);
				});

			if (title != null)
				title.ObserveOn(_scheduler).Subscribe(t => _notifyIcon.Text = t);
		}

		public IObservable<int> UserClicked { get; private set; }

		public async Task<NotificationFeedback> Show(Notification notification)
		{
			if (_notifier == null)
				throw new InvalidOperationException("Application have not been started yet");

			return await _notifier.Show(notification);
		}

		public void Dispose()
		{
			_scheduler.InvokeAsync(
				() =>
				{
					_notifier.Dispose();
					_notifyIcon.Dispose();
				});
		}
	}
}