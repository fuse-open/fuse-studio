using System.Threading.Tasks;
using Outracks.IO;

namespace Outracks
{
	public interface IFileDialogImplementation
	{
		// NOTE: There are more methods for different kinds of file browsing dialogs
		//		 in the implementations, we can expose them here as we need them

		Task<Optional<FileDialogResult>> SaveFile(FileDialogOptions options);
		Task<Optional<FileDialogResult>> OpenFile(FileDialogOptions options);
	}

	public struct FileDialogResult{
		public AbsoluteFilePath Path;
		public FileFilter Filter;
		public FileDialogResult(AbsoluteFilePath path, FileFilter filter){
			this.Path = path;
			this.Filter = filter;
		}
	}

	public class DirectoryDialogOptions
	{
		public string Caption { get; private set; }
		public AbsoluteDirectoryPath Directory { get; private set; }

		public DirectoryDialogOptions(string caption, AbsoluteDirectoryPath directory = null)
		{
			Directory = directory;
			Caption = caption;
		}
	}

	public class FileDialogOptions
	{
		public string Caption { get; private set; }
		public AbsoluteDirectoryPath Directory { get; private set; }
		public FileFilter[] Filters { get; private set; }

		public FileDialogOptions(string caption, params FileFilter[] filters)
			: this (caption, null, filters)
		{ }

		public FileDialogOptions(string caption, AbsoluteDirectoryPath directory, params FileFilter[] filters)
		{
			Caption = caption;
			Filters = filters;
			Directory = directory;
		}
	}
}