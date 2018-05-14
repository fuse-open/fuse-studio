using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Outracks.IO;

namespace Outracks.AndroidManager
{
	public static class DownloadHelper
	{
		public static void DownloadFileWithProgress(Uri downloadUri, AbsoluteFilePath destination, CancellationToken ct, IProgress<InstallerEvent> progress)
		{
			using (var client = new WebClient())
			{
				double curDownloadPercent = 0;
				
				var taskRes = new TaskCompletionSource<object>();
				client.DownloadProgressChanged += (sender, args) =>
				{
					var downloadPercent = (double) args.BytesReceived / (double) args.TotalBytesToReceive;
					if (curDownloadPercent + 0.01 < downloadPercent)
					{
						curDownloadPercent = downloadPercent;
						progress.Report(
							new InstallerMessage(
								(downloadPercent.ToString("P") + " Downloaded")));	
					}					
				};

				client.DownloadFileCompleted += (sender, args) =>
				{
					if (args.Error != null)
						taskRes.SetException(args.Error.InnerException ?? args.Error);
					else
						taskRes.SetResult(new object());
				};

				// We need to do Task.Run on OSX because a bug in mono implementation.
				Task.Run(() => client.DownloadFileAsync(downloadUri, destination.NativePath));

				try
				{
					taskRes.Task.Wait(ct);
				}
				catch (AggregateException e)
				{
					throw e.GetBaseException();
				}
			}
		}
	}
}