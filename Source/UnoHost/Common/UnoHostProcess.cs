using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Outracks.UnoHost
{
	using IO;
	using IPC;
	using Fusion;

	public class UnoHostProcess : IDisposable
	{
		public static IExternalApplication Application { get; set; }

		public static UnoHostProcess Spawn(AbsoluteFilePath assembly, IObservable<IBinaryMessage> messagesToUnoHost, AbsoluteFilePath userDataPath, IEnumerable<string> moreArgs)
		{
			if (Application == null) 
				throw new InvalidOperationException("UnoHostProcess.Application has not been initialized");

			var args = new UnoHostArgs
			{
				AssemblyPath = assembly,
				OutputPipe = PipeName.New(),
				InputPipe = PipeName.New(),
				UserDataPath = userDataPath,
			};

			var disp = args.InputPipe.BeginWritingMessages("UnoHost", ex => Console.WriteLine("Designer failed to write message to UnoHost: " + ex), messagesToUnoHost);
			
			return new UnoHostProcess
			{
				Messages = args.OutputPipe.ReadMessages("UnoHost"),
				Process = Observable.Start(() => Application.Start(args.Serialize().Concat(moreArgs))),
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