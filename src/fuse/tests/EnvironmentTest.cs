using System;
using System.Net.Sockets;
using Outracks.Fuse.Protocol;

namespace Outracks.EditorService.Tests
{
	static class EnvironmentTest
	{
		static int Main()
		{
			var tcpClient = new TcpClient("127.0.0.1", 12122);
			var client = new LocalSocketClient(tcpClient);
			client.Disconnected.Subscribe(c => WriteColoredLine("Lost connnection", ConsoleColor.DarkRed));

			//client.EnqueueMessage("{\"MessageType\":\"Request\", \"Type\":\"Hello\"}");
			Console.ReadLine();

			/*Console.Write("Your name: ");
			var name = Console.ReadLine();
			Console.Write("Do you want swear word protection? ");
			var swearWordProtection = Console.ReadLine().ToUpper() == "YES";

			Console.Write("Do you want to be an admin? ");
			var isAdmin = Console.ReadLine().ToUpper() == "YES";

			var serializer = new Serializer(new PayloadParserContext(
				typeof(HelloRequest),
				typeof(HelloResponse),
				typeof(SendMessageToAdminRequest),
				typeof(SendMessageToAdminResponse),
				typeof(Message),
				typeof(SwearMessage),
				typeof(UserConnected)));

			var messagesOut = client.OutgoingMessages.Serialized(serializer);
			var messagesIn = client.IncomingMessages.TryDeserialize(serializer, null);

			messagesIn
				.OfType<IEventMessage<UserConnected>>()
				.Subscribe(u => WriteColoredLine(u.Data.Name + " has connected to Fuse group chat (" + u.Data.DateTime.ToShortTimeString() + ")", ConsoleColor.Cyan));

			messagesIn
				.OfType<IEventMessage<Message>>()
				.Subscribe(m => WriteColoredLine(m.Data.Text, ConsoleColor.White));

			messagesIn
				.OfType<IRequestMessage<SendMessageToAdminRequest>>()
				.Subscribe(
					r =>
					{
						WriteColoredLine(r.Data.Name + " sent you a PM: " + r.Data.Message, ConsoleColor.Yellow);
						messagesOut.OnNext(Response.CreateSuccess(r, new SendMessageToAdminResponse() { Answer = "I got the message." }));
					});

			messagesIn
				.OfType<IResponseMessage<SendMessageToAdminResponse>>()
				.Subscribe(res => WriteColoredLine("Admin responded: " + res.Data.Answer, ConsoleColor.DarkGreen));

			var waitForHelloResponse = messagesIn
				.OfType<IResponseMessage<HelloResponse>>()
				.FirstAsync().ToTask();

			client.StartRead();

			var eventFilter = swearWordProtection ? "^((?!SwearMessage).)*$" : "";
			var implements = isAdmin ? new List<string> { "Fuse.Test.MessageToAdmin" } : new List<string> { "" };

			messagesOut.OnNext(Request.Create(0, new HelloRequest() { Identifier = name, EventFilter = eventFilter, Implements = implements }));
			waitForHelloResponse.Wait();

			messagesOut.OnNext(Event.Create(new UserConnected() { Name = name, DateTime = DateTime.Now }));

			var counter = 0;
			while (tcpClient.Connected)
			{
				var data = counter + "\n";// Console.ReadLine();
				messagesOut.OnNext(Event.Create(new Message() { Text = data }));
				++counter;

				/*if(data.StartsWith("/admin"))
					messagesOut.OnNext(Request.Create(0, new SendMessageToAdminRequest() { Message = data.Substring(7, data.Length - 7), Name = name }));
				else
					messagesOut.OnNext(Event.Create(CreateMessage(name + ": " + data)));
			}*/

			return 0;
		}

		static IEventData CreateMessage(string message)
		{
			if (IsSwearWord(message))
				return new SwearMessage() { Text = message };
			else
				return new Message() { Text = message };
		}

		static bool IsSwearWord(string msg)
		{
			if (msg.ToUpper().Contains("FUCK"))
			{
				WriteColoredLine("Warning Fuck is considered a swear word.", ConsoleColor.Red);
				return true;
			}
			if (msg.ToUpper().Contains("VEGARD"))
			{
				WriteColoredLine("Warning Vegard is considered a swear word.", ConsoleColor.Red);
				return true;
			}

			return false;
		}

		static void WriteColoredLine(string text, ConsoleColor color)
		{
			var oldColor = Console.ForegroundColor;
			Console.ForegroundColor = color;
			Console.WriteLine(text);
			Console.ForegroundColor = oldColor;
		}
	}

	[PayloadTypeName("Fuse.DebugLog")]
	class Message : IEventData
	{
		public string Text;
	}

	[PayloadTypeName("Fuse.Test.SwearMessage")]
	class SwearMessage : Message
	{
	}
}