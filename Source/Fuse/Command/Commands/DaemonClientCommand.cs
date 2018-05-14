using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mono.Options;

namespace Outracks.Fuse
{
	using Analytics;
	using Protocol;

	public class DaemonClientCommand : CliCommand
	{

		public static CliCommand Create()
		{
			var fuse = FuseApi.Initialize("daemon-client", new List<string>());
			return new DaemonClientCommand(ColoredTextWriter.Out, fuse);
		}

		readonly ColoredTextWriter _textWriter;
		readonly IFuseLauncher _fuseLauncher;
		readonly OptionSet _optionSet;
		bool _noSpawnMode = false;

		readonly HelpSynopsis _helpSynopsis = new HelpSynopsis("fuse daemon-client <Identifier>");

		public DaemonClientCommand(ColoredTextWriter textWriter, IFuse fuse) : base("daemon-client", "Create a connection to a daemon.")
		{
			_textWriter = textWriter;
			_fuseLauncher = fuse;
			_optionSet = CreateOptions();
		}

		public override void Help()
		{
			var helpArguments = new HelpArguments(
				new HelpHeader("fuse " + Name, Description),
				_helpSynopsis,
				new HelpDetailedDescription(
					"Creates a new daemon if no daemon exists, unless --no-spawn option is used.\n"
					+ "Send message to daemon by writing to stdin and receive message from daemon by reading from stdout."
				),
				new HelpOptions(_optionSet.ToTable()));

			_textWriter.WriteHelp(helpArguments);
		}

		/// <exception cref="ExitWithError" />
		public override void Run(string[] args, CancellationToken ct)
		{
			var clientName = _optionSet.Parse(args)
				.TryGetAt(0)
				.OrThrow(new ExitWithError("Expected client identifier. Usage: " + _helpSynopsis.Usage));

			try
			{
				var daemonConnection = ConnectToDaemon();
				var outStream = Console.OpenStandardOutput(16384);
				var inStream = Console.OpenStandardInput(16384);

				var stream = daemonConnection.GetStream();

				SendHello(clientName, stream);

				var readTask = Task.Run(
					() =>
					{
						while (true)
						{
							var availableBytes = Math.Max(1, daemonConnection.Available);
							var buffer = new byte[availableBytes];
							var bytesRead = stream.Read(buffer, 0, availableBytes);
							if(bytesRead == 0)
								throw new Exception("Lost connection to daemon.");

							outStream.Write(buffer, 0, bytesRead);
						}
					});

				var writeTask = Task.Run(
					() =>
					{
						while (true)
						{
							HandleSend(inStream, stream);
						}
					});

				while (true)
				{
					if (!daemonConnection.Connected)
					{
						// This should not happen, but better be safe.
						throw new ExitWithError("Lost connection to daemon.");
					}

					if (readTask.Exception != null)
						throw new ExitWithError(readTask.Exception.InnerException.Message);
					if (writeTask.Exception != null)
						throw new ExitWithError(writeTask.Exception.InnerException.Message);

					Thread.Sleep(50);
				}
			}
			catch (FailedToSpawnDaemon)
			{
				throw new ExitWithError("Failed to spawn daemon.", 10);
			}
			catch (FailedToConnectToDaemon)
			{
				throw new ExitWithError("Failed to connect to daemon.", 10);
			}
		}

		static void SendHello(string clientName, NetworkStream stream)
		{
			var helloPayload =
				"{" +
					"\"Name\": \"Hello\", " +
					"\"Id\": 0, " +
					"\"Arguments\": "
					+ "{"
						+ "\"Identifier\": \"" + clientName + "\","
						+ "\"DaemonKey\": \"" + DaemonKey.GetDaemonKey() + "\","
						+ "\"EventFilter\": \"Strict\""
					+ "}" +
					"}";

			var helloPayloadBytes = Encoding.UTF8.GetBytes(helloPayload);
			var helloRequestHead = Encoding.UTF8.GetBytes("Request\n" + helloPayloadBytes.Length + "\n");
			var helloRequest = helloRequestHead.Concat(helloPayloadBytes).ToArray();
			stream.Write(helloRequest, 0, helloRequest.Length);

			while (true)
			{
				var reader = new BinaryReader(stream);
				var messageType = Encoding.UTF8.GetString(reader.ReadLine());
				var lengthBytes = Encoding.UTF8.GetString(reader.ReadLine());

				int length;
				if (!int.TryParse(lengthBytes, out length))
					throw new ExitWithError("Failed to parse message length of expected Hello response.");

				// We need to read all data, even if we don't care about other things than hello response.
				reader.ReadBytes(length);

				if (messageType == "Response")
				{
					// TODO: We should probably verify that the response is a hello response. 
					break;
				}
			}
		}

		static void HandleSend(Stream inStream, Stream outStream)
		{
			var reader = new BinaryReader(inStream);
			var messageType = reader
				.ReadLine()
				.Concat(new[] { (byte)'\n' })
				.ToArray();

			var lengthBytes = reader
				.ReadLine()
				.Concat(new[] { (byte)'\n' })
				.ToArray();

			int length;
			if (!int.TryParse(Encoding.UTF8.GetString(lengthBytes), out length))
			{
				Console.Error.WriteLine("Failed to parse message length.");
				return;
			}

			var payload = reader.ReadBytes(length);
			outStream.Write(messageType, 0, messageType.Length);
			outStream.Write(lengthBytes, 0, lengthBytes.Length);
			outStream.Write(payload, 0, payload.Length);
			outStream.Flush();
		}

		TcpClient ConnectToDaemon()
		{
			if (_noSpawnMode)
			{
				return Connect();
			}
			else
			{
				try
				{
					return new TcpClient(IPAddress.Loopback.ToString(), 12122);
				}
				catch (Exception)
				{
					var p =_fuseLauncher.StartFuse("daemon", new[] { "-b" });
					p.WaitForExit();
					if(p.ExitCode != 0)
						throw new FailedToSpawnDaemon(p.ExitCode);

					return Connect();
				}				
			}
		}

		TcpClient Connect()
		{
			try
			{
				return new TcpClient(IPAddress.Loopback.ToString(), 12122);
			}
			catch (Exception e)
			{
				throw new FailedToConnectToDaemon(e);
			}		
		}

		OptionSet CreateOptions()
		{
			return new OptionSet()
			{
				{"no-spawn", "Prevents spawning of a new daemon if no daemon exists.\n"
				+ "An error is written to stderr if no daemon exists and process exits with none zero result.", a => _noSpawnMode = true }
			};
		}

	}

	static class BinaryReaderExtensions
	{
		public static byte []ReadLine(this BinaryReader reader)
		{
			if(!reader.BaseStream.CanRead)
				throw new ArgumentException("Reader don't have a readable stream.");

			var byteBuffer = new List<byte>(256);
			while (true)
			{
				var curByte = reader.ReadByte();
				
				if(curByte == '\n')
				{
					break;
				}

				byteBuffer.Add(curByte);
			}

			return byteBuffer.Where(b => b != '\r').ToArray();
		}
	}
}