using System;
using System.Collections.Generic;
using System.Linq;
using Outracks.IO;

namespace Outracks.Fuse.Templates
{
	public class SpawnedTemplate
	{
		public SpawnedTemplate(IEnumerable<AbsoluteFilePath> spawnedProjectFiles)
		{
			SpawnedProjectFiles = spawnedProjectFiles.ToArray();
		}

		public IEnumerable<AbsoluteFilePath> SpawnedProjectFiles { get; private set; }
	}

	public class TemplateSpawner
	{
		public static SpawnedTemplate SpawnTemplate(Template template, AbsoluteDirectoryPath whereToSpawnTemplate, ITemplateVariableResolver variableResolver, IFileSystem fileSystem)
		{
			var spawner = new TemplateSpawner(variableResolver, fileSystem);
			return new SpawnedTemplate(spawner.Spawn(template, whereToSpawnTemplate));
		}

		readonly ITemplateVariableResolver _templateVariableResolver;
		readonly IFileSystem _fileSystem;
		readonly TemplateParser _templateParser;

		public TemplateSpawner(ITemplateVariableResolver templateVariableResolver, IFileSystem fileSystem)
		{
			_templateVariableResolver = templateVariableResolver;
			_fileSystem = fileSystem;
			_templateParser = new TemplateParser(_templateVariableResolver);
		}

		public IEnumerable<AbsoluteFilePath> Spawn(Template template, AbsoluteDirectoryPath containingDir)
		{
			_fileSystem.CreateIfNotExists(containingDir);
			
			return CopyTemplateFiles(template, _templateVariableResolver.ResolveVariable("filename"), containingDir);
		}

		IEnumerable<AbsoluteFilePath> CopyTemplateFiles(Template template, string name, AbsoluteDirectoryPath containingDir)
		{
			foreach (var file in template.TemplateFiles)
			{
				// TODO: Make logic for undoing a template spawning if exceptions occurred during the process.
				var newPath = RelativeFilePath.Parse(file.Path.NativeRelativePath.Replace("_filename_", name));
				var destinationFile = containingDir / newPath;
				var sourceFile = template.BasePath / file.Path;

				_fileSystem.CreateIfNotExists(destinationFile.ContainingDirectory);

				if (_fileSystem.Exists(destinationFile))
					throw new FileAlreadyExist("File {0} already exists.".FormatWith(destinationFile.NativePath));

				if (ShouldReplaceVariables(destinationFile))
					CopyAndReplaceVariables(sourceFile, destinationFile);
				else
					_fileSystem.Copy(sourceFile, destinationFile);

				yield return destinationFile;
			}
		}

		void CopyAndReplaceVariables(AbsoluteFilePath src, AbsoluteFilePath dst)
		{
			using (var backup = _fileSystem.BackupAndDeleteFile(dst))
			{
				try
				{
					var templateDocument = _fileSystem.ReadAllText(src, 5);
				    var newDocument = _templateParser.PreprocessIncludeRegions(templateDocument);
                    newDocument = _templateParser.ReplaceVariables(newDocument);
					_fileSystem.WriteNewText(dst, newDocument);
				}
				catch (Exception)
				{
					backup.Restore();
					throw;
				}
			}
		}

		static bool ShouldReplaceVariables(AbsoluteFilePath file)
		{
			return file.Name.HasExtension(".ux") 
				|| file.Name.HasExtension(".uno")
				|| file.Name.HasExtension(".unoproj")
				|| file.Name.HasExtension(".js");
		}
	}

}
