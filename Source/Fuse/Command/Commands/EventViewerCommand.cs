using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Threading;
using Mono.Options;
using Outracks.Fuse.Analytics;
using Outracks.Fuse.Protocol;
using Outracks.Fuse.Protocol.Messages;

namespace Outracks.Fuse
{
	public class EventViewerCommand : CliCommand
	{
		public static CliCommand CreateEventViewerCommand()
		{
			return new EventViewerCommand(ColoredTextWriter.Out, FuseApi.Initialize("Fuse", new List<string>()));
		}

		readonly IFuse _fuse;
		readonly HelpArguments _helpArguments;
		readonly ColoredTextWriter _textWriter;
		readonly OptionSet _optionSet;
		readonly IReport _report;
		bool _useColors = false;
		bool _extraLineSpace = false;
		string _eventFilter = ".*";

		public EventViewerCommand(ColoredTextWriter textWriter, IFuse fuse) : base("event-viewer", "Dump all events", true)
		{
			_textWriter = textWriter;
			_fuse = fuse;
			_optionSet = CreateOptions();
			_report = _fuse.Report;
			_helpArguments = new HelpArguments(
				new HelpHeader("fuse " + Name, Description),
				new HelpSynopsis("fuse " + Name + " [<Options>]"),
				new HelpDetailedDescription("Dumps all Fuse events to the console."),
				new HelpOptions( new[]
					{
						_optionSet.ToTable(),
					}));

		}

		OptionSet CreateOptions()
		{
			return new OptionSet()
			{
				{ "c|color", "Distinguish different types of events with different colors.", c => _useColors = true },
				{ "s|space", "Print an extra line break between each line.", s => _extraLineSpace = true },
				{ "f|filter=", "Filter events using this regex.", f => _eventFilter = f}
			};
		}

		public override void Help()
		{
			_textWriter.WriteHelp(_helpArguments);
		}

		/// <exception cref="ExitWithError" />
		public override void Run(string[] args, CancellationToken ct)
		{
			_optionSet.Parse(args);

			var localSocketClient = new LocalSocketClient(new TcpClient("127.0.0.1", 12122));
			localSocketClient.StartRead();
			var serializer = new Serializer();
			var client = new Client(localSocketClient, serializer);

			client.Request(new HelloRequest()
			{
				Identifier = "Event Viewer",
			}).Wait();

			client.Request(new SubscribeRequest(_eventFilter, replay: true, subscriptionId: 0));

			var messages = localSocketClient.IncomingMessages;
			messages.TryDeserialize(serializer, _report).OfType<IEventMessage<UnresolvedMessagePayload>>().Subscribe(WriteEvent);

			var run = true;
			localSocketClient.Disconnected.Subscribe(c => run = false);
			//TODO handle ctrl+c

			while (run)
			{
				Thread.Sleep(500);
			}

		}

		void WriteEvent(IEventMessage<UnresolvedMessagePayload> m)
		{
			var output = m.Name + ": " + m.Data;
			if (_useColors)
				using (_textWriter.PushColor(ColorFor(m)))
					_textWriter.WriteLine(output);
			else
				_textWriter.WriteLine(output);
			if (_extraLineSpace)
				_textWriter.WriteLine();
		}

		ConsoleColor ColorFor(IEventMessage<UnresolvedMessagePayload> m)
		{
			const ConsoleColor defaultColor = ConsoleColor.Gray;

			var name = m.Name;

			if (name.StartsWith("Fuse.Build"))
				return ConsoleColor.Cyan;

			if (IsOfPayloadType<SelectionChanged>(name))
				return ConsoleColor.White;
			if (IsOfPayloadType<LogEvent>(name) || IsOfPayloadType<ExceptionEvent>(name))
				return ConsoleColor.Yellow;
			return defaultColor;
		}

		static bool IsOfPayloadType<T>(string name)
		{
			return name == typeof (T).GetPayloadTypeName();
		}
	}
}
