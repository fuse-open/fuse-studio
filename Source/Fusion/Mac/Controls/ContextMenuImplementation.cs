using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using AppKit;

namespace Outracks.Fusion.OSX
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
						var view = self.NativeHandle as NSView;
						if (view == null)
							return Disposable.Empty;

						return AddMenuTemporarily(view, menu.Select(x => x.ConnectWhile(self.IsRooted)), dispatcher);
					})
					.SubscribeUsing(disposable => disposable);

				return self;
			};
		}

		public static IDisposable AddMenuTemporarily(NSView view, Optional<Menu> menu, IScheduler dispatcher)
		{
			if (!menu.HasValue)
			{
				view.Menu = null;
				return Disposable.Empty;
			}

			view.Menu = new DataBoundNSMenu(menu.Value, ReportFactory.FallbackReport, populateLazily: true);
			return view.Menu;
		}

	}
}
