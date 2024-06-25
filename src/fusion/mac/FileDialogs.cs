using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppKit;
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using Outracks.IO;

namespace Outracks.Fusion.Mac
{
	public class FileDialogs : IFileDialogImplementation
	{
		readonly NSWindow _window;
		public FileDialogs(NSWindow window)
		{
			_window = window;
		}

		public static void Initialize()
		{
			FileDialog.Implementation = windowControl =>
			{
				var window =
					windowControl.Select(control => control.NativeHandle)
						.As<NSView>()
						.Select(nativeHandle => nativeHandle.Window);
				return new FileDialogs(window.OrDefault());
			};
		}

		public Task<Optional<FileDialogResult>> SaveFile (FileDialogOptions options)
		{
			return Fusion.Application.MainThread.InvokeAsync(() => SaveFileSync(options));
		}

		public Task<Optional<FileDialogResult>> OpenFile (FileDialogOptions options)
		{
			return Fusion.Application.MainThread.InvokeAsync(() => OpenFileSync(options));
		}

		public Task<Optional<IEnumerable<FileDialogResult>>> OpenFiles (FileDialogOptions options)
		{
			return Fusion.Application.MainThread.InvokeAsync(() => OpenFilesSync(options));
		}

		public Task<Optional<AbsoluteDirectoryPath>> SelectDirectory (DirectoryDialogOptions options)
		{
			return Fusion.Application.MainThread.InvokeAsync(() => SelectDirectorySync (options));
		}

		Optional<FileDialogResult> SaveFileSync(FileDialogOptions options)
		{
			var dialog = new SaveDialog (options.Filters)
			{
				Title = options.Caption,
			};

			if (options.Directory != null)
				dialog.Directory = options.Directory;

			var success = dialog.Run(_window);

			FileDialogResult result = new FileDialogResult();
			if (success)
			{
				result = new FileDialogResult (dialog.FilePath, dialog.CurrentFilter);
			}

			return success
				? Optional.Some(result)
				: Optional.None<FileDialogResult>();
		}

		Optional<FileDialogResult> OpenFileSync(FileDialogOptions options)
		{
			var dialog = new OpenDialog (options.Filters)
			{
				Title = options.Caption,
				Multiselect = false
			};

			if (options.Directory != null)
				dialog.Directory = options.Directory;

			var success = dialog.Run();

			FileDialogResult result = new FileDialogResult();
			if (success)
			{
				result = new FileDialogResult (dialog.FilePath, dialog.CurrentFilter);
			}

			return success
				? Optional.Some(result)
				: Optional.None<FileDialogResult>();
		}

		Optional<IEnumerable<FileDialogResult>> OpenFilesSync(FileDialogOptions options)
		{
			var dialog = new OpenDialog (options.Filters)
			{
				Title = options.Caption,
				Multiselect = true
			};

			if (options.Directory != null)
				dialog.Directory = options.Directory;

			var success = dialog.Run();

			IEnumerable<FileDialogResult> result = null;
			if (success)
			{
				result = dialog.FilePaths.Select(f => new FileDialogResult (f, dialog.CurrentFilter));
			}

			return success
				? Optional.Some(result)
				: Optional.None<IEnumerable<FileDialogResult>>();
		}

		Optional<AbsoluteDirectoryPath> SelectDirectorySync(DirectoryDialogOptions options)
		{
			var ofd = new OpenDialog()
			{
				Title = options.Caption,
				CanChooseDirectories = true,
				CanCreateDirectories = true,
				CanChooseFiles = false
			};

			if (options.Directory != null)
				ofd.Directory = options.Directory;

			var success = ofd.Run();
			var path = ofd.Directory;

			return success
				? Optional.Some(path)
				: Optional.None<AbsoluteDirectoryPath>();
		}
	}

	class Dialog
	{
		protected readonly FileFilter[] _filters;
		protected readonly NSSavePanel _panel;

		protected Dialog(NSSavePanel panel, FileFilter []filters)
		{
			_filters = filters;
			_panel = panel;
			CreateFilters(filters);
		}

