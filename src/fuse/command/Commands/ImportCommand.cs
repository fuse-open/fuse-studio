using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Mono.Options;
using Outracks.Fuse.Import;
using Outracks.IO;

namespace Outracks.Fuse
{
	public class ImportCommand : CliCommand
	{
		public static ImportCommand CreateImportCommand()
		{
			var shell = new Shell();
			var fuse = FuseApi.Initialize("fuse", new List<string>());
			var projectDetector = new ProjectDetector(shell);

			ImportOperation[] importers = {
				new SketchConversionOperation(
					"sketch",
					"Import Sketch symbols from given sketch file or from <project>.sketchFiles",
					shell,
					fuse.Report)
			};

			return new ImportCommand(shell, projectDetector, importers);
		}

		readonly IFileSystem _fileSystem;
		readonly ProjectDetector _projectDetector;
		readonly ImportOperation[] _operations;

		public ImportCommand(
			IFileSystem fileSystem,
			ProjectDetector projectDetector,
			params ImportOperation[] availableOperations)
			: base("import", "Import a file to your fuse X project")
		{
			_fileSystem = fileSystem;
			_projectDetector = projectDetector;
			_operations = availableOperations;
		}

		/// <exception cref="ExitWithError" />
		public override void Run(string[] args, CancellationToken ct)
		{
			try
			{
				var list = false;
				var type = Optional.None<string>();
				var maybeProject = Optional.None<IAbsolutePath>();
				var options = CreateOptions(
					type: s => type = s,
					project: s => maybeProject = Optional.Some(_fileSystem.ResolveAbsolutePath(s)),
					list: s => list = (s != null));

				var remainingArgs = options.Parse(args);

				if (remainingArgs.Count < 1)
				{
					WriteUsage(Console.Error);
					return;
				}

				var project = _projectDetector.GetCurrentProject(maybeProject);
				var file = _fileSystem.ResolveAbsolutePath(remainingArgs[0]);


				if (list)
				{
					var supportedOperations = _operations.Where(o => o.CanExecute(file, project)).ToArray();
					Console.Out.WriteLine(
						supportedOperations.Length == 0
							? "This file can not be imported by Fuse"
							: ListOperations(supportedOperations));
					return;
				}

				ImportOperation operation;
				if (type.HasValue)
				{
					operation = _operations
						.FirstOrNone(op => op.Name.Equals(type.Value, StringComparison.InvariantCultureIgnoreCase))
						.OrThrow(new ExitWithError("Unknown operation type " + type.Value));
				}
				else
				{
					var supportedOperations = _operations.Where(o => o.CanExecute(file, project)).ToArray();

					if (supportedOperations.Length < 1)
						throw new ExitWithError("This file can not be imported by Fuse");

					operation = supportedOperations[0];
				}

				operation.Execute(file, project, remainingArgs);
			}
			catch (ProjectNotFound)
			{
				throw new ExitWithError("Could not find destination project");
			}
			catch (ImportFailed p)
			{
				throw new ExitWithError("Import failed: " + p.Message);
			}
			catch (InvalidPath p)
			{
				throw new ExitWithError("The path specified is invalid: " + p.Path);
			}
		}


		static string ListOperations(IEnumerable<ImportOperation> operations)
		{
			return "The following import operations are supported for this file:\n"
				+ operations.Select(op => "- " + op.Description + " (-t=" + op.Name + ")").Join("\n");
		}


		public override void Help()
		{
			WriteUsage (Console.Out);
			Console.Out.WriteLine ();
			WriteOptions (Console.Out);
		}

		static void WriteUsage(TextWriter writer)
		{
			writer.WriteLine("Usage: fuse import <file> [options] ");
		}

		void WriteOptions(TextWriter writer)
		{
			var options = CreateOptions(_ => { }, _ => { }, _ => { });
			options.WriteOptionDescriptions(Console.Out);

			Console.Out.WriteLine ();

			foreach (var operation in _operations)
			{
				operation.WriteOptions (Console.Out);
			}
		}

		OptionSet CreateOptions(Action<string> type, Action<string> project, Action<string> list)
		{
			return new OptionSet()
			{
				{ "p|project=", "Specify which project to import the file to. By default this is auto-detected based on working directory.", project },
				{ "l|list", "List the available import operations for the specified file", list },
				{ "t|type=", "Specify which import operation to perform. By default this is auto-detected based on file extension.", type },
			};
		}
	}
}
