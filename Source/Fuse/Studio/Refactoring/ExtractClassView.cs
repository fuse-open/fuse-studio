using Outracks.Fusion;

namespace Outracks.Fuse.Refactoring
{
	public static class ExtractClassView
	{
		public static IControl CreateButton(IExtractClassButtonViewModel model)
		{
			var command = model.Command;
			var hoverColor = Theme.Purple;
			var mouseEntered = Command.Enabled(model.HoverEnter);
			var mouseExited = Command.Enabled(model.HoverExit);
			return CreateButton(command, hoverColor, mouseEntered, mouseExited);
		}

		public static void OpenDialog(
			IModalHost modalHost,
			IExtractClassViewModel viewModel)
		{
			var toggleCreateInNewFile = viewModel.CreateInNewFile.Toggle();

			var errorMsg = Label.Create(
					text: viewModel.UserInfo.AsText(),
					font: Theme.DescriptorFont,
					color: Theme.ErrorColor,
					lineBreakMode: LineBreakMode.TruncateTail)
				.CenterVertically()
				.WithHeight(26);

			var optionsPart = 
				Layout.StackFromTop(
					Layout.Dock()
						.Left(CheckBox.Create(viewModel.CreateInNewFile, toggleCreateInNewFile)
								.CenterVertically()
								.WithPadding(right: new Points(4), top: new Points(1)))
						.Fill(
							Label.Create(
									"Extract to new file",
									Theme.DefaultFont,
									color: Theme.DefaultText)
								.OnMouse(pressed: toggleCreateInNewFile)
								.CenterVertically())
						.WithHeight(22),
					Layout.Dock()
						.Left(Control.Empty.WithWidth(21))
						.Fill(
							Label.Create(
									"Create a new .ux file for the class",
									Theme.DefaultFont,
									color: Theme.DescriptorText,
									lineBreakMode: LineBreakMode.TruncateTail))
								.CenterVertically()
						.WithHeight(21),
					Layout.Dock()
						.Left(Control.Empty.WithWidth(21))
						.Fill(ThemedTextBox.Create(viewModel.NewFileName))
						.WithPadding(top: new Points(6), bottom: new Points(3))
				);

			var content =
				Layout.StackFromTop(
					Layout.Dock()
						.Left(
							Label.Create("ux:Class", Theme.DefaultFont, color: Theme.DefaultText)
								.CenterVertically().WithPadding(right: new Points(4)))
						.Fill(ThemedTextBox.Create(viewModel.ClassName))
						.WithPadding(bottom: new Points(23)),
					optionsPart,
					errorMsg)
				.WithPadding(left: new Points(15), top: new Points(15), right: new Points(15));

			modalHost.Open(
				confirm: viewModel.CreateCommand,
				fill: Layout.Dock()
						.Top(
							// Just use button as header for dialog
							CreateButton(Command.Enabled(() => {}), hoverColor: Theme.DefaultText))
						.Fill(content),
				confirmText: "Create class",
				confirmTooltip: "Creates new UX class based on selected element");
		}

		static IControl CreateButton(
			Command command,
			Brush hoverColor = default(Brush),
			Command mouseEntered = default(Command),
			Command mouseExited = default(Command))
		{
			return
				Layout.StackFromTop(
					ThemedButton.Create(
							command: command,
							label: "Make class from selection",
							icon: Icons.ExtractClass(command.IsEnabled),
							tooltip: "Make class from selection",
							hoverColor: hoverColor)
						.WithHeight(45)
						.OnMouse(
							entered: mouseEntered,
							exited: mouseExited),
					Separator.Shadow);
		}

	}
}