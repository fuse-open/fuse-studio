using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using Outracks.Fuse.Designer;

namespace Outracks.Fuse
{
	using Fusion;
	using IO;

	public static class ProjectMenu
	{
		public static Menu CommandItems(IObservable<Optional<AbsoluteFilePath>> project, IShell shell)
		{
			return Menu.Item("Open project folder", OpenFolder.CreateCommand(shell, project))
				+ Menu.Item("Open in Terminal", OpenTerminal.CreateCommand(shell, project))
				+ OpenTextEditor.CreateMenu(project);
		}

		public static Menu FileItems(IProject project, IShell shell)
		{
			return project
				.Documents
				.SelectPerElement(doc => doc.FilePath)
				.ToObservableEnumerable()
				.CombineLatest(
					project.FilePath,
					(uxFiles, projFile) =>
						CreateOpenMenuItems(projFile.ContainingDirectory, uxFiles.ConcatOne(projFile), shell))
				.Concat();
		}

		static IEnumerable<Menu> CreateOpenMenuItems(AbsoluteDirectoryPath root, IEnumerable<IAbsolutePath> filesAndFolders, IShell shell)
		{
			var folderToFiles = filesAndFolders.ToChildLookup(path => path.ContainingDirectory.ToOptional<IAbsolutePath>());
			return CreateItems(root, folderToFiles, shell);
		}

		static IEnumerable<Menu> CreateItems(AbsoluteDirectoryPath currentDir, ILookup<IAbsolutePath, IAbsolutePath> folderToFiles, IShell shell)
		{
			var dirs = folderToFiles[currentDir].OfType<AbsoluteDirectoryPath>().OrderBy(f => f.Name);
			var files = folderToFiles[currentDir].OfType<AbsoluteFilePath>().OrderBy(f => f.Name);

			foreach (var dir in dirs)
			{
				yield return Menu.Submenu(
					name: dir.Name.ToString() + Path.DirectorySeparatorChar,
					icon: Icons.Folder,
					submenu: CreateItems(dir, folderToFiles, shell).Concat());
			}

			foreach (var file in files)
			{
				var f = file;
				yield return Menu.Item(
					name: file.Name.ToString(),
					icon: Icons.GetFileIcon(f),
					action: () => shell.OpenWithDefaultApplication(f));
			}
		}
	}
}