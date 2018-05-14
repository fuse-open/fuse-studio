using System;
using System.Collections.Immutable;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Outracks.Diagnostics;
using Outracks.Fusion;

namespace Outracks.Fuse
{
	public static class BuildFlagsWindow
	{
		public static Window Create(BehaviorSubject<bool> showWindow, BuildArgs args)
		{
			return new Window
			{
				Title = Observable.Return("Add build flags"),
				Size = Optional.Some(Property.Constant(Optional.Some(new Size<Points>(600, 280)))),
				Content = Control.Lazy(() => CreateContent(showWindow, args)),
				Background = Theme.PanelBackground,
				Foreground = Theme.DefaultText,
				Border = Separator.MediumStroke,
				Style = WindowStyle.Fat,
			};
		}

		static IControl CreateContent(BehaviorSubject<bool> showWindow, BuildArgs buildArgs)
		{
			var editString = Property.Create("");
			var editArgs = editString.Convert(BuildArgs.GetArgumentList, BuildArgs.GetArgumentString);

			Action save = async () => buildArgs.All.OnNext(await editArgs.FirstAsync());
			buildArgs.All.ConnectWhile(showWindow).Subscribe(args => editArgs.Write(args));

			return Layout.Dock()
				.Bottom(CreateButtons(showWindow, save: save))
				.Bottom(Separator.Medium)
				.Fill(
					Layout.StackFromTop(
							Spacer.Medium,
							Label.Create(
									text: "Separate flags with a space. Flags apply to all targets.",
									color: Theme.DefaultText)
								.CenterHorizontally(),
							Spacer.Medim,
							ThemedTextBox.Create(editString, doWrap: true),
							Spacer.Medium,
							Label.Create(
								text: "Presets",
								color: Theme.DefaultText),
							Spacer.Smaller,
							CreateFlagCheckBox(showWindow, editArgs, "-DCOCOAPODS", "Enable CocoaPods builds on iOS (-DCOCOAPODS)"),
							Spacer.Smaller,
							CreateFlagCheckBox(showWindow, editArgs, "-DGRADLE", "Enable Gradle builds on Android (-DGRADLE)"),
							Spacer.Smaller,
							CreateFlagCheckBox(showWindow, editArgs, "-DDEBUG_V8", "Enable the V8 debugger (-DDEBUG_V8)"),
							Spacer.Smaller,
							CreateFlagCheckBox(showWindow, editArgs, "-v", "Enable verbose builds (-v)"))
						.WithMacWindowStyleCompensation()
						.WithPadding(new Thickness<Points>(45, 0, 45, 15)))
				.WithBackground(Theme.PanelBackground);
		}

		static IControl CreateButtons(IObserver<bool> showWindow, Action save)
		{
			var cancelButton = CreateButton(
				icon: Icons.Cancel(Theme.Cancel),
				text: "Cancel",
				clicked: Command.Enabled(() => showWindow.OnNext(false)));

			var doneButton = CreateButton(
				icon: Icons.Confirm(Theme.Active),
				text: "Done",
				clicked: Command.Enabled(() =>
				{
					save();
					showWindow.OnNext(false);
				}));

			var leftButton = Platform.OperatingSystem == OS.Mac ? cancelButton : doneButton;
			var rightButton = Platform.OperatingSystem == OS.Mac ? doneButton : cancelButton;

			return Layout.SubdivideHorizontally(
					Layout.Dock().Right(Separator.Medium).Fill(leftButton),
					rightButton)
				.WithHeight(45);
		}

		static IControl CreateButton(IControl icon, string text, Command clicked)
		{
			return Button.Create(
				clicked: clicked,
				content: bs => Layout.Dock()
					.Left(icon.CenterVertically())
					.Left(Spacer.Small)
					.Fill(Label.Create(
						text: text,
						color: Theme.DefaultText))
					.Center());
		}

		static IControl CreateFlagCheckBox(
			IObservable<bool> showWindow,
			IProperty<ImmutableList<string>> editArgs,
			string flag,
			string text)
		{
			var contains = editArgs.Contains(flag);
			return Layout.StackFromLeft(
					CheckBox.Create(contains.And(showWindow), contains.Toggle()),
					Spacer.Smaller,
					Label.Create(text: text, color: Theme.DefaultText).CenterVertically(),
					Spacer.Medium)
				.OnMouse(pressed: contains.Toggle());
		}

		static IProperty<bool> Contains(this IProperty<ImmutableList<string>> self, string flag)
		{
			return self.Convert(
				convert: flags => flags.Contains(flag),
				convertBack: (flags, shouldContain) => shouldContain 
					? AddFlag(flags.Or(ImmutableList<string>.Empty), flag) 
					: RemoveFlag(flags.Or(ImmutableList<string>.Empty), flag));
		}

		static ImmutableList<string> AddFlag(ImmutableList<string> flags, string flag)
		{
			return flags.Contains(flag) ? flags : flags.ConcatOne(flag).ToImmutableList();
		}

		static ImmutableList<string> RemoveFlag(ImmutableList<string> flags, string flag)
		{
			return flags.Where(f => f != flag).ToImmutableList();
		}
	}
}
