using Newtonsoft.Json;

namespace Outracks.Fuse.Protocol
{
	public sealed class Event<T> : IEventMessage<T> where T : IEventData
	{
		[JsonIgnore]
		public string MessageType { get { return "Event"; } }		

		public string Name { get; private set; }
		public T Data { get; private set; }

		// SHOULD ONLY BE USED BY THE DAEMON
		public int SubscriptionId { get; private set; }

		[JsonConstructor]
		public Event(string name, T data, int subscriptionId = -1)
		{
			Name = name;
			Data = data;
			SubscriptionId = subscriptionId;
		}
	}

	public static class Event
	{
		public static Event<T> Create<T>(T data) where T : IEventData
		{
			return new Event<T>(data.GetPayloadType(), data);
		}
	}
}