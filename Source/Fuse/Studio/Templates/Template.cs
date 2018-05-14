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
	}
}