using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows.Controls;

namespace Outracks.Fusion.Windows
{
	public class WindowsMenuBuilder
	{
		public static IDisposable PopulateContextMenu(Menu menu, System.Windows.Controls.ContextMenu contextMenu, WindowWithOverlays inputBindings, IScheduler dispatcher)
		{
			return CreateSubMenu(menu, contextMenu.Items, inputBindings, dispatcher, false);
		}

		public static IDisposable Populate(Menu menu, System.Windows.Controls.Menu mainMenu, WindowWithOverlays inputBindings, IScheduler dispatcher)
		{
			return CreateSubMenu(menu, mainMenu.Items, inputBindings, dispatcher, false);
		}

		public static System.Windows.Controls.Control CreateItem(MenuItem item, WindowWithOverlays inputBindings, IScheduler dispatcher, bool applyStyle)
		{
			if (item.IsSeparator)
				return new Separator();

			var menuItem = new System.Windows.Controls.MenuItem();
			if (applyStyle)
				menuItem.Foreground = System.Windows.Media.Brushes.Black;

			Action click = () => { };
			menuItem.Click += (s, a) => click();
			menuItem.Tag =
				Disposable.Combine(
					item.Menu.Select(submenu => CreateSubMenu(submenu, menuItem.Items, inputBindings, dispatcher, true)).Or(Disposable.Empty),
					item.Command.Action.CombineLatest(item.Name).ObserveOn(dispatcher).SubscribeUsing(cmdName =>
					{
						var cmd = cmdName.Item1;
						var name = cmdName.Item2;
						var command = new WpfCommand(cmd);
						menuItem.Header = name;
						menuItem.Command = command;
						return item.Hotkey != HotKey.None && inputBindings != null
							? inputBindings.AddInputBinding(item.Hotkey, menuItem, command)
							: Disposable.Empty;
					}),
					item.IsToggled.Subscribe(isToggled =>
					{
						dispatcher.Schedule(() => { menuItem.IsChecked = isToggled; });
					}));
			return menuItem;
		}

		static IDisposable CreateSubMenu(Menu menu, ItemCollection nativeMenu, WindowWithOverlays inputBindings, IScheduler dispatcher, bool applyStyle)
		{
			return menu.Items
				.ObserveOn(Fusion.Application.MainThread)
				.UnsafeAsObservableList()
				.Select(item => CreateItem(item, inputBindings, dispatcher, applyStyle))
				.DisposeElements(item => item.Tag as IDisposable ?? Disposable.Empty)
				.Subscribe(change => change.ApplyLegacy(nativeMenu));
		}
	}
}