using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Outracks.IO;

namespace Outracks.Fuse.CodeAssistanceService
{
	
	class ChangedFile
	{
		public string Source { get; private set; }
		public AbsoluteFilePath FilePath { get; private set; }
		public DateTime Time { get; private set; }

		public ChangedFile(string source, AbsoluteFilePath filePath, DateTime time)
		{
			Source = source;
			FilePath = filePath;
			Time = time;
		}

		public void UpdateTime(DateTime time)
		{
			Time = time;
		}
	}

	public class EditorManager : UnoDevelop.CodeNinja.IEditorManager
	{
		public DateTime GetLastWriteTime(AbsoluteFilePath filePath)
		{
			var fileChanged = _filesChanged.FirstOrDefault(f => f.FilePath == filePath);
			var lastWriteTime = File.GetLastWriteTime(filePath.NativePath);
			if (fileChanged != null)
			{				
				if (lastWriteTime < fileChanged.Time)
				{
					return fileChanged.Time;
				}

				_filesChanged.Remove(fileChanged);
			}

			return lastWriteTime;
		}

		public bool IsOpen(AbsoluteFilePath filePath)
		{
			return _filesChanged.Select(f => f.FilePath).Any(f => f == filePath);
		}

		public Stream OpenFileStream(AbsoluteFilePath filePath, FileAccess access = FileAccess.ReadWrite)
		{
			var fileChanged = _filesChanged.FirstOrDefault(f => f.FilePath == filePath);
			if (fileChanged != null)
			{
				var lastWriteTime = File.GetLastWriteTime(filePath.NativePath);
				if (lastWriteTime < fileChanged.Time)
				{
					return new MemoryStream(Encoding.UTF8.GetBytes(fileChanged.Source));
				}
				
				_filesChanged.Remove(fileChanged);
			}

			return File.Open(filePath.NativePath, FileMode.OpenOrCreate, access, access.HasFlag(FileAccess.Write) ? FileShare.Read : FileShare.ReadWrite);
		}


		readonly List<ChangedFile> _filesChanged = new List<ChangedFile>(); 

		public void AddChangedFile(string source, AbsoluteFilePath filePath)
		{
			var found = _filesChanged.FirstOrDefault(f => f.FilePath == filePath);
			if (found != null)
			{
				found.UpdateTime(DateTime.Now);
				return;
			}

			_filesChanged.Add(new ChangedFile(source, filePath, DateTime.Now));						
		}

		public string ReadAllText(AbsoluteFilePath filePath)
		{
			return OpenFileStream(filePath, FileAccess.Read).ReadToEnd();
		}
	}
}