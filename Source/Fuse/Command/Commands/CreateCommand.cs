using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using Mono.Options;
using Uno.Configuration;

namespace Outracks.Fuse
{
	using Analytics;
	using IO;
	using Templates;

	public class CreateCommand : CliCommand
	{
		public static CreateCommand CreateCreateCommand()
		{
			var shell = new Shell();
			var coloredConsole = ColoredTextWriter.Out;
			var fuse = FuseApi.Initialize("Fuse", new List<string>());
			return new CreateCommand(
				shell,
				() => TemplateLoader.LoadTemplatesFrom(UnoConfig.Current.GetTemplatesDir() / "Projects", shell),
				() => TemplateLoader.LoadTemplatesFrom(UnoConfig.Current.GetTemplatesDir() / "Files", shell),
				coloredConsole,
				fuse);
		}


		readonly IFileSystem _fileSystem;
		readonly Func<IEnumerable<Template>> _projectTemplates;
		readonly Func<IEnumerable<Template>> _fileTemplates;
		readonly ColoredTextWriter _textWriter;
		readonly IFuse _fuse;
		readonly OptionSet _optionSet;

		bool _shouldListPackages = false;

		public CreateCommand(
			IFileSystem shell,
			Func<IEnumerable<Template>> projectTemplates,
			Func<IEnumerable<Template>> fileTemplates,
			ColoredTextWriter textWriter,
			IFuse fuse)
			: base("create", "Create a project or file from a template")
		{
			_fileTemplates = fileTemplates;
			_textWriter = textWriter;
			_fuse = fuse;
			_projectTemplates = projectTemplates;
			_fileSystem = shell;
			_optionSet = new OptionSet()
			{
				{"l|list", "List all available templates.", s => { _shouldListPackages = true; }}
			};
		}

		public override void Help()
		{
			var templates = _projectTemplates().Concat(_fileTemplates());

			_textWriter.WriteHelp(new HelpArguments(
				new HelpHeader("fuse create", Description),
				new HelpSynopsis(Usage),
				new HelpDetailedDescription(
					"When creating a file, a project must be within reach from the destination folder.\n" + 
					"A project is within reach if current or any of the parent directories contains a project.\n" +
					"NOTE: Files are added to the project within reach, and will be part of next build of project."),
				new HelpOptions(new[]
				{
					_optionSet.ToTable(),
					new Table("Templates",
						templates
							.OrderByDescending(t => t.Priority)
							.Select(t => new Row(t.Alias.Or(""), t.Name, t.Description)))
				})));
		}

		public override void Run(string[] args, CancellationToken ct)
		{
			var parsedArgs = _optionSet.Parse(args);

			if (_shouldListPackages)
				ListPackages();
			else
				Create(parsedArgs);
		}

		void ListPackages()
		{		
			using(_textWriter.PushColor(ConsoleColor.Yellow))
				_textWriter.WriteLine("Project Templates");
			_projectTemplates()
				.OrderBy(p => p.Priority)
				.Each(p => _textWriter.WriteLine(p.Name));

			_textWriter.WriteLine();

			using (_textWriter.PushColor(ConsoleColor.Yellow))
				_textWriter.WriteLine("File Templates");
			_fileTemplates()
				.OrderBy(p => p.Priority)
				.Each(t => _textWriter.WriteLine(t.Name));
		}

		void Create(IList<string> args)
		{
			Task.Run(
				() => _fuse.ConnectOrSpawn("Fuse create", Timeout.InfiniteTimeSpan));

			var argIdx = 0;
			var templateName = args.TryGetAt(argIdx++)
				.OrThrow(new ExitWithError(Usage));
			
			var name = args.TryGetAt(argIdx++).Or("Untitled");

			var validatedName = FileName.Validate(name);
			if (!validatedName.HasValue) 
				throw new ExitWithError(validatedName.Error.Capitalize());

			try
			{
				var destPath = args.TryGetAt(argIdx++).Select(
					p => (_fileSystem.ResolveAbsolutePath(p) as AbsoluteDirectoryPath).ToOptional()
						.OrThrow(new ExitWithError("Invalid project path" + ": " + args[argIdx - 1])));

				var spawnTemplate = new SpawnTemplate(_fileSystem);
				var projectTemplate = _projectTemplates().FirstOrDefault(t => TemplateNameEquals(t, templateName));
				var fileTemplate = _fileTemplates().FirstOrDefault(t => TemplateNameEquals(t, templateName));

				if (projectTemplate == null && fileTemplate == null)
					throw new ExitWithError("Unknown template name, see fuse help create for a list of valid template names.");

				var templateIsProjectTemplate = projectTemplate != null;

				if (templateIsProjectTemplate)
				{
					var resultPath = spawnTemplate.CreateProject(name, projectTemplate, destPath);
					using (_textWriter.PushColor(ConsoleColor.Green))
						_textWriter.WriteLine("Created project: '" + name + "' at '" + resultPath.NativePath + "'");
				}
				else
				{
					var resultPath = spawnTemplate.CreateFile(name, fileTemplate, destPath)
						.OrThrow(new FailedToCreateFileFromTemplate("Failed to create file from template (unknown reason)"));
					using (_textWriter.PushColor(ConsoleColor.Green))
						_textWriter.WriteLine("Created file at '" + resultPath.NativePath + "'");
				}
			}
			catch (ProjectNotFound)
			{
				throw new ExitWithError("Could not find a project to put the file in, please check if destination folder or its parents contains a project.");
			}
			catch (FileAlreadyExist e)
			{
				throw new ExitWithError(e.Message);
			}
			catch (InvalidPath p)
			{
				throw new ExitWithError("Invalid project path" + ": " + p.Path);
			}
			catch (SecurityException e)
			{
				throw new ExitWithError(e.Message);
			}
			catch (DaemonException e)
			{
				throw new ExitWithError(e.Message);
			}
			catch (FailedToCreateFileFromTemplate e)
			{
				throw new ExitWithError(e.Message);
			}
			catch (ProjectFolderNotEmpty)
			{
				throw new ExitWithError("A folder with that name already exists, and it is not empty.");
			}
			catch (UnauthorizedAccessException e)
			{
				throw new ExitWithError(e.Message);
			}
			catch (IOException e)
			{
				throw new ExitWithError(e.Message);
			}
			catch (FailedToAddProjectToRecentList e)
			{
				throw new ExitWithError(e.Message);
			}
		}

		public static string Usage
		{
			get { return "fuse create (<Alias>|<TemplateName>) [<Name>] [<DestinationDirectory>]"; }
		}

		static bool TemplateNameEquals(Template template, string name)
		{
			return template.Name == name || template.Alias == name;
		}
	}

	class FailedToAddProjectToRecentList : Exception
	{
		public FailedToAddProjectToRecentList(string message) : base(message)
		{			
		}
	}

	class FailedToCreateFileFromTemplate : Exception
	{
		public FailedToCreateFileFromTemplate(string message) : base(message)
		{			
		}
	}
}