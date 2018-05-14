using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Outracks.Fuse.Components;
using Outracks.Fuse.Designer;

namespace Outracks.Fuse.Setup
{
	using Fusion;
	
	public static class MissingPluginNotification
	{
		public static void Create(IFuse fuse, IObservable<Unit> doneLoadingMainWindow, BehaviorSubject<bool> showSetupGuide)
		{
			var showSublimeNotification = new BehaviorSubject<bool>(false);
			var dontCheckForSublimePlugin = UserSettings.Bool("DontCheckForSublimePlugin").AutoInvalidate();

			Application.Desktop.CreateSingletonWindow(
				isVisible: showSublimeNotification.CombineLatest(doneLoadingMainWindow, (show,_) => show),
				window: dialog => new Window
				{
					Title = Observable.Return("No editor plugin detected"),
					Size = Optional.Some(Property.Constant(Optional.Some(new Size<Points>(600, 270)))),
					Content = Control.Lazy(() => CreateContent(showSublimeNotification, showSetupGuide, dontCheckForSublimePlugin))
						.WithBackground(Theme.PanelBackground),
					Background = Theme.PanelBackground,
					Foreground = Theme.DefaultText,
					Border = Separator.MediumStroke,
				});

			dontCheckForSublimePlugin
				.Select(v => v.Or(false))
				.Where(v => !v)
				.Take(1)
				.SubscribeOn(TaskPoolScheduler.Default)
				.Subscribe(_ => Task.Run(() =>
				{
					if (new SublimePlugin(fuse.ModulesDir).Status == ComponentStatus.NotInstalled)
						showSublimeNotification.OnNext(true);
				}));
		}

		public static IControl CreateContent(BehaviorSubject<bool> showSublimeNotification, BehaviorSubject<bool> showSetupGuide, IProperty<Optional<bool>> dontCheckForSublimePlugin)
		{
			return Layout.Dock()

				.Top(Label.Create(
					text: "You don't seem to have an editor plugin set up.", 
					color: Theme.DefaultText))

				.Top(Spacer.Medium)

				.Top(Label.Create(
					text: "Do you want help setting it up now?",
					color: Theme.DefaultText))

				.Bottom(Layout.Dock()
					.Left(Buttons.DefaultButton(
							text: "Don't warn again",
							cmd: Command.Enabled(() =>
							{
								dontCheckForSublimePlugin.Write(true);
								showSublimeNotification.OnNext(false);
							}))
							.WithWidth(150))

					.Right(Buttons.DefaultButtonPrimary(
							text: "Open Setup Guide",
							cmd: Command.Enabled(() =>
							{
								showSublimeNotification.OnNext(false);
								showSetupGuide.OnNext(true);
							}))
							.WithWidth(150))

					.Right(Spacer.Medium)

					.Right(Buttons.DefaultButton(
							text: "Remind Me Later",
							cmd: Command.Enabled(() =>
							{
								showSublimeNotification.OnNext(false);
							}))
							.WithWidth(150))

					.Fill())
				
				.Fill()

				.WithPadding(new Thickness<Points>(new Points(20)));
		}
	}
}
