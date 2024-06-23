using System.Collections.Generic;
using System.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using Outracks.Fuse.Auth;
using Outracks.Fuse.Protocol;

namespace Outracks.Fuse
{
	public class UriCommand : CliCommand
	{
		public UriCommand() : base("uri", "Handles fuse-x:// links")
		{
		}

		public override void Help()
		{
		}

		public override void Run(string[] args, CancellationToken ct)
		{
			var fuse = FuseApi.Initialize("fuse", new List<string>());
			var task = fuse.ConnectOrSpawnAsync("fuse");
			var daemon = task.ToObservable().Switch();
			new UriHandler(fuse, daemon).OnUri(args.FirstOrDefault());
			task.Wait(ct);
		}
	}
}
