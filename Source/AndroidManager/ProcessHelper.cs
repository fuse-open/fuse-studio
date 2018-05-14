using System;
using System.Diagnostics;
using System.Reactive;
using System.Threading;

namespace Outracks.AndroidManager
{
    public class ProcessHelper
    {
	    public static int StartProcess(ProcessStartInfo processStart, CancellationToken ct, Action<Process> usesProcess)
	    {		    
		    var p = Process.Start(processStart)
				.ToOptional()
				.OrThrow(new Exception("Failed to start " + processStart.FileName));

			ct.Register(p.Kill);

			if(!ct.IsCancellationRequested)
				usesProcess(p);

		    while (!p.HasExited)
		    {
			    Thread.Sleep(1);
		    }

		    return p.ExitCode;
	    }

		public static int StartProcessWithProgress(
			ProcessStartInfo processStart, 
			CancellationToken ct, 
			Action<Process> usesProcess,
			IProgress<InstallerEvent> progress)
		{
			processStart.RedirectStandardError = true;
			processStart.RedirectStandardOutput = true;
			processStart.UseShellExecute = false;

			return StartProcess(processStart, 
				ct,
			    process =>
			    {
					process.ConsumeError(Observer.Create((string line) => progress.Report(new InstallerMessage(line, InstallerMessageType.Error))));
					process.ConsumeOutput(Observer.Create((string line) => progress.Report(new InstallerMessage(line))));	

					usesProcess(process);
			    });
		}
    }
}