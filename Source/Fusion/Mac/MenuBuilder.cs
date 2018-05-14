using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using AppKit;
using System;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

namespace Outracks.Fusion.OSX
{
	public class MenuBuilder
	{
		public static void Initialize(IScheduler dispatcher)
		{
		}

		public static async Task<DataBoundNSMenu> CreateMenu(Menu menu, IReport log, IObservable<string> title = null)
		{
			return await Fusion.Application.MainThread.InvokeAsync(() => new DataBoundNSMenu(menu, log, populateLazily: false, title: title));
		}
	}

	public class DataBoundNSMenu : NSMenu
	{
		readonly IScheduler _dispatcher;
		readonly IReport _log;
		IDisposable _subscription = Disposable.Empty;

		public DataBoundNSMenu(IntPtr handle)
			: base(handle)
		{
		}

		public DataBoundNSMenu(Menu menu, IReport log, bool populateLazily, IObservable<string> title = null)
		{
			title = title ?? Observable.Never<string>();
			_dispatcher = Fusion.Application.MainThread;
			_log = log;
			base.AutoEnablesItems = false;

			if (populateLazily)
			{
				Delegate = new DataBoundNSMenuDelegate(
					menuWillOpen: () =>
					{
						_subscription.Dispose();
						RemoveAllItems();
						_subscription = Populate(title, menu);
					},
					menuDidClose: () =>
					{
						_subscription.Dispose();
						RemoveAllItems();
						_subscription = Disposable.Empty;
					});
			}
			else
			{
				_subscription = Populate(title, menu);
			}
		}

		IDisposable Populate(IObservable<string> title, Menu menu)
		{
			return Disposable.Combine(
				title.Subscribe(t => Title = t),
				menu.Items
					.ObserveOn(_dispatcher)
					.UnsafeAsObservableList()
					.Select(CreateItem)
					.DisposeElements()
					.Subscribe(change =>
                        _log.TrySomethingBlocking(() =>
                        {
                            change(
                                insert: (i, x) => InsertItem(x, i),
                                replace: (i, x) =>
                                {
                                    RemoveItemAt(i);
                                    InsertItem(x, i);
                                },
                                remove: i => RemoveItemAt(i),
                                clear: RemoveAllItems);
                        })));
		}

		NSMenuItem CreateItem(MenuItem item)
		{
			if (item.IsSeparator)
				return (NSMenuItem)NSMenuItem.SeparatorItem.Copy();

			return new DataBoundNSMenuItem(_dispatcher, item, _log);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
				_subscription.Dispose();

			base.Dispose(disposing);
		}

		class DataBoundNSMenuDelegate : NSMenuDelegate
		{
			readonly Action _menuWillOpen;
			readonly Action _menuDidClose;

			public DataBoundNSMenuDelegate(IntPtr handle)
				: base(handle)
			{ }

			public DataBoundNSMenuDelegate(Action menuWillOpen, Action menuDidClose)
			{
				_menuWillOpen = menuWillOpen;
				_menuDidClose = menuDidClose;
			}

			public override void MenuWillOpen(NSMenu menu)
			{
				_menuWillOpen();
			}

			public override void MenuDidClose(NSMenu menu)
			{
				_menuDidClose();
			}

			public override void MenuWillHighlightItem(NSMenu menu, NSMenuItem item) { }
		}
	}

	class DataBoundNSMenuItem : NSMenuItem
	{
		readonly IDisposable _subscription;

		public DataBoundNSMenuItem(IntPtr handle) : base(handle)
		{			
		}

		public DataBoundNSMenuItem(IScheduler dispatcher, MenuItem item, IReport log)
		{
			item.Menu.Do(submenu => 
				MenuBuilder.CreateMenu(submenu, log, item.Name)
					.ToObservable().Subscribe(t => 
						Fusion.Application.MainThread.InvokeAsync(() => Submenu = t)));

			Action click = () => { };

			_subscription = Disposable.Combine
			(
				item.Command.Action.CombineLatest(item.Name).Subscribe(cmdName => 
					dispatcher.Schedule(() =>
					{
						var cmd = cmdName.Item1;
						var name = cmdName.Item2;
						Title = name;
						Enabled = cmd.HasValue;
						click = () => log.TrySomethingBlocking(() => cmd.Do(x => x()));
						if (item.Hotkey != HotKey.None)
						{
							KeyEquivalentModifierMask = item.Hotkey.Modifier.ToNSEventModifierMask();
							KeyEquivalent = item.Hotkey.Key.ToKeyEquivalent();
						}
					})),
		
				item.IsToggled.Subscribe(isToggled =>
					dispatcher.Schedule(() => 
						State = isToggled ? NSCellStateValue.On : NSCellStateValue.Off)),

				Disposable.Create(() => item.Menu.Do(_ => Submenu.Dispose()))
			);

			Activated += (s, a) => click();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
				_subscription.Dispose();

			base.Dispose(disposing);
		}
	}
}
