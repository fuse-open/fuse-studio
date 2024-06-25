using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Threading.Tasks;
using Outracks.Fuse.Auth;
using Outracks.Fuse.Protocol;

namespace Outracks.Fuse
{
	public static class UriTool
	{
		[STAThread]
		public static void Main(string[] args)
		{
			var fuse = FuseApi.Initialize("fuse-uri", new List<string>());
			var task = fuse.ConnectOrSpawnAsync("fuse-uri");
			var daemon = task.ToObservable().Switch();
			new UriHandler(fuse, daemon).OnUri(args.FirstOrDefault());
			task.Wait();
		}
	}
}
