using System;
using System.IO;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Outracks.Fuse.Studio;
using Outracks.Fuse.Templates;
using Outracks.Fusion;
using Outracks.Fusion.Dialogs;
using Outracks.IO;
using Uno.Collections;
using Uno.Configuration;

namespace Outracks.Fuse.Dashboard
{
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
					Size = Optional.Some(Property.Constant(Optional.Some(new Size<Points>(400, 275)))),
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

			var gitSupport = UserSettings.Bool("CreateProjectGitSupport")
				.Convert(v => v.Or(false), v => v);
			var typescriptSupport = UserSettings.Bool("CreateProjectTypeScriptSupport")
				.Convert(v => v.Or(false), v => v);
			var vscodeSupport = UserSettings.Bool("CreateProjectVSCodeSupport")
				.Convert(v => v.Or(false), v => v);

			return
				Layout.Dock()
					.Bottom(Layout.Dock()
						.Right(
							Buttons.DefaultButtonPrimary(
								text: Texts.Dashboard_Button_Create,
								cmd: Observable.CombineLatest(
									projectName, projectLocation, gitSupport, typescriptSupport, vscodeSupport,
									(name, location, git, typescript, vscode) =>
									{
										var projectDirectory =
											location.SelectMany(l =>
												name.Select(n => l / n))
											.Where(d => !_shell.Exists(d));

										return Command.Create(
											isEnabled: projectDirectory.HasValue,
											action: async () =>
											{
												try
												{
													var spawnTemplate = new SpawnTemplate(_shell);
													var resultPath = spawnTemplate.CreateProject(name.Value.ToString(), template.Filter(git, typescript, vscode), location.Value);
													var projectPath = resultPath / new FileName(name.Value + ".unoproj");

													await Application.OpenDocument(projectPath, showWindow: true);
													dialog.Close(true);
												}
												catch (Exception e)
												{
													_fuse.Report.Exception(this, e);
													MessageBox.Show("Could not create a project.\n\n" + e.Message, "Error", MessageBoxType.Error);
												}
											});
									}).Switch())
								.WithWidth(104))
						.Right(Buttons.DefaultButton(text: Texts.Button_Cancel, cmd: Command.Enabled(() => dialog.Close(false)))
							.WithWidth(104)
							.WithPadding(right: new Points(16)))
						.Fill())

					.Fill(
						Layout.StackFromTop(
							Label.Create("Name", color: Theme.DescriptorText)
								.WithPadding(LabelThickness),
							ValidatedTextBox.Create(validatedName)
								.WithPadding(ControlPadding),
							Control.Empty
								.WithHeight(8),
							Label.Create("Location", color: Theme.DescriptorText)
								.WithPadding(LabelThickness),
							Layout.Dock()
								.Right(
									Buttons.DefaultButton(text: Texts.Button_Browse, cmd: Command.Enabled(async () =>
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
							Control.Empty
								.WithHeight(8),
							Label.Create("Tooling", color: Theme.DescriptorText)
								.WithPadding(LabelThickness),
							Control.Empty
								.WithHeight(6),
							Layout.StackFromLeft(
									Spacer.Small2,
									CheckBox.Create(gitSupport, gitSupport.Toggle(true)),
									Spacer.Smaller,
									Label.Create(text: "Git", color: Theme.DescriptorText).CenterVertically(),
									Spacer.Medium)
								.OnMouse(gitSupport.Toggle(true)),
							Control.Empty
								.WithHeight(3),
							Layout.StackFromLeft(
									Spacer.Small2,
									CheckBox.Create(typescriptSupport, typescriptSupport.Toggle(true)),
									Spacer.Smaller,
									Label.Create(text: "TypeScript (package.json)", color: Theme.DescriptorText).CenterVertically(),
									Spacer.Medium)
								.OnMouse(typescriptSupport.Toggle(true)),
							Control.Empty
								.WithHeight(3),
							Layout.StackFromLeft(
									Spacer.Small2,
									CheckBox.Create(vscodeSupport, vscodeSupport.Toggle(true)),
									Spacer.Smaller,
									Label.Create(text: "Visual Studio Code", color: Theme.DescriptorText).CenterVertically(),
									Spacer.Medium)
								.OnMouse(vscodeSupport.Toggle(true)),
							Control.Empty
								.WithHeight(8),
							ErrorLabel.Create(possibleProjectName)
								.WithPadding(ControlPadding)))
					.WithPadding(ControlPadding)
					.WithBackground(Theme.WorkspaceBackground);
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
						.WithBackground(Theme.WorkspaceBackground), // TODO Should show full path on mouse over if truncated?
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
			return AbsoluteDirectoryPath.Parse(unoConfig.GetFullPath("Fuse.Templates"));
		}
	}
}
