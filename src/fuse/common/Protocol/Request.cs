using Newtonsoft.Json;

namespace Outracks.Fuse.Protocol
{
	public sealed class Request<T> : IRequestMessage<T> where T : IRequestData
	{
		[JsonIgnore]
		public string MessageType { get { return "Request"; } }

		public int Id { get; private set; }

		public string Name { get; private set; }

		public T Arguments { get; private set; }

		[JsonConstructor]
		public Request(int id, string name, T arguments)
		{
			Id = id;
			Name = name;
			Arguments = arguments;
		}
	}

	public static class Request
	{
		public static Request<T> Create<T>(int id, T arguments) where T : IRequestData
		{
			return new Request<T>(id, arguments.GetPayloadType(), arguments);
		}
	}
}