		void CreateFilters(FileFilter []filters)
		{
			if (filters.Any ())
			{
				var label = new NSTextField ();
				var fileTypes = new NSPopUpButton ();
				fileTypes.AddItems (filters.Select (f => f.Label).ToArray ());

				var fileTypeView = new NSView ();
				fileTypeView.AutoresizingMask = NSViewResizingMask.HeightSizable | NSViewResizingMask.WidthSizable;
				const int padding = 15;

				label.StringValue = "Show Files";
				label.DrawsBackground = false;
				label.Bordered = false;
				label.Bezeled = false;
				label.Editable = false;
				label.Selectable = false;
				label.SizeToFit ();
				fileTypeView.AddSubview (label);

				fileTypes.SizeToFit ();
				fileTypes.Activated += (sender, e) =>
				{
					var currentFilter = filters.FirstOrDefault (f => f.Label == fileTypes.TitleOfSelectedItem);
					SetCurrentItem (currentFilter);

					// THIS DOES NOT WORK ON MAC OS FROM MAVERICS TO YOSEMITE
					// There exists hacks, however they are dependent on OS X version
					// I have filed bug as many others, but I guess this will never be fixed
					_panel.ValidateVisibleColumns ();
					_panel.Update ();
				};
				fileTypeView.AddSubview (fileTypes);
				fileTypes.SetFrameOrigin (new CGPoint(label.Frame.Width + 10, padding));

				label.SetFrameOrigin (new CGPoint(0, padding + (fileTypes.Frame.Height - label.Frame.Height) / 2));

				fileTypeView.Frame = new CGRect (0, 0, fileTypes.Frame.Width + label.Frame.Width + 10, fileTypes.Frame.Height + padding * 2);

				_panel.AccessoryView = fileTypeView;
				if (filters.Any ())
					SetCurrentItem (filters.First ());
			}
			else
				_panel.AccessoryView = null;
		}

		public string Title
		{
			get
			{
				return _panel.Title;
			}
			set
			{
				_panel.Title = value;
			}
		}

		public AbsoluteFilePath FilePath
		{
			get { return _panel.Url.ToAbsoluteFilePath(); }
		}

		public AbsoluteDirectoryPath Directory
		{
			get
			{
				return _panel.DirectoryUrl.ToAbsoluteDirectoryPath ();
			}
			set
			{
				_panel.DirectoryUrl = value.ToNSUrl ();
			}
		}

		public bool CanCreateDirectories
		{
			get { return _panel.CanCreateDirectories; }
			set { _panel.CanCreateDirectories = value; }
		}

		public FileFilter CurrentFilter { get; private set; }

		void SetCurrentItem(FileFilter filter)
		{
			CurrentFilter = filter;

			var macfilters = new List<string> ();
			foreach (var filterext in filter.Extensions)
			{
				macfilters.Add (filterext.TrimStart ('*', '.'));
			}

			_panel.AllowsOtherFileTypes = false;
			SetAllowedFileTypes(_panel, macfilters.Distinct().ToArray ());
		}

		public bool Run(Optional<NSWindow> window = default(Optional<NSWindow>))
		{
			if (window.HasValue) {
				_panel.BeginSheet (window.Value, result => NSApplication.SharedApplication.StopModalWithCode (result));
				return NSApplication.SharedApplication.RunModalForWindow (window.Value) == 1;
			} else {
				return _panel.RunModal() == 1;
			}
		}

		static readonly IntPtr selSetAllowedFileTypes_Handle = Selector.GetHandle("setAllowedFileTypes:");

		static void SetAllowedFileTypes(NSSavePanel openPanel, string []extensions)
		{
			if (extensions.Length == 0)
			{
				Messaging.void_objc_msgSendSuper_IntPtr (openPanel.SuperHandle, selSetAllowedFileTypes_Handle, IntPtr.Zero);
			}
			else
			{
				NSArray array = NSArray.FromStrings (extensions);
				Messaging.void_objc_msgSendSuper_IntPtr (openPanel.SuperHandle, selSetAllowedFileTypes_Handle, array.Handle);
			}
		}
	}

	class SaveDialog : Dialog
	{
		public SaveDialog(Optional<IEnumerable<FileFilter>> filters = default(Optional<IEnumerable<FileFilter>>))
			: base(new NSSavePanel(), filters.HasValue ? filters.Value.ToArray() : new FileFilter[0])
		{
		}
	}

	class OpenDialog : Dialog
	{
		readonly NSOpenPanel _nsOpenPanel;

		public OpenDialog(Optional<IEnumerable<FileFilter>> filters = default(Optional<IEnumerable<FileFilter>>))
			: base(new NSOpenPanel(), filters.HasValue ? filters.Value.ToArray() : new FileFilter[0])
		{
			_nsOpenPanel = (NSOpenPanel)_panel;
		}

		public bool Multiselect
		{
			get { return _nsOpenPanel.AllowsMultipleSelection; }
			set { _nsOpenPanel.AllowsMultipleSelection = value; }
		}

		public bool CanChooseDirectories
		{
			get { return _nsOpenPanel.CanChooseDirectories; }
			set { _nsOpenPanel.CanChooseDirectories = value; }
		}

		public bool CanChooseFiles
		{
			get { return _nsOpenPanel.CanChooseFiles; }
			set { _nsOpenPanel.CanChooseFiles = value; }
		}

		public IEnumerable<AbsoluteFilePath> FilePaths
		{
			get { return _nsOpenPanel.Urls.Select (s => s.ToAbsoluteFilePath ()); }
		}
	}
}
