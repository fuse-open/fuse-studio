using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using AppKit;
using Outracks.Extensions;
using Outracks.Fuse.Analytics;
using Outracks.Fusion;
using Outracks.IO;
using Outracks.UnoHost.Mac.Protocol;
using Outracks.UnoHost.Mac.UnoView;
using Outracks.UnoHost.Mac.UnoView.RenderTargets;
using Application = Uno.Application;

namespace Outracks.UnoHost.Mac
{
	public class Program
	{
		static Program()
		{
			Thread.CurrentThread.SetInvariantCulture();
#if DEBUG
			ConsoleExtensions.RedirectStreamsToLogFiles("unohost");
#endif
		}

		[STAThread]
		public static void Main(string[] argsArray)
		{
			var shell = new Shell();
			var systemId = SystemGuidLoader.LoadOrCreateOrEmpty();
			var sessionId = Guid.NewGuid();
			var log = ReportFactory.GetReporter(systemId, sessionId, "UnoHost");
			var descriptiveString = "UnoHost (" + argsArray.Select(Uno.Extensions.QuoteSpace).Join(" ") + ")";
			try
			{
				AppDomain.CurrentDomain.ReportUnhandledExceptions(log);

				var dispatcher = new Dispatcher(Thread.CurrentThread);

				log.Info("Starting " + descriptiveString, ReportTo.LogAndUser);
				var argsList = argsArray.ToList();

				var args = UnoHostArgs.RemoveFrom(argsList, shell);

				NSApplication.Init();

				NSApplication.SharedApplication.Delegate = new AppDelegate();
				AppDelegate.ThrowOnTerminate = false;

				Action exit = () =>
				{
					log.Info("Stopping " + descriptiveString);
					NSApplication.SharedApplication.Terminate(NSApplication.SharedApplication);
				};
				Console.CancelKeyPress += (sender, e) => exit();

				NSApplication.CheckForIllegalCrossThreadCalls = false;

				// Load metadata
				var unoHostProject = UnoHostProject.Load(args.MetadataPath, shell);

				Action<Exception> onLostConnection = exception =>
				{
					if (exception == null)
					{
						log.Info("CommunicationProtocol closed");
					}
					else
					{
						log.Exception("CommunicationProtocol failed", exception);
					}
					exit();
				};

				var renderTarget = new IOSurfaceRenderTarget();
				var glView = new UnoView.UnoView(dispatcher, log, renderTarget)
				{
					WantsBestResolutionOpenGLSurface = true
				};
				glView.PrepareOpenGL(); // We have to call this method manually because the view isn't bound to a window

				var openGlVersion = new Subject<OpenGlVersion>();

				var messagesTo = new Subject<IBinaryMessage>();
				var output = Observable.Merge(
					messagesTo,
					openGlVersion.Select(OpenGlVersionMessage.Compose),
					renderTarget.SurfaceRendered.Select(NewSurfaceMessage.Compose));

				if (!args.IsDebug)
					args.OutputPipe.BeginWritingMessages(
						"Designer",
						ex => Console.WriteLine("UnoHost failed to write message to designer: " + ex),
						output.ObserveOn(new QueuedDispatcher()));

				try
				{
					glView.Initialize(unoHostProject, openGlVersion);
				}
				catch (Exception e)
				{
					Console.Error.WriteLine("Initialize: " + e);
					log.Exception("Initialize failed", e);
				}

				var messagesFrom = args.InputPipe
					.ReadMessages("Designer")
					.RefCount()
					.ObserveOn(dispatcher)
					.Publish();

				messagesFrom.SelectSome(CocoaEventMessage.TryParse)
					.Subscribe(e => EventProcesser.SendEvent(glView, e), onLostConnection, () => onLostConnection(null));

				// Fuselibs fails during construction if we don't get this, and we can't wait for it because we are dispatching responses on our queue
				glView.Resize(new SizeData(Size.Create<Points>(0,0), 1.0));
				messagesFrom.SelectSome(ResizeMessage.TryParse)
					.Subscribe(glView.Resize, onLostConnection, () => onLostConnection(null));

				var size = glView.Size.Transpose();


				// Run the uno entrypoints, this initializes Uno.Application.Current

				try
				{
					unoHostProject.ExecuteStartupCode();
				}
				catch (Exception e)
				{
					Console.Error.WriteLine("ExecuteStartupCode: " + e);
					log.Exception("ExecuteStartupCode failed", e);
				}

				var app = Application.Current as dynamic;


				// Init plugins

				FusionImplementation.Initialize(dispatcher, args.UserDataPath, app.Reflection);
				var overlay = PluginManager.Initialize(messagesFrom, messagesTo, dispatcher, glView.PerFrame, size);
				app.ResetEverything(true, overlay);


				// Ready to go

				messagesFrom.Connect();
				messagesTo.OnNext(new Ready());

				NSApplication.SharedApplication.Run();
			}
			catch (Exception e)
			{
				log.Exception("Exception in " + descriptiveString, e);
			}
		}
	}
}
