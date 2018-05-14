using System.Linq;
using Outracks.IO;
using Uno.ProjectFormat;

namespace Outracks.Fuse.Templates
{
	public class SpawnTemplate
	{
		readonly IFileSystem _fileSystem;

		public SpawnTemplate(IFileSystem fileSystem)
		{
			_fileSystem = fileSystem;
		}

		public AbsoluteDirectoryPath CreateProject(string name, Template template, Optional<AbsoluteDirectoryPath> destinationPath)
		{
			var location = destinationPath.HasValue ? destinationPath.Value : DirectoryPath.GetCurrentDirectory();

			var projectDir = location / name;

			if (_fileSystem.Exists(projectDir) &&
				(_fileSystem.GetFiles(projectDir).Any() || _fileSystem.GetDirectories(projectDir).Any()))
				throw new ProjectFolderNotEmpty();

			var environment = new TemplateVariableResolver().With("filename", name);

			TemplateSpawner.SpawnTemplate(template, projectDir, environment, _fileSystem);

			return projectDir;
		}

		public Optional<AbsoluteFilePath> CreateFile(string name, Template template, Optional<AbsoluteDirectoryPath> destinationPath)
		{
			var location = destinationPath.HasValue ? destinationPath.Value : DirectoryPath.GetCurrentDirectory();

			var fileName = location / new FileName(name);
			var fileNameWithExt = template.FileExt.MatchWith(fileName.AddExtension, () => fileName);
			
			var project = FindAndLoadProjectInDirectory(location);

			var nss = new NamespaceName(project.GetString("RootNamespace") ?? "");

			var rootDir = location.ContainingDirectory;
			var relativeDir = location.RelativeTo(rootDir);
			var ns = relativeDir.ToNamespace(nss).Or(new NamespaceName(""));
            
			var fullTypeName = ns.FullName == "" ? fileName.Name.ToString() : ns.FullName + "." + fileName.Name;

			var environment = new TemplateVariableResolver()
				.With("filename", fileNameWithExt.Name.ToString())
				.With("typename", fileName.Name.ToString())
				.With("fulltypename", fullTypeName)
				.With(ns.FullName == "" ? "exclude_namespace" : "include_namespace", "") //we just check the existence of the key so no value is needed
				.With("namespace", ns.FullName);

			var result = TemplateSpawner.SpawnTemplate(template, location, environment, _fileSystem);

			return result.SpawnedProjectFiles.FirstOrNone();
		}

		Project FindAndLoadProjectInDirectory(AbsoluteDirectoryPath directory)
		{
			if(directory.ContainingDirectory == null)
				throw new ProjectNotFound();
			
			var projectPath = _fileSystem.GetFiles(directory).FirstOrDefault(f => f.Name.HasExtension("unoproj"));
			if (projectPath == null)
				return FindAndLoadProjectInDirectory(directory.ContainingDirectory);

			var unoProj = Project.Load(projectPath.NativePath);
			unoProj.AddDefaults();
			return unoProj;
		}
	}
}