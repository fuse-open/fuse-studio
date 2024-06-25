using System.Collections.Generic;
using Outracks.IO;

namespace Outracks.Fuse.Templates
{
	public class Template
	{
		public string Name;
		public string Description;
		public Optional<string> FileExt;
		public Optional<string> Alias;
		public Optional<AbsoluteFilePath> IconFile;
		public int Priority;
		public AbsoluteDirectoryPath BasePath;
		public readonly List<TemplateFile> TemplateFiles = new List<TemplateFile>();

		public AbsoluteFilePath ManifestFile
		{
			get { return BasePath / new FileName("manifest.xml"); }
		}

		public IEnumerable<AbsoluteFilePath> BannedFiles
		{
			get
			{
				yield return ManifestFile;
				if (IconFile.HasValue)
					yield return IconFile.Value;
			}
		}

		public override string ToString()
		{
			return Name;
		}

		public Template Filter(bool git, bool typescript, bool vscode)
		{
			var result = new Template {
				Name = Name,
				Description = Description,
				FileExt = FileExt,
				Alias = Alias,
				IconFile = IconFile,
				Priority = Priority,
				BasePath = BasePath,
			};
			
			foreach (var file in TemplateFiles)
			{
				var name = file.Path.ToString();

				if (!git && (
						name == ".gitattributes" ||
						name == ".gitignore"
					) ||
					!typescript && (
						name == "package.json" ||
						name == "tsconfig.json"
					) ||
					!vscode && name.StartsWith(".vscode/"))
					continue;

				result.TemplateFiles.Add(file);
			}
			
			return result;
		}
	}
}
