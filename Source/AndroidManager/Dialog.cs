using Outracks.IO;

namespace Outracks.AndroidManager
{
	public interface IDialogResult
	{
	}

	public interface IDialogArguments<out TResult> where TResult : IDialogResult
	{		
	}

	public interface IDialog
	{
		TResult Start<TResult>(IDialogArguments<TResult> dialogArguments)
			where TResult : IDialogResult;
	}

	public class DialogYesNoResult : IDialogResult
	{
		readonly bool _wasYes;

		public bool Yes { get { return _wasYes; } }
		public bool No { get { return !_wasYes; } }

		public DialogYesNoResult(bool wasYes)
		{
			_wasYes = wasYes;
		}
	}

	public class DialogPathResult : IDialogResult
	{
		readonly string _rawPath;
		public readonly Optional<IAbsolutePath> Path;

		public Optional<AbsoluteDirectoryPath> DirectoryPath
		{
			get { return Path.Where(p => p is AbsoluteDirectoryPath).Select(p => (AbsoluteDirectoryPath)p); }
		}

		public Optional<AbsoluteFilePath> FilePath
		{
			get { return Path.Where(p => p is AbsoluteFilePath).Select(p => (AbsoluteFilePath)p); }
		}

		public bool Empty
		{
			get { return string.IsNullOrWhiteSpace(_rawPath); }
		}

		public DialogPathResult(string rawPath)
		{
			_rawPath = rawPath;
			Path = AbsoluteDirectoryPath.TryParse(rawPath).Select(p => (IAbsolutePath)p)
					.Or(AbsoluteFilePath.TryParse(rawPath).Select(p => (IAbsolutePath)p));
		}
	}

	public class DialogQuestion<T> : IDialogArguments<T>
		where T : IDialogResult
	{
		public readonly string Question;

		public DialogQuestion(string question)
		{
			Question = question;
		}
	}
}