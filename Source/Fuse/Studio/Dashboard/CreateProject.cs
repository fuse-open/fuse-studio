using System;
using System.IO;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Uno.Collections;
using Uno.Configuration;

namespace Outracks.Fuse.Dashboard
{
	using Designer;
	using Fusion;
	using IO;
	using Templates;

	class CreateProject
	{
		readonly Shell _shell;
		readonly IFuse _fuse;

		private static readonly Thickness<Points> LabelThickness = new Thickness<Points>(6, 0, 6, 0);
		private static readonly Thickness<Points> ControlPadding = new Thickness<Points>(8, 8, 8, 8);

		public CreateProject(IFuse fuse)
		{
			_fuse = fuse;
			_shell = new Shell();
		}

		public Task<bool> ShowDialog(Template template, IDialog<object> parent)
		{
			return parent.ShowDialog<bool>(dialog => 
				new Window
				{
					Style = WindowStyle.Sheet,
					Title = Observable.Return("New Project"),
					Size = Optional.Some(Property.Constant(Optional.Some(new Size<Points>(400, 200)))),
					Content = Control.Lazy(() => CreateContent(template, dialog)),
				});
		}

		IControl CreateContent(Template template, IDialog<bool> dialog)
		{
			var projectLocation =
				UserSettings.Folder("MostRecentlyUsedFolder")
					.Convert(v => v.Or(Optional.Some(_fuse.ProjectsDir)), d => d)
					.AutoInvalidate(TimeSpan.FromMilliseconds(100));
			var validatedLocation = projectLocation.Validate(v => v.NativePath, ValidateExistingDirectory);

			var projectName = Property.Create(
				Optional.Some(_shell.MakeUnique(projectLocation.Latest().First().Value / "App").Name));
			var validatedName = 
				projectName.Validate(v => v.ToString(), DirectoryName.Validate);

			var possibleProjectName = projectLocation.CombineLatest(
					projectName,
					(loc, pro) => { return loc.SelectMany(l => pro.Select(n => l / n)); })
				.Select(
					d => d.HasValue && Directory.Exists(d.Value.NativePath) ? 
					Optional.Some("Project '" + d.Value.Name + "' already exists in " + d.Value.ContainingDirectory.NativePath) : 
					Optional.None());

			return
				Layout.Dock()
					.Bottom(Layout.Dock()
						.Right(
							Buttons.DefaultButtonPrimary(
								text: "Create",
								cmd: Observable.CombineLatest(
									projectName, projectLocation,
									(name, location) =>
									{
										var projectDirectory =
											location.SelectMany(l =>
												name.Select(n => l / n))
											.Where(d => !_shell.Exists(d));

										return Command.Create(
											isEnabled: projectDirectory.HasValue,
											action: async () =>
											{
												var spawnTemplate = new SpawnTemplate(_shell);
												var resultPath = spawnTemplate.CreateProject(name.Value.ToString(), template, location.Value);
												var projectPath = resultPath / new FileName(name.Value + ".unoproj");

												await Application.OpenDocument(projectPath, showWindow: true);
												dialog.Close(true);
											});
									}).Switch())
								.WithWidth(104))
						.Right(Buttons.DefaultButton(text: "Cancel", cmd: Command.Enabled(() => dialog.Close(false)))
							.WithWidth(104)
							.WithPadding(right: new Points(16)))
						.Fill())

					.Fill(
						Layout.StackFromTop(
							Label.Create("Name:", color: Theme.DefaultText)
								.WithPadding(LabelThickness),
							ValidatedTextBox.Create(validatedName)
								.WithPadding(ControlPadding),
							Control.Empty
								.WithHeight(8),
							Label.Create("Location:", color: Theme.DefaultText)
								.WithPadding(LabelThickness),
							Layout.Dock()
								.Right(
									Buttons.DefaultButton(text: "Browse", cmd: Command.Enabled(async () =>
										{
											var directory = await dialog.BrowseForDirectory(await projectLocation.FirstAsync().Or(DirectoryPath.GetCurrentDirectory()));
											if (directory.HasValue)
											{
												projectLocation.Write(directory.Value, save: true);
											}
										}))
									.WithWidth(80)
									.WithHeight(22)
									.CenterVertically()
									.WithPadding(LabelThickness)
									)
								.Fill(ValidatedTextBox.Create(validatedLocation)
									.WithPadding(ControlPadding)),
							ErrorLabel.Create(possibleProjectName)
								.WithPadding(ControlPadding)))
					.WithPadding(ControlPadding)
					.WithPadding(ControlPadding)
					.WithBackground(Theme.PanelBackground);
		}

		public static IValidationResult<AbsoluteDirectoryPath> ValidateExistingDirectory(string nativePath)
		{
			var parsed = AbsoluteDirectoryPath.TryParse(nativePath);
			if (parsed.HasValue && !Directory.Exists(nativePath))
			{
				return Optional.None<AbsoluteDirectoryPath>().AsValidationResult("No such directory");
			}
			return parsed.AsValidationResult("Invalid path");
		}

		static class ValidatedTextBox
		{
			public static IControl Create(IValidatedProperty<string> value)
			{
				return Layout.StackFromTop(
					TextBox.Create(value, foregroundColor: Theme.DefaultText)
						.WithHeight(20)
						.WithOverlay(Shapes.Rectangle(stroke: Theme.FieldStroke))
						.WithBackground(Theme.FieldBackground), // TODO Should show full path on mouse over if truncated?
					ErrorLabel.Create(value.ValidationError)
				);
			}


		}

		static class ErrorLabel
		{
			public static IControl Create(IObservable<Optional<string>> error)
			{
				return error.SelectPerElement(
						v => Label.Create(v, Theme.DescriptorFont, color: Theme.FieldErrorStroke.Brush)
							.WithHeight(15))
					.Or(Control.Empty).Switch();
			}
		}
		
	}

	public static class UnoConfigExtensions
	{

		public static AbsoluteDirectoryPath GetTemplatesDir(this UnoConfig unoConfig)
		{
			return AbsoluteDirectoryPath.Parse(unoConfig.GetFullPath("TemplatesDirectory"));
		}
	}
}