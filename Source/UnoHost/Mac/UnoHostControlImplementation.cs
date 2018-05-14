using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Outracks.IO;

namespace Outracks.Fusion.OSX
{
	using UnoHost;
	using UnoHost.OSX.FusionSupport;


	public class UnoHostControlFactory : IUnoHostControlFactory
	{
		public IUnoHostControl Create(
			AbsoluteFilePath assemblyPath,
			Command onFocused,
			Menu contextMenu,
			AbsoluteFilePath userDataPath,
			Action<IUnoHostControl> initialize,
			Action<OpenGlVersion> gotVersion,
			params string[] arguments)
		{
			var currentExeDir = typeof(Application).Assembly.GetCodeBaseFilePath().ContainingDirectory;
			var contentsDir = currentExeDir / ".." / ".." / "..";
			var unoHostBundle = contentsDir / "UnoHost.app";
			UnoHostProcess.Application = ExternalApplication.FromAppBundle(unoHostBundle);
 
			var unoHostExe = unoHostBundle / "Contents" / "MacOS" / new FileName("UnoHost");
			UnoHostProcess.Application = ExternalApplication.FromNativeExe(unoHostExe);

			var dispatcher = Fusion.Application.MainThread;

			var messagesToHost = new Subject<IBinaryMessage>();

			var unoHost = UnoHostProcess.Spawn(assemblyPath, messagesToHost, userDataPath, /*TODO*/new List<string>());

			var control = new UnoHostControlImplementation()
			{
				Disposables = Disposable.Combine(unoHost),
				Messages = unoHost.Messages,
				MessagesTo = messagesToHost,
				Process = unoHost.Process,
			};

			control.Control =
				Fusion.Control.Create(self =>
				{
					var view = UnoHostViewFactory.Create(unoHost.Messages, messagesToHost);

					self.BindNativeDefaults(view.View, dispatcher);

					view.Focus
						.Where(foc => foc == FocusState.Focused)
						.WithLatestFromBuffered(onFocused.Execute.ConnectWhile(self.IsRooted), (_, c) => c)
						.Subscribe(c => c());

					unoHost.Messages
						.SelectMany(m => m.TryParse(Ready.MessageType, Ready.ReadDataFrom))
						.Take(1)
						.Subscribe(_ => initialize(control));

					unoHost.Messages
						.SelectSome(OpenGlVersionMessage.TryParse)
						.Subscribe(gotVersion);

					unoHost.Messages.Connect();

					return view.View;
				}).SetContextMenu(contextMenu);

			return control;
		}
	}

	class UnoHostControlImplementation : IUnoHostControl
	{

		public IDisposable Disposables { get; set; }

		public void Dispose()
		{
			Disposables.Dispose(); 
		}

		public IConnectableObservable<IBinaryMessage> Messages { get; set; }

		public IObserver<IBinaryMessage> MessagesTo { get; set; }

		public IControl Control { get; set; }
		public IObservable<Process> Process { get; set; }
	}
}