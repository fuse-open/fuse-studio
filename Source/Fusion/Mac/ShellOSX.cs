using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AppKit;
using Foundation;
using CoreFoundation;

namespace Outracks.IO.Native
{
	using Fusion;

	public class ShellOSX : IOldShell
	{
		public string Name
		{
			get
			{
				return "Finder";
			}
		}

		public void OpenWithDefaultApplication(AbsoluteFilePath path)
		{
			var task = Application.MainThread.InvokeAsync(
				() =>
				{
					if (!NSWorkspace.SharedWorkspace.OpenFile(path.NativePath))
						throw new Exception("No default application found for " + path.NativePath);
					return new object();
				});

			task.Wait();
		}

		public FileStream Open(AbsoluteFilePath path, FileMode mode, FileAccess access, FileShare share)
		{
			return File.Open(path.NativePath, mode, access, share);
		}

		public Stream OpenRead(AbsoluteFilePath path)
		{
			return File.OpenRead(path.NativePath);
		}

		public Stream OpenWrite(AbsoluteFilePath path)
		{
			return File.OpenWrite(path.NativePath);
		}

		public IEnumerable<AbsoluteFilePath> GetFiles(AbsoluteDirectoryPath path)
		{
			return Directory.GetFiles(path.NativePath).Select(AbsoluteFilePath.Parse);
		}

		public IEnumerable<AbsoluteDirectoryPath> GetDirectories(AbsoluteDirectoryPath path)
		{
			return Directory.GetDirectories(path.NativePath).Select(AbsoluteDirectoryPath.Parse);
		}

		public IEnumerable<AbsoluteFilePath> GetFiles (AbsoluteDirectoryPath path, string searchPattern)
		{
			return Directory.GetFiles(path.NativePath, searchPattern).Select(AbsoluteFilePath.Parse);
		}

		public IEnumerable<AbsoluteDirectoryPath> GetDirectories (AbsoluteDirectoryPath path, string searchPattern)
		{
			return Directory.GetDirectories(path.NativePath, searchPattern).Select(AbsoluteDirectoryPath.Parse);
		}

		public IAbsolutePath ResolveAbsolutePath(string nativePath)
		{
			try
			{
				var absPath = Path.IsPathRooted(nativePath)
					? nativePath
					: Path.GetFullPath(nativePath);

				var isDirectory = Directory.Exists(nativePath);

				return isDirectory
					? (IAbsolutePath)AbsoluteDirectoryPath.Parse(absPath)
					: (IAbsolutePath)AbsoluteFilePath.Parse(absPath);

			}
			catch (ArgumentException e)
			{
				throw new InvalidPath(nativePath, e);
			}
			catch (NotSupportedException e)
			{
				throw new InvalidPath(nativePath, e);
			}
			catch (PathTooLongException e)
			{
				throw new InvalidPath(nativePath, e);
			}
		}

		public void Delete(IAbsolutePath path)
		{
			path.Do(
				(AbsoluteFilePath file) => File.Delete(file.NativePath),
				(AbsoluteDirectoryPath directory) => Directory.Delete(directory.NativePath, true));
		}

		public bool Exists(IAbsolutePath path)
		{
			return path.MatchWith(
				(AbsoluteFilePath file) => File.Exists(file.NativePath),
				(AbsoluteDirectoryPath directory) => Directory.Exists(directory.NativePath));
		}

		public void Copy(AbsoluteFilePath source, AbsoluteFilePath dest)
		{
			File.Copy(source.NativePath, dest.NativePath);
		}

		public void Move(AbsoluteFilePath source, AbsoluteFilePath destination)
		{
			if (!ShellHelper.TryPureMove(source, destination, File.Move))
			{
				File.Copy(source.NativePath, destination.NativePath);
				File.Delete(source.NativePath);
			}
		}

		public void Move(AbsoluteDirectoryPath source, AbsoluteDirectoryPath destination)
		{
			if (!ShellHelper.TryPureMove(source, destination, Directory.Move))
			{
				ShellHelper.DirectoryCopy(source.NativePath, destination.NativePath, copySubDirs: true);
				Directory.Delete(source.NativePath, recursive: true);
			}
		}

		public void Create(AbsoluteDirectoryPath directory)
		{
			Directory.CreateDirectory(directory.NativePath);
		}

		public Stream Create(AbsoluteFilePath file)
		{
			return File.Open(file.NativePath, FileMode.Create);
		}

		public Stream CreateNew(AbsoluteFilePath file)
		{
			return File.Open(file.NativePath, FileMode.CreateNew);
		}

