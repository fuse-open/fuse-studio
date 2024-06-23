using System;
using System.Diagnostics;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace Outracks
{
	public static class ProcessExtension
	{
		public static void ReadCharacters(this StreamReader stream, IObserver<string> lines)
		{
			new Thread(() =>
			{
				PushCharacterChunksToObserver(stream, lines);
				lines.OnCompleted();
			})
			{
				Name = "Consume characters from stream",
				IsBackground = true,
			}.Start();
		}

		static void PushCharacterChunksToObserver(TextReader stream, IObserver<string> lines)
		{
			var characters = new Subject<char>();

			characters
				.Buffer(timeSpan: TimeSpan.FromMilliseconds(50))
				.Where(l => !l.IsEmpty())
				.Subscribe(
					msg =>
					{
						lines.OnNext(string.Concat(msg));
					});

			int character;
			while ((character = stream.Read()) != -1)
			{
				characters.OnNext((char) character);
			}
			characters.OnCompleted();
		}

		public static Task ConsumeOutput(this Process process, IObserver<string> lines)
		{
			return ConsumeReader(process.StandardOutput, lines);
		}

		public static Task ConsumeError(this Process process, IObserver<string> lines)
		{
			return ConsumeReader(process.StandardError, lines);
		}

		public static Task ConsumeOutAndErr(this Process process,
			IObserver<string> outLines,
			IObserver<string> errLines)
		{
			return Task.Run(
				() =>
				{
					var waitOut = process.ConsumeOutput(outLines);
					var waitErr = process.ConsumeError(errLines);

					waitOut.Wait();
					waitErr.Wait();
				});
		}

		static Task ConsumeReader(TextReader reader, IObserver<string> lines)
		{
			return Task.Run(
				() =>
				{
					PushCharacterChunksToObserver(reader, lines);
					lines.OnCompleted();
				});
		}
	}
}
