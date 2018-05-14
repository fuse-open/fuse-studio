using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace Outracks.Fusion.Windows
{
	public class WinFormMenuBuilder
	{
		public static IDisposable Populate(Menu menu, System.Windows.Forms.ContextMenu mainMenu)
		{
			return menu.Items
				.ObserveOn(DispatcherScheduler.Current)
				.UnsafeAsObservableList()
				.Select(CreateItem)
				.DisposeElements(i => i.Tag as IDisposable ?? Disposable.Empty)
				.Subscribe(change => change.ApplyLegacy(mainMenu.MenuItems));
		}

		public static System.Windows.Forms.MenuItem CreateItem(MenuItem item)
		{
			var menuItem = new System.Windows.Forms.MenuItem();

			Action click = () => { };
			menuItem.Click += (s, a) => click();

			menuItem.Tag = Disposable.Combine(
				item.Menu.Select(
					submenu => submenu.Items
						.ObserveOn(DispatcherScheduler.Current)
						.UnsafeAsObservableList()
						.Select(CreateItem)
						.DisposeElements(i => i.Tag as IDisposable ?? Disposable.Empty)
						.Subscribe(change => change.ApplyLegacy(menuItem.MenuItems)))
					.Or(Disposable.Empty),

				item.Command.Action
					.CombineLatest(item.Name)
					.ObserveOn(DispatcherScheduler.Current)
					.Subscribe(
						cmdName =>
						{
							menuItem.Text = cmdName.Item2;
							click = () => cmdName.Item1.Do(x => x());
						}),

				item.IsToggled
					.ObserveOn(DispatcherScheduler.Current)
					.Subscribe(
						isToggled =>
							menuItem.Checked = isToggled));

			return menuItem;
		}
	}
}