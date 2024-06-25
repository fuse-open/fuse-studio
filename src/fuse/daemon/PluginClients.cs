using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Outracks.Fuse.Protocol;

namespace Outracks.Fuse.Daemon
{
	class PluginClients
	{
		readonly ConcurrentDictionary<IMessageConnection, PluginClient> _clients = new ConcurrentDictionary<IMessageConnection, PluginClient>();
		int _currentGlobalRequestId = 0;

		public void Add(IMessageConnection socket, PluginClient client)
		{
			_clients.TryAdd(socket, client);
		}

		public void PassRequestToAnImplementor(PluginClient whoRequested, IRequestMessage<IRequestData> request)
		{
			var clientsThatImplementsRequest = _clients.Values.Where(c => c.IsImlementorOf(request.Name));
			foreach (var client in clientsThatImplementsRequest)
			{
				var responseTask = client.SendRequest(whoRequested, Interlocked.Increment(ref _currentGlobalRequestId), request);
				if (!responseTask.Wait(TimeSpan.FromMinutes(1)))
					continue;

				var response = responseTask.Result;
				if (response.Status == Status.Unhandled)
					continue;

				whoRequested.SendResponse(response.ChangeId(request.Id));
				return;
			}

			whoRequested.SendResponse(Response.CreateUnhandled(request, new Error(ErrorCode.InvalidRequest, "Did not find an implementor for " + request.Name), (UnresolvedMessagePayload)null));
		}

		public void Remove(LocalSocketClient socket)
		{
			PluginClient pluginClient;
			_clients.TryRemove(socket, out pluginClient);
			pluginClient.DoIfNotNull(c => c.Dispose());
		}
	}
}