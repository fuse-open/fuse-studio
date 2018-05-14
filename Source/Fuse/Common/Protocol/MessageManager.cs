using System;
using System.Reactive;
using System.Reactive.Linq;

namespace Outracks.Fuse.Protocol
{
	public static class SerializerExtensions
	{
		public static IObserver<IMessage> Serialized(this IObserver<Message> messageJson, ISerializer serializer)
		{
			return Observer.Create<IMessage>(s => messageJson.OnNext(serializer.Serialize(s)));
		}

		public static IObservable<IMessage> TryDeserialize(this IObservable<Message> messageJson, ISerializer serializer, IReport report)
		{
			return messageJson.Select(json => serializer.TryDeserialize(json, report)).NotNone();
		}

		static Optional<IMessage> TryDeserialize(this ISerializer serializer, Message json, IReport report)
		{
			try
			{
				return Optional.Some(serializer.Deserialize(json));
			}
			catch (Exception e)
			{
				report.Error("Failed to deserialize " + json.MessageType + ": " + e.Message);
				return Optional.None();
			}
		}

		public static IObservable<IEventMessage<TRet>> Deserialize<TRet>(this IObservable<IEventMessage<UnresolvedMessagePayload>> message, IReport report) 
			where TRet : IEventData
		{
			return message
				.Where(m => m.Name == typeof (TRet).GetPayloadTypeName())
				.Select(m => m.Data.Deserialize<TRet>(report)
					.Select(d => new Event<TRet>(m.Name, d, m.SubscriptionId)))
				.NotNone();
		}

		public static IObservable<IRequestMessage<TRet>> Deserialize<TRet>(
			this IObservable<IRequestMessage<UnresolvedMessagePayload>> message, IReport report)
			where TRet : IRequestData
		{
			return message
				.Where(m => m.Name == typeof(TRet).GetPayloadTypeName())
				.Select(m => m.Arguments.Deserialize<TRet>(report)
					.Select(d => new Request<TRet>(m.Id, m.Name, d)))
				.NotNone();
		}
	}

}