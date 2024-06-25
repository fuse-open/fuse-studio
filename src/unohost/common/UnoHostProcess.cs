using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Outracks.IO;
using Outracks.IPC;

namespace Outracks.UnoHost
{
	public class UnoHostProcess : IDisposable
	{
		public static UnoHostProcess Spawn(AbsoluteFilePath assembly, IObservable<IBinaryMessage> messagesToUnoHost, AbsoluteFilePath userDataPath, IExternalApplication application, IEnumerable<string> moreArgs)
		{
			var args = new UnoHostArgs
			{
				AssemblyPath = assembly,
				OutputPipe = PipeName.New(),
				InputPipe = PipeName.New(),
				UserDataPath = userDataPath,
			};

			var disp = args.InputPipe.BeginWritingMessages("UnoHost", ex => Console.WriteLine("fuse X failed to write message to UnoHost: " + ex), messagesToUnoHost);

			return new UnoHostProcess
			{
				Messages = args.OutputPipe.ReadMessages("UnoHost"),
				Process = Observable.Start(() => application.Start(args.Serialize().Concat(moreArgs))),
				Disposables = Disposable.Combine(disp)
			};
		}

		public IConnectableObservable<IBinaryMessage> Messages;
		IDisposable Disposables { get; set; }

		public IObservable<Process> Process
		{
			get; private set;
		}

		public IObservable<T> Receieve<T>(Func<IBinaryMessage, Optional<T>> tryParse)
		{
			return Messages.SelectSome(tryParse);
		}

		public void Dispose()
		{
			Process.Take(1).Do(p => p.CloseMainWindow()).Subscribe();
			Disposables.Dispose();
		}
	}

}
