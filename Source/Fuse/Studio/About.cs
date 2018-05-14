using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Outracks.Fusion;

namespace Outracks.Fuse.Designer
{
	public class About {

		public About(Version version, Debug debug)
		{
			var showAboutDialog = new BehaviorSubject<bool>(false);

			var thanksTo = "Created by:\n\n"
				+ "João Abecasis, Karina Asaevich, Christopher Bagley, Alexander Bladyko, Anders Bondehagen, Aaron Cohen, Liam Crouch, "
				+ "Håkon Martin Eide, Guro Faller, Erik Faye-Lund, Morten Daniel Fornes, Olle Fredriksson, Anette Gjetnes, Lorents Odin Gravås, "
				+ "Kristian Hasselknippe, Daniel Hollick, Erik Hvattum, Anders Schau Knatten, Anders Knive Lassen, Vegard Lende, Vika Lepeshinskaya, "
				+ "Sumi Lim, edA-qa mort-ora-y, Trond Arve Nordheim, Remi Johan Pedersen, Evgeny Razumov, Jonny Ree, Sebastian Reinhard, "
				+ "Jan Stefan Rimaila, Andreas Rønning, Emil Sandstø, Bent Stamnes, Karsten Nikolai Strand, Jake Taylor."
				+ "\n\n Fuse © 2017";

			Application.Desktop.CreateSingletonWindow(
				isVisible: showAboutDialog,
				window: dialog =>
					new Window
					{
						Title = Observable.Return("About Fuse"),
						Content = Control.Lazy(() =>
							Layout.Dock()
								.Top(LogoAndVersion.Create(version).WithMacWindowStyleCompensation())
								.Bottom(CreateOkButton(Command.Enabled(() => dialog.Close())))
								.Bottom(Separator.Medium)
								.Fill(
									Label.Create(font: Theme.DefaultFont, color:Theme.DefaultText, lineBreakMode: LineBreakMode.Wrap, text: thanksTo)
										.OnMouse(debug.EnableDebugMenuByRapidClicking)
										.WithPadding(Thickness.Create(new Points(20), 10, 20, 10))
								).WithBackground(Theme.PanelBackground)),
						Size = Optional.Some(Property.Constant(Optional.Some(new Size<Points>(370, 520)))),
						Background = Theme.PanelBackground,
						Foreground = Theme.DefaultText,
						Border = Separator.MediumStroke,
						Style = WindowStyle.Fat
					}
			);

			Menu = Menu.Item("About Fuse", () => showAboutDialog.OnNext(true));
		}

		static IControl CreateOkButton(Command clicked)
		{
			return Button.Create(
				clicked: clicked,
				content: bs => Layout.Dock()
					.Left(Fuse.Icons.Confirm(Theme.Active).CenterVertically())
					.Left(Spacer.Small)
					.Fill(Label.Create(
						text: "Ok",
						color: Theme.DefaultText))
					.Center())
				.WithHeight(45);
		}

		public Menu Menu { get; private set; }
	}
}
