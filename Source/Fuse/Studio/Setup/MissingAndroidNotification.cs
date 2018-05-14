using System;
using System.Diagnostics;
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

	static class MissingAndroidNotification
	{
		public static void Create(IFuse fuse, Subject<Unit> startedAndroidPreview)
		{
			var showNotification = new BehaviorSubject<bool>(false);
			var dontCheckForAndroid = UserSettings.Bool("DontCheckForAndroid").AutoInvalidate();

			Application.Desktop.CreateSingletonWindow(
				isVisible: showNotification,
				window: dialog => new Window
				{
					Title = Observable.Return("No Android tools detected"),
					Size = Optional.Some(Property.Constant(Optional.Some(new Size<Points>(600, 270)))),
					Content = Control.Lazy(() => CreateContent(showNotification, dontCheckForAndroid))
						.WithBackground(Theme.PanelBackground),
					Background = Theme.PanelBackground,
					Foreground = Theme.DefaultText,
					Border = Separator.MediumStroke,
				});

			startedAndroidPreview
				.Take(1)
				.WithLatestFromBuffered(dontCheckForAndroid.Select(v => v.Or(false)), (_, v) => v)
				.Where(v => !v)
				.SubscribeOn(TaskPoolScheduler.Default)
				.Subscribe(_ =>
					Task.Run(() =>
						{
							if (new AndroidInstaller().Status == ComponentStatus.NotInstalled)
								showNotification.OnNext(true);
						}));
		}

		public static IControl CreateContent(BehaviorSubject<bool> showSublimeNotification, IProperty<Optional<bool>> dontCheckForAndroid)
		{
			return Layout.Dock()

				.Top(Label.Create(
					text: "You don't seem to have Android tools set up.",
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
								dontCheckForAndroid.Write(true);
								showSublimeNotification.OnNext(false);
							}))
							.WithWidth(150))

					.Right(Buttons.DefaultButtonPrimary(
							text: "Get Android Tools",
							cmd: Command.Enabled(() =>
							{
								Process.Start("https://go.fusetools.com/previe_and_export");
								showSublimeNotification.OnNext(false);
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