		public IObservable<Unit> Watch(AbsoluteFilePath path)
		{
			return Observable.Create<Unit>(observer =>
			{
				if (!Exists(path.ContainingDirectory))
				{
					observer.OnError(new FileNotFoundException("Directory not found", path.ContainingDirectory.NativePath));
					return Disposable.Empty;
				}

				var fsw = new FileSystemWatcher(path.ContainingDirectory.NativePath, path.Name.ToString())
				{
					NotifyFilter = NotifyFilters.CreationTime
						| NotifyFilters.FileName
						| NotifyFilters.LastWrite
						| NotifyFilters.Size,
				};

				var garbage = Disposable.Combine(
					fsw,
					Observable.FromEventPattern<ErrorEventArgs>(fsw, "Error")
						.Subscribe(_ => observer.OnError(_.EventArgs.GetException())),
					Observable.FromEventPattern<FileSystemEventArgs>(fsw, "Created")
						.Subscribe(_ => observer.OnNext(Unit.Default)),
					Observable.FromEventPattern<FileSystemEventArgs>(fsw, "Changed")
						.Subscribe(_ => observer.OnNext(Unit.Default)),
					Observable.FromEventPattern<FileSystemEventArgs>(fsw, "Renamed")
						.Subscribe(_ => observer.OnNext(Unit.Default)),
					Observable.FromEventPattern<FileSystemEventArgs>(fsw, "Deleted")
						.Subscribe(_ => observer.OnNext(Unit.Default)));

				fsw.EnableRaisingEvents = true;

				return garbage;
			});
		}

		public void OpenFolder(AbsoluteDirectoryPath path)
		{
			Process.Start(path.NativePath);
		}

		public void ShowInFolder(AbsoluteFilePath path)
		{
			var urls = new [] { new NSUrl(new Uri(path.NativePath).AbsoluteUri) };
			var workspace = NSWorkspace.SharedWorkspace;
			workspace.BeginInvokeOnMainThread(() => workspace.ActivateFileViewer (urls));
		}

		public void OpenTerminal(AbsoluteDirectoryPath containingDirectory)
		{
			var t = new NSTask();
			t.LaunchPath = "/usr/bin/osascript";
			t.Arguments = new [] {"-e", "tell application \"Terminal\"\ndo script \"cd \\\""+containingDirectory.NativePath+"\\\"\"\nactivate\nend tell" };
			t.Launch();
		}

		public IObservable<FileSystemEventData> Watch(AbsoluteDirectoryPath path, Optional<string> filter = default(Optional<string>))
		{
			return Observable.Create<FileSystemEventData>(observer =>
			{
				if (!Exists(path))
				{
					observer.OnError(new FileNotFoundException("Directory not found", path.ContainingDirectory.NativePath));
					return Disposable.Empty;
				}

				var fsw = filter.Select(f => new FileSystemWatcher(path.NativePath, f)).Or(new FileSystemWatcher(path.NativePath));
				fsw.IncludeSubdirectories = false;
				fsw.NotifyFilter = NotifyFilters.CreationTime
					| NotifyFilters.FileName
					| NotifyFilters.LastWrite
					| NotifyFilters.Size;

				var garbage = Disposable.Combine(
					fsw,
					Observable.FromEventPattern<ErrorEventArgs>(fsw, "Error")
						.Subscribe(_ => observer.OnError(_.EventArgs.GetException())),
					Observable.FromEventPattern<FileSystemEventArgs>(fsw, "Changed")     
						.Subscribe(e => observer.OnNext(new FileSystemEventData(AbsoluteFilePath.Parse(e.EventArgs.FullPath), FileSystemEvent.Changed))),
					Observable.FromEventPattern<FileSystemEventArgs>(fsw, "Created")
						.Subscribe(e => observer.OnNext(new FileSystemEventData(AbsoluteFilePath.Parse(e.EventArgs.FullPath), FileSystemEvent.Created))),
					Observable.FromEventPattern<RenamedEventArgs>(fsw, "Renamed")
						.Subscribe(e =>
						{
							observer.OnNext(new FileSystemEventData(
								AbsoluteFilePath.Parse(e.EventArgs.FullPath), 
								FileSystemEvent.Renamed, 
								AbsoluteFilePath.Parse(e.EventArgs.OldFullPath)));
						}),
					Observable.FromEventPattern<FileSystemEventArgs>(fsw, "Deleted")
						.Subscribe(e => observer.OnNext(new FileSystemEventData(AbsoluteFilePath.Parse(e.EventArgs.FullPath), FileSystemEvent.Removed))));

				fsw.EnableRaisingEvents = true;

				return garbage;
			});
		}
	}
}
