using System;
using System.Diagnostics;
using System.Threading;
using Outracks.IO;

namespace Outracks.Fuse.SystemTest {
	public class FuseRunner
	{
		readonly AbsoluteFilePath _fuse;
		public FuseRunner(AbsoluteFilePath fuse)
		{
			_fuse = fuse;
		}

		public Process Preview(AbsoluteFilePath unoProj, Optional<Action<string>> stdOutReceived)
		{
			return Run("preview --print-unoconfig --verbose " + unoProj.NativePath, stdOutReceived);
		}

		public Process KillAll()
		{
			return Run("kill-all", Optional.None());
		}

		public Process Run(string arguments, Optional<Action<string>> stdOutReceived)
		{
			Console.WriteLine("Starting '" + _fuse.NativePath + "' with arguments '" + arguments + "'");
			var p = Process.Start(new ProcessStartInfo(_fuse.NativePath)
			{
				Arguments = arguments,
				UseShellExecute = false,
				RedirectStandardError = true,
				RedirectStandardOutput = true
			});

			if (p == null)
			{
				throw new TestFailure("Failed to start process");
			}

			p.OutputDataReceived += (sender, args) =>
			{
				Console.WriteLine("  >" + args.Data);
				stdOutReceived.Do(a => {
					if (args.Data!= null)
						a(args.Data);
				});
			};
			p.ErrorDataReceived += (sender, args) =>
			{
				Console.Error.WriteLine("  >" + args.Data);
			};
			p.BeginErrorReadLine();
			p.BeginOutputReadLine();
			return p;
		}

		public void KillOrThrow()
		{
			try
			{
				Console.WriteLine("Killing Fuse");
				KillAll().WaitOrThrow(TimeSpan.FromSeconds(30));
				// We sometimes see sporadic failures on TC connecting to the daemon. My theory
				// is that the  previous kill-all didn't have time to properly shut everything down,
				// close pipes and file handles etc., before the next test starts. Let's give it
				// some more time to do that.
				var wait = TimeSpan.FromSeconds(3);
				Console.WriteLine("Fuse killed. Waiting for " + wait + " just to be on the safe side.");
				Thread.Sleep(wait);
			}
			catch (Exception e)
			{
				throw new TestFailure("Failed to kill fuse! " + e.Message);
			}
		}
	}
}