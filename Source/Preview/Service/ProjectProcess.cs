using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using Outracks;
using Outracks.Fuse.Analytics;
using Outracks.IO;
using Outracks.Simulator.Protocol;

namespace Fuse.Preview
{
	public static class ProjectProcess 
	{
		static readonly IPlatform Platform = PlatformFactory.Create();

		static readonly Assembly EntryAssembly = typeof (Program).Assembly;
		const string MagicArgument = "start";

		public static IObservable<SimulatorHost> SpawnAsync()
		{
			return Observable
				.Start(() => Spawn())
				.Catch((Exception e) =>
				{
					Console.WriteLine(e);
					return Observable.Never<SimulatorHost>();
				});
		}

		public static SimulatorHost Spawn()
		{
			var previewHost = Platform.StartProcess(EntryAssembly, MagicArgument, Guid.NewGuid().ToString());

			var inputStream = previewHost.OpenStream("input");
			var outputStream = previewHost.OpenStream("output");

			// TODO: for the simulator to shut down properly i think this stream needs to be ended
			var input = new Subject<IBinaryMessage>();
			var output = outputStream.ReadMessages("Simulator").Publish();
			var subscription = inputStream.BeginWritingMessages("Simulator", ex => Console.WriteLine("Designer failed to write message to simulator: " + ex), input);

			return new SimulatorHost(input.OnNext, output);
		}

		public static void Run(string[] args)
		{
			if (args.FirstOrDefault() != MagicArgument)
				return;

			if (args.Length < 2)
			{
				Console.Error.WriteLine("Expected second argument to be the unique identifier for the pipe.");
				return;
			}

			var systemId = SystemGuidLoader.LoadOrCreateOrEmpty();
			var sessionId = Guid.NewGuid();
			var reporter = ReportFactory.GetReporter(systemId, sessionId, "SimulatorHost");
			AppDomain.CurrentDomain.ReportUnhandledExceptions(reporter);

			var inputPipe = Platform.CreateStream("input");
			var outputPipe = Platform.CreateStream("output");

			var input = ReadInput(inputPipe);

			var output = Run(input, new Version());

			using (output.WriteOutput(outputPipe))
			using (input.Connect())
			{
				input.LastAsync().Wait();
			}
		}

		static IConnectableObservable<IBinaryMessage> ReadInput(Stream inputPipe)
		{
			return inputPipe.ReadMessages("Designer").Publish();
		}

		static IDisposable WriteOutput(this IObservable<IBinaryMessage> messages, Stream stream)
		{
			var writer = new BinaryWriter(stream);

			return Disposable.Combine(
				stream, writer,
				messages.Subscribe(message => message.WriteTo(writer)));
		}

		public static IObservable<IBinaryMessage> Run(IObservable<IBinaryMessage> input, Version version)
		{
			var builder = new Builder(new UnoBuild(version));
			var reifier = new Reifier(builder);
			var updater = new Updater(reifier);

			return Observable.Merge(
				builder.Build(input.TryParse(BuildProject.MessageType, BuildProject.ReadDataFrom)),
				reifier.Reify(input.TryParse(GenerateBytecode.MessageType, GenerateBytecode.ReadDataFrom)),
				updater.Update(input.TryParse(UpdateAttribute.MessageType, UpdateAttribute.ReadDataFrom)));
		}
	}
}
