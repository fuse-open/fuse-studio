using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Outracks.Fusion.Windows
{
	using IO;

	public class FileDialogs : IFileDialogImplementation
	{
	    readonly System.Windows.Window _window;
		readonly System.Windows.Threading.Dispatcher _dispatcher;
		readonly Action _focus;

		public static void Initialize()
		{
			FileDialog.Implementation = windowControl =>
			{
				var window = windowControl.Select(x => x.NativeHandle).As<FrameworkElement>()
					.SelectMany(
						x =>
						{
							var hwndSource = PresentationSource.FromVisual(x);
							if (hwndSource == null)
								return Optional.None();

							var w = hwndSource.RootVisual as System.Windows.Window;
							if (w == null)
								return Optional.None();

							return Optional.Some(w);
						});
				return new FileDialogs(window.OrDefault());
			};
		}

		public FileDialogs(System.Windows.Window window)
	    {
		    _window = window;
		    _dispatcher = _window == null
				? System.Windows.Application.Current.Dispatcher
				: _window.Dispatcher;

			_focus = _window == null
				? new Action(() => { })
				: (() => _window.Focus());
	    }

	    public Task<Optional<FileDialogResult>> SaveFile(FileDialogOptions options)
	    {
			return _dispatcher.InvokeAsync(() => SaveFileSync(options)).Task;
		}

		public Task<Optional<FileDialogResult>> OpenFile(FileDialogOptions options)
		{
			return _dispatcher.InvokeAsync(() => OpenFileSync(options)).Task;
		}

		public Task<Optional<IEnumerable<FileDialogResult>>> OpenFiles(FileDialogOptions options)
		{
			return _dispatcher.InvokeAsync(() => OpenFilesSync(options)).Task;
		}

		public Task<Optional<AbsoluteDirectoryPath>> SelectDirectory(DirectoryDialogOptions options)
		{
			return _dispatcher.InvokeAsync(() => SelectDirectorySync(options)).Task;
		}

		Optional<FileDialogResult> SaveFileSync(FileDialogOptions options)
		{
			var ofd = new Microsoft.Win32.SaveFileDialog
			{
				Title = options.Caption,
				Filter = options.Filters.CreateFilterString(),
				OverwritePrompt = true,
			};

			if (options.Directory != null)
				ofd.InitialDirectory = options.Directory.NativePath;

			var success = _window == null ? ofd.ShowDialog() : ofd.ShowDialog(_window);
			var filename = ofd.FileName;
			var result = new FileDialogResult();
			if (success.Value)
			{
				result.Path = AbsoluteFilePath.Parse(ofd.FileName);
				result.Filter = FilterAtIndex(ofd.Filter, ofd.FilterIndex);
			}
			Focus();

			return success.Value
				? Optional.Some(result)
				: Optional.None<FileDialogResult>();
		}

		Optional<FileDialogResult> OpenFileSync(FileDialogOptions options)
		{
			var ofd = new Microsoft.Win32.OpenFileDialog
			{
				Title = options.Caption,
				Filter = options.Filters.CreateFilterString(),
				CheckPathExists = true,
				CheckFileExists = true,
			};

			FilterString.Parse(ofd.Filter).Each(f => Console.WriteLine(f));

			if (options.Directory != null)
				ofd.InitialDirectory = options.Directory.NativePath;

			var success = _window == null ? ofd.ShowDialog() : ofd.ShowDialog(_window);
			var filename = ofd.FileName;
			var result = new FileDialogResult();
			if (success.Value)
			{
				result.Path = AbsoluteFilePath.Parse(ofd.FileName);
				result.Filter = FilterAtIndex(ofd.Filter, ofd.FilterIndex);
			}

			Focus();

			return success.Value
				? Optional.Some(result)
				: Optional.None<FileDialogResult>();
		}

		Optional<IEnumerable<FileDialogResult>> OpenFilesSync(FileDialogOptions options)
	    {
			var ofd = new Microsoft.Win32.OpenFileDialog
			{
				Title = options.Caption,
				Filter = options.Filters.CreateFilterString(),
				CheckPathExists = true,
				CheckFileExists = true,
				Multiselect = true,
			};

			Console.WriteLine(FilterString.Parse(ofd.Filter));

			if (options.Directory != null)
				ofd.InitialDirectory = options.Directory.NativePath;

			var success = _window == null ? ofd.ShowDialog() : ofd.ShowDialog(_window);
			var filenames = ofd.FileNames;

			filenames.Select(AbsoluteFilePath.Parse);
			IEnumerable<FileDialogResult> results = null;
			if (success.Value)
			{
				var f = FilterAtIndex(ofd.Filter, ofd.FilterIndex);
				results = ofd.FileNames.Select(
					p => new FileDialogResult(AbsoluteFilePath.Parse(p), f)
				);
			}

			Focus();

			return success.Value
				? Optional.Some(results)
				: Optional.None<IEnumerable<FileDialogResult>>();
	    }

		Optional<AbsoluteDirectoryPath> SelectDirectorySync(DirectoryDialogOptions options)
		{
			var ofd = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog
			{
				Description = options.Caption,
				ShowNewFolderButton = true,
				UseDescriptionForTitle = true,
			};

			if (options.Directory != null)
				ofd.SelectedPath = options.Directory.NativePath;

			var success = _window == null ? ofd.ShowDialog() : ofd.ShowDialog(_window);
			var path = ofd.SelectedPath;

			return success.HasValue && success.Value
				? Optional.Some(AbsoluteDirectoryPath.Parse(path))
				: Optional.None<AbsoluteDirectoryPath>();
		}

	    void Focus()
	    {
			_focus();
	    }

		static FileFilter FilterAtIndex(string filterStr, int idx){
			idx--; //Filters start at 1 for whatever reason
			var a = FilterString.Parse(filterStr);
			return a[idx];
		}

	
    }
}
