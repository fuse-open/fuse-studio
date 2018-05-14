using System;
using System.Drawing;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using AppKit;
using CoreGraphics;
using Foundation;

namespace Outracks.Fusion.OSX
{
	public class MonoMacTrayApplication : ITrayApplication
	{
		readonly NSStatusItem _item;
		readonly MonoMacNotifier _notifier;

		public MonoMacTrayApplication(IReport errorHandler, IObservable<Icon> icon, IObservable<string> title, Menu menu)
		{
			// Display tray icon in upper-right-hand corner of the screen
			_item = NSStatusBar.SystemStatusBar.CreateStatusItem(30);
			_item.HighlightMode = true;

			// Remove the system tray icon from upper-right hand corner of the screen
			// (works without adjusting the LSUIElement setting in Info.plist)
			NSApplication.SharedApplication.ActivationPolicy = NSApplicationActivationPolicy.Accessory;

			_notifier = new MonoMacNotifier(NSUserNotificationCenter.DefaultUserNotificationCenter, errorHandler);

			UserClicked = DataBinding.ObservableFromNativeEvent<EventArgs>(_item, "DoubleClick").Select(_ => 1);

			if (menu != default(Menu))
				MenuBuilder.CreateMenu(menu, errorHandler).ToObservable().Subscribe(m => 
					Fusion.Application.MainThread.InvokeAsync(() => _item.Menu = m));

			if (icon != null) 
				icon.Subscribe(i => MainThread.BeginInvoke(() => SetIcon(i)));
			
			if (title != null) 
				title.Subscribe(t => MainThread.BeginInvoke(() => SetTitle(t)));

			((AppDelegate)NSApplication.SharedApplication.Delegate).Terminates.Subscribe(_ => Dispose());
		}
		
		void SetIcon(Icon icon)
		{
			using (var stream = icon.GetStream())
			{
				// Fun song and dance to avoid using the default 256/256 rep
				var icn = NSImage.FromStream(stream).BestRepresentation(new CGRect(0, 0, 32, 32), null, null);
				icn.Size = new CGSize(24, 24);
				var image = new NSImage(icn.Size);
				image.AddRepresentation(icn);
				image.Template = true;
				_item.Image = image;
			}
		}

		void SetTitle(string title)
		{
			_item.Title = title;
		}
		
		public IObservable<int> UserClicked { get; private set; }

		public Task<NotificationFeedback> Show(Notification notification)
		{
			return _notifier.Show(notification);
		}

		public void Dispose()
		{
			MainThread.BeginInvoke(
				() =>
				{
					_notifier.Dispose();
					NSStatusBar.SystemStatusBar.RemoveStatusItem(_item);
				});
		}
	}
}
