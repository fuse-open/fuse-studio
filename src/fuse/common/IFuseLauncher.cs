using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace Outracks.Fuse
{
	public interface IFuseLauncher
	{
		Process StartFuse(string command, params string[] commandArgs);
		Process StartFuse(string command, string[] commandArgs, bool redirect, bool hide = true);
		void RestartFuse();
	}

	// TODO: merge below functions with main IFuseLauncher interface.
	// The code below was copied from dashboard code

	class FuseProcessException : Exception
	{
		public FuseProcessException(string message)
			: base(message)
		{
		}
	}

	public class FuseExitCode
	{
		public readonly int Code;

		public FuseExitCode(int code)
		{
			Code = code;
		}

		public bool Success { get { return Code == 0; } }

		public bool Failed { get { return Code != 0; } }

		public bool FatalCrash { get { return Code == 255; } }
	}

	public static class FuseLauncher
	{
		public static Task<FuseExitCode> RunFuse(
			this IFuseLauncher fuseExe,
			string command, string[] arguments,
			IObserver<string> stdout,
			IObserver<string> stderr = null,
			bool hide = true)
		{
			return Task.Run(() =>
			{
				stderr = stderr ?? stdout;

				using (var p = fuseExe.StartFuse(command, arguments, true, hide))
				{
					p.StandardOutput.ReadCharacters(stdout);
					p.StandardError.ReadCharacters(stderr);

					p.WaitForExit();
					return new FuseExitCode(p.ExitCode);
				}
			});
		}

		// Throws exceptions when errors.
		// TODO: handling errors from this function is hard due to some errors in Task and some from the call
		public static Task<Unit> RunFuseWithErrors(
			this IFuseLauncher fuseExe,
			string command,
			string[] arguments,
			IObserver<string> outputLines,
			bool hide = true)
		{
			var p = fuseExe.StartFuse(command, arguments, true, hide);

			var errorQueue = new ConcurrentQueue<FuseProcessException>();
			var errorLines = new Subject<string>();

			errorLines.Subscribe(line => errorQueue.Enqueue(new FuseProcessException(line.Replace("fuse:", "").TrimStart())));

			p.ConsumeOutput(outputLines);
			p.ConsumeError(errorLines);

			return Task.Run(() =>
			{
				p.WaitForExit();
				var exitCode = new FuseExitCode(p.ExitCode);
				if (exitCode.Failed)
				{
					throw new AggregateException(errorQueue.OfType<Exception>().ToArray());
				}

				return Unit.Default;
			});
		}

	}
}
