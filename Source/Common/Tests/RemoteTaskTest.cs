using System;

namespace Outracks.IPC.Tests
{
	class RemoteTaskTest
	{
		/*[Test]
		public void HostExecutesClientInvokes()
		{
			RegisterSerializers();

			var guid = Guid.NewGuid();
		    var name = new PipeName(guid);
			var host = Task.Factory.StartNew(
				() =>
				{
                    return RemoteTask.HostExecuter<TaskArgs, TaskResult>(name,
						(a, c) => Task.FromResult(new TaskResult(1337)));
				});

            var client = RemoteTask.ConnectToExecuter<TaskArgs, TaskResult>(name);

			var args = new TaskArgs();
			var task = client.Run(args, CancellationToken.None);
			var result = task.Result;

			Assert.AreEqual(result.Magic, 1337);
			client.Dispose();
			host.Dispose();
		}

		[Test]
		public void ClientExecutesHostInvokes()
		{
			RegisterSerializers();

			var guid = Guid.NewGuid();
		    var name = new PipeName(guid);			
            Task.Factory.StartNew(
				() =>
				{
                    var host = RemoteTask.HostInvoker<TaskArgs, TaskResult>(name);

					var args = new TaskArgs();
					var task = host.Run(args, CancellationToken.None);
					var result = task.Result;

					Assert.AreEqual(result.Magic, 1338);
					host.Dispose();
				});

            var client = RemoteTask.ConnectToInvoker<TaskArgs, TaskResult>(name, 
				(a, c) => Task.FromResult(new TaskResult(1338)));

			client.Dispose();
		}

		static void RegisterSerializers()
		{
			BinarySerializer.Register((w, o) => { }, (r) => new TaskArgs());
			BinarySerializer.Register((w, o) => w.Write(o.Magic), (r) => new TaskResult(r.ReadInt32()));
			BinarySerializer.Register((w, o) => { }, (r) => new TaskException());
		}
		 * */
	}




	class TaskException : Exception
	{
		
	}

	class TaskResult
	{
		public int Magic { get; private set; }

		public TaskResult(int magic)
		{
			Magic = magic;
		}

	}

	class TaskArgs
	{

	}
}
