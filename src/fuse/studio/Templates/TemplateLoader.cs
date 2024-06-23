using System.Collections.Generic;
using System.Linq;
using Outracks.IO;

namespace Outracks.Fuse.Templates
{
	public class TemplateFile
	{
		public RelativeFilePath Path;
	}

	public class TemplateLoader
	{
		public static IEnumerable<Template> LoadTemplatesFrom(AbsoluteDirectoryPath baseDirectory, IFileSystem fileSystem)
		{
			foreach (var dir in fileSystem.GetDirectories(baseDirectory))
			{
				yield return LoadTemplateFrom(dir, fileSystem);
			}
		}

		public static Template LoadTemplateFrom(AbsoluteDirectoryPath baseDir, IFileSystem fileSystem)
		{
			return new TemplateLoader(baseDir, fileSystem).Load();
		}

		readonly Template _template;
		readonly IFileSystem _fileSystem;

		TemplateLoader(AbsoluteDirectoryPath baseDir, IFileSystem fileSystem)
		{
			_template = new Template() { BasePath = baseDir };
			_fileSystem = fileSystem;
		}

		Template Load()
		{
			ApplyManifest(LoadManifest());
			LoadTemplateFiles(_template.BasePath);
			return _template;
		}

		TemplateManifest LoadManifest()
		{
			using (var fileRead = _fileSystem.OpenRead(_template.ManifestFile))
				return TemplateManifestParser.Parse(fileRead);
		}

		void ApplyManifest(TemplateManifest manifest)
		{
			_template.Name = manifest.Name;
			_template.Description = manifest.Description;
			_template.IconFile = manifest.Icon != null ? Optional.Some(_template.ManifestFile.ContainingDirectory / new FileName(manifest.Icon)) : Optional.None();
			_template.Priority = manifest.Priority != null ? int.Parse (manifest.Priority) : 0;
			_template.FileExt = manifest.FileExt != null ? Optional.Some(manifest.FileExt) : Optional.None();
			_template.Alias = manifest.Alias != null ? Optional.Some(manifest.Alias) : Optional.None();
		}

		void LoadTemplateFiles(AbsoluteDirectoryPath directoryPath)
		{
			var files = _fileSystem.GetFiles(directoryPath);
			foreach (var file in files)
				AddTemplateFile(file);

			var directories = _fileSystem.GetDirectories(directoryPath);
			foreach (var dir in directories)
				LoadTemplateFiles(dir);
		}

		void AddTemplateFile(AbsoluteFilePath fileName)
		{
			if (_template.BannedFiles.Contains(fileName)) return;

			var templateFile = new TemplateFile()
			{
				Path = fileName.RelativeTo(_template.BasePath)
			};

			_template.TemplateFiles.Add(templateFile);
		}

	}
}