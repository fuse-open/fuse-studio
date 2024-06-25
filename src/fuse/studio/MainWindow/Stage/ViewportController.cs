using System;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using Fuse.Preview;
using Outracks.Fuse.Studio;
using Outracks.Fusion;
using Outracks.IO;
using Outracks.Simulator.Bytecode;
using Outracks.Simulator.Protocol;

namespace Outracks.Fuse.Stage
{
	class ViewportController : IViewport
	{
		static Brush ViewportBackground => Theme.PanelBackground;

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
			IObserver<OpenGlVersion> glVersion,
			Action enterCode)
		{
			VirtualDevice = new BehaviorSubject<VirtualDevice>(initialDevice);
			_onClose = onClose;
			_onFocus = onFocus;

			var simulatorControl = assembly
				.Select(assemblyPath =>
					Observable.Start(() =>
						Observable.Using(() =>
							preview.LockBuild(assemblyPath.BuildDirectory),
							assemblyLock => IsActivated()
								? Observable.Using(() =>
									SpawnUnoHost(assemblyPath, port, fuse, menu(this), initialize, glVersion),
									unoHost =>
										Observable
											// never end stream, we don't want to dispose until we've gotten a new one
											.Never<Optional<IUnoHostControl>>()
											.StartWith(Optional.Some(unoHost)))
								: Observable
									// never spawn unohost without a valid license
									.Never<Optional<IUnoHostControl>>()))
					.Switch()
					.StartWith(Optional.None<IUnoHostControl>()))
				.Switch()
				.Replay(1);

			_subscription = simulatorControl.Connect();

			Control = IsActivated()
				? simulatorControl
					.StartWith(Optional.None<IUnoHostControl>())
					.Select(c => c.Select(cc => cc.Control).Or(() => CreateLoadingIndicator()))
					.Switch()
					.WithBackground(ViewportBackground)
				: CreateActivateIndicator(enterCode);

			var realControl = simulatorControl.NotNone().Replay(1).RefCount();

			_send = message => realControl.Take(1).Subscribe(unoHost => unoHost.MessagesTo.OnNext(message));
		}

		static IControl CreateActivateIndicator(Action enterCode)
		{
			return Layout.Layer(
				Shapes.Rectangle(fill: ViewportBackground),
				Layout.StackFromTop(
					LogoAndVersion.Logo()
						.WithSize(new Size<Points>(336 * 0.5, 173 * 0.5)),
					Label.Create(Strings.NagScreen_Text.Wrap(48),
								 font: Theme.DefaultFont,
								 color: Theme.DefaultText)
						.WithPadding(top: new Points(32), bottom: new Points(32))
						.Center(),
					Buttons.DefaultButtonPrimary(
								text: Texts.NagScreen_Button_SignIn,
								cmd: Command.Enabled(() => Process.Start(WebLinks.SignIn)))
						.WithWidth(128)
						.Center(),
					Buttons.DefaultButton(
								text: Texts.NagScreen_Button_EnterCode,
								cmd: Command.Enabled(enterCode))
						.WithPadding(top: new Points(8))
						.WithWidth(128)
						.Center())
				.Center());
		}

		static IControl CreateLoadingIndicator()
		{
			return Layout.Layer(
				Shapes.Rectangle(fill: ViewportBackground),
				Label.Create("Building project...", font: Theme.DefaultFont, color: Theme.DefaultText).Center());
		}

		IUnoHostControl SpawnUnoHost(AssemblyBuilt assemblyPath, int port, IFuse fuse, Menu menu, Action<IUnoHostControl> initialize, IObserver<OpenGlVersion> glVersion)
		{
			var unoHost = UnoHostControl.Create(
				assemblyPath.Assembly,
				Command.Enabled(Focus),
				menu,
				fuse.UserDataDir / new FileName("studio-settings.json"),
				fuse.UnoHost,
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

		static bool IsActivated()
		{
			try
			{
				return (bool)typeof(FuseImpl)
					.GetField("_IsLicenseValid", BindingFlags.Static | BindingFlags.NonPublic)
					.GetValue(null);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				return false;
			}
		}
	}
}
