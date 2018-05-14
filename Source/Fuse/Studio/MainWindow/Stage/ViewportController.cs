using System;
using System.Globalization;
using System.Net;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Fuse.Preview;
using ICSharpCode.NRefactory.Utils;
using Outracks.UnoHost;

namespace Outracks.Fuse.Stage
{
	using IO;
	using Fusion;
	using Simulator.Protocol;
	using Simulator.Bytecode;

	class ViewportController : IViewport
	{
		readonly Action<IBinaryMessage> _send;

		readonly Action<ViewportController> _onClose;
		readonly Action<ViewportController> _onFocus;

		readonly IDisposable _subscription;
		
		public ViewportController(
			VirtualDevice initialDevice,
			Action<ViewportController> onFocus,
			Action<ViewportController> onClose,
			Func<ViewportController, Menu> menu,
			ProjectPreview preview,
			IObservable<AssemblyBuilt> assembly,
			int port,
			IFuse fuse,
			Action<IUnoHostControl> initialize,
			IObserver<OpenGlVersion> glVersion)
		{
			VirtualDevice = new BehaviorSubject<VirtualDevice>(initialDevice);
			_onClose = onClose;
			_onFocus = onFocus;
			
			var simulatorControl = assembly
				.Select(assemblyPath => 
					Observable.Start(() => 
						Observable.Using(() => 
							preview.LockBuild(assemblyPath.BuildDirectory), 
							assemblyLock => 
								Observable.Using(() => 
									SpawnUnoHost(assemblyPath, port, fuse, menu(this), initialize, glVersion),
									unoHost => 
										Observable
											// never end stream, we don't want to dispose until we've gotten a new one
											.Never<Optional<IUnoHostControl>>()
											.StartWith(Optional.Some(unoHost)))))
					.Switch()
					.StartWith(Optional.None<IUnoHostControl>()))
				.Switch()
				.Replay(1);

			_subscription = simulatorControl.Connect();
			
			Control = simulatorControl
				.StartWith(Optional.None<IUnoHostControl>())
				.Select(c => c.Select(cc => cc.Control).Or(() => CreateLoadingIndicator()))
				.Switch()
				.WithBackground(Color.White);

			var realControl = simulatorControl.NotNone().Replay(1).RefCount();

			_send = message => realControl.Take(1).Subscribe(unoHost => unoHost.MessagesTo.OnNext(message));
		}		
		
		static IControl CreateLoadingIndicator()
		{
			return Layout.Layer(
				Shapes.Rectangle(fill: Theme.PanelBackground),
				Label.Create("Building project...", font: Theme.DefaultFont, color: Theme.DefaultText).Center());
		}

		IUnoHostControl SpawnUnoHost(AssemblyBuilt assemblyPath, int port, IFuse fuse, Menu menu, Action<IUnoHostControl> initialize, IObserver<OpenGlVersion> glVersion)
		{
			var unoHost = UnoHostControl.Create(
				assemblyPath.Assembly,
				Command.Enabled(Focus),
				menu,
				fuse.UserDataDir / new FileName("designer-config.json"),
				initialize,
				m => 
				{
					glVersion.OnNext(m);
					fuse.Report.Info(m.GlVersion, ReportTo.LogAndUser);
				},

				// args
				"--host",
				IPAddress.Loopback.ToString(),
				port.ToString(CultureInfo.InvariantCulture));

			return unoHost;
		}

		public IControl Control
		{
			get; private set;
		}

		public BehaviorSubject<VirtualDevice> VirtualDevice
		{
			get; private set;
		}

		public void Focus()
		{
			_onFocus(this);
		}

		public void Close()
		{
			_onClose(this);
			_subscription.Dispose();
		}

		public void Execute(Statement bytecode)
		{
			_send(new[] { bytecode }.MakeExecute());
		}

	}
}