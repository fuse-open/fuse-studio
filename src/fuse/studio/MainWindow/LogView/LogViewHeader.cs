using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Outracks.Fusion;

namespace Outracks.Fuse.Studio
{
	static class LogViewHeader
	{
		public static Points HeaderHeight = 24;

		public static IControl CreateHeader(IEnumerable<LogViewTab> tabs, ISubject<LogViewTab> activeTab, IProperty<bool> showLog )
		{
			return  Shapes.Rectangle(fill: Theme.PanelBackground)
						.WithOverlay(
							Layout.Dock()
							.Left(Layout.StackFromLeft(tabs.Select(tab =>
								CreateHeaderTitle(
									tab.Title,
									activeTab.Select(t => t == tab),
									activeTab.Select(t => t == tab)
										.CombineLatest(showLog, (isActive, show) => isActive && show)
										.Select(isRead => isRead ? Observable.Return(false) : tab.NotifyUser)
										.Switch()
										.DistinctUntilChanged(),
									() =>
									{
										activeTab.OnNext(tab);
										showLog.Write(true, save: false);
									}))))
							.Right(
								Button.Create(
									showLog.Toggle(),
									states =>
									{
										return Layout.StackFromTop(
												Shapes.Rectangle(fill: Theme.IconBrush)
													.WithSize(new Size<Points>(20, 1))
													.WithPadding(new Thickness<Points>(0, 2)),
												Shapes.Rectangle(fill: Theme.IconBrush)
													.WithSize(new Size<Points>(20, 1))
													.WithPadding(new Thickness<Points>(0, 2))
											)
											.WithPadding(new Thickness<Points>(0, 0, 15, 0))
											.Center();
									}))
							.Fill()
							)
						.WithHeight(HeaderHeight)
						.DockBottom()

			;
		}

		static IControl CreateHeaderTitle(Text title, IObservable<bool> isActive, IObservable<bool> unread, Action pressed)
		{
			isActive = isActive.StartWith(false);
			return
				Button.Create(Command.Enabled(pressed),
					content: states =>
						Layout.StackFromLeft(
							Label.Create(
								title,
								font: Font.SystemDefault(Observable.Return(13.0), isActive),
								color: isActive.Select(active => active ? Theme.DefaultText : Theme.DisabledText).Switch()),
							unread.StartWith(false).Select(b => b == false
								? Control.Empty
								: Shapes.Circle(fill: Theme.NotificationBrush)
									.WithSize(new Size<Points>(10,10))
									.WithPadding(new Thickness<Points>(10,0,0,0))
									.Center())
								.Switch()
						)
						.WithPadding(new Thickness<Points>(30, 0))
						.CenterVertically());
		}
	}
}
