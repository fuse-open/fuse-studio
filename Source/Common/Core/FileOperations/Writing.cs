using System.IO;
using System.Reactive;

namespace Outracks.IO
{
	public static class Writing
	{
		public static void WriteNewText(this IFileSystem fileSystem, AbsoluteFilePath path, string text)
		{
			using (var stream = fileSystem.CreateNew(path))
			using (var writer = new StreamWriter(stream))
				writer.Write(text);
		}

		public static void ReplaceText(this IFileSystem fileSystem, AbsoluteFilePath path, string text)
		{
			using (var stream = fileSystem.Create(path))
			using (var writer = new StreamWriter(stream))
				writer.Write(text);
		}

		public static void ReplaceText(this IFileSystem fileSystem, AbsoluteFilePath path, string text, int attempts)
		{
			RetryLoop.Try(attempts,
				() =>
				{
					fileSystem.ReplaceText(path, text);
					return Unit.Default;
				});
		}
	}
}