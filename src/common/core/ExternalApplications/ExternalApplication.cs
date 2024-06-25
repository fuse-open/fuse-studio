using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Outracks.IO
{
	public interface IExternalApplication
	{
		Process Start(Optional<ProcessStartInfo> startInfo = default(Optional<ProcessStartInfo>));

		Process Open(IAbsolutePath fileName, Optional<ProcessStartInfo> startInfo = default(Optional<ProcessStartInfo>));

		string Name { get; }

		bool Exists { get; }
	}

	public static class ExternalApplication
	{
		public static Process Start(this IExternalApplication app, IEnumerable<string> args)
		{
			return app.Start(new ProcessStartInfo { Arguments = args.Select(Uno.Extensions.QuoteSpace).Join(" ") });
		}

		public static Process StartWithoutShellExecute(this IExternalApplication app, IEnumerable<string> args)
		{
			return app.Start(new ProcessStartInfo
			{
				Arguments = args.Select(Uno.Extensions.QuoteSpace).Join(" "),
				UseShellExecute = false,
				RedirectStandardInput = true,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				CreateNoWindow = true
			});
		}

		public static IExternalApplication FromNativeExe(AbsoluteFilePath path)
		{
			return new NativeExe(path);
		}

		[Obsolete("Starting Mono does not work well in Xamarin.Mac apps.")]
		public static IExternalApplication FromMonoExe(AbsoluteFilePath path, Optional<AbsoluteFilePath> mono)
		{
			return new MonoExe(path, mono);
		}

		public static IExternalApplication FromAppBundle(IAbsolutePath path)
		{
			return new AppBundle(path);
		}
	}
}
