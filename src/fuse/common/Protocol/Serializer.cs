using System;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Outracks.IO;

namespace Outracks.Fuse.Protocol
{
	public interface ISerializer
	{
		IMessage Deserialize(Message message);
		Message Serialize(IMessage cmd);
	}

	public class Serializer : ISerializer
	{
		public IMessage Deserialize(Message message)
		{
			var parsedResult = JObject.Parse(message.Payload);
			var messageType = message.MessageType;

			var type = CommandTypeBasedOnName(messageType);

			var instance = (IMessage)parsedResult.ToObject(type, FuseJsonSerializer.CreateDefault());
			return instance;
		}

		Type CommandTypeBasedOnName(string messageType)
		{
			switch (messageType)
			{
				case "Request":
					return typeof(Request<>).MakeGenericType(typeof(UnresolvedMessagePayload));
				case "Response":
					return typeof(Response<>).MakeGenericType(typeof(UnresolvedMessagePayload));
				case "Event":
					return typeof(Event<>).MakeGenericType(typeof(UnresolvedMessagePayload));
			}

			return null;
		}

		public Message Serialize(IMessage cmd)
		{
			return new Message(cmd.MessageType, JsonConvert.SerializeObject(cmd, FuseJsonSerializer.Settings));
		}
	}

	public class UnresolvedMessagePayload : IEventData, IRequestData, IResponseData
	{
		public readonly Optional<JObject> Obj;

		public UnresolvedMessagePayload(Optional<JObject> obj)
		{
			Obj = obj;
		}

		public Optional<T> Deserialize<T>(IReport report)
		{
			return Obj.SelectMany(
				o =>
				{
					try
					{
						return o.ToObject<T>(FuseJsonSerializer.CreateDefault());
					}
					catch (Exception e)
					{
						report.Error("Failed to deserialize: " + e.Message);
						return Optional.None<T>();
					}
				});
		}

		public override string ToString()
		{
			return Obj.HasValue ? Obj.Value.ToString() : "Failed to parse message, or unknown payload type";
		}
	}

	public static class FuseJsonSerializer
	{
		public static readonly JsonSerializerSettings Settings;

		static FuseJsonSerializer()
		{
			Settings = new JsonSerializerSettings();
			Settings.Converters.Add(new StringEnumConverter());
			Settings.Converters.Add(new AbsoluteFilePathConverter());
			Settings.Converters.Add(new IPAddressConverter());
			Settings.Converters.Add(new IPEndPointConverter());
			Settings.Converters.Add(new UnresolvedMessagePayloadConverter());
		}

		public static JsonSerializer CreateDefault()
		{
			return JsonSerializer.CreateDefault(Settings);
		}
	}

	class AbsoluteFilePathConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(AbsoluteFilePath);
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var ip = (AbsoluteFilePath)value;
			writer.WriteValue(ip.NativePath);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var token = JToken.Load(reader);
			return AbsoluteFilePath.Parse(token.Value<string>());
		}
	}

	class IPAddressConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(IPAddress);
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var ip = (IPAddress) value;
			writer.WriteValue(ip.ToString());
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var token = JToken.Load(reader);
			return IPAddress.Parse(token.Value<string>());
		}
	}


	class IPEndPointConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(IPEndPoint);
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var ep = (IPEndPoint) value;
			writer.WriteStartObject();
			writer.WritePropertyName("Address");
			serializer.Serialize(writer, ep.Address);
			writer.WritePropertyName("Port");
			writer.WriteValue(ep.Port);
			writer.WriteEndObject();
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var jo = JObject.Load(reader);
			var address = jo["Address"].ToObject<IPAddress>(serializer);
			var port = jo["Port"].Value<int>();
			return new IPEndPoint(address, port);
		}
	}

	class UnresolvedMessagePayloadConverter : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var payload = (UnresolvedMessagePayload)value;
			payload.Obj.Do(o => o.WriteTo(writer), writer.WriteNull);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			if (reader.TokenType == JsonToken.Null)
			{
				return new UnresolvedMessagePayload(Optional.None());
			}

			var obj = JObject.Load(reader);
			return new UnresolvedMessagePayload(obj);
		}

		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(UnresolvedMessagePayload);
		}
	}
}