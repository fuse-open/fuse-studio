using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows;

namespace Outracks.Fusion.Windows
{
	public static class ContextMenuImplementation
	{
		public static void Initialize(IScheduler dispatcher)
		{
			ContextMenu.Implementation.Set = (self, menu) =>
			{
				self.IsRooted
					.ObserveOn(dispatcher)
					.Select(isRooted =>
					{
						var element = self.NativeHandle as FrameworkElement;
						if (element == null)
							return Disposable.Empty;

						return AddMenuTemporarily(element, menu.Select(x => x.ConnectWhile(self.IsRooted)), dispatcher);
					})
					.SubscribeUsing(disposable => disposable);

				return self;
			};
		}

		public static IDisposable AddMenuTemporarily(FrameworkElement element, Optional<Menu> menu, IScheduler dispatcher)
		{
			if (!menu.HasValue)
			{
				element.ContextMenu = null;
				return Disposable.Empty;
			}

			element.ContextMenu = new System.Windows.Controls.ContextMenu();

			var dispose = Disposable.Empty;

			element.ContextMenu.IsVisibleChanged += (sender, e) =>
			{
				if ((bool)e.NewValue)
				{
					dispose.Dispose();
					element.ContextMenu.Items.Clear();
					dispose = WindowsMenuBuilder.PopulateContextMenu(menu.Value, element.ContextMenu, null, dispatcher);
				}
				else
				{
					dispose.Dispose();
					element.ContextMenu.Items.Clear();
					dispose = Disposable.Empty;
				}
			};

			return Disposable.Create(() => dispose.Dispose());
		}

	}
}
