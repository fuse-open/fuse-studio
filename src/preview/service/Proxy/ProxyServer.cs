using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reactive;
using System.Reactive.Subjects;
using Outracks;
using Outracks.IPC;
using Uno.ProjectFormat;

namespace Fuse.Preview
{
	public class ProxyServer : IDisposable
	{
		public static ProxyServer Start(Action<string> log)
		{
			return new ProxyServer(
				log,
				new AndroidPortReverser(),
				port: 12124);
		}

		readonly Dictionary<int, ProxyProjectEntry> _projects = new Dictionary<int, ProxyProjectEntry>();
		readonly ISocketServer _socketServer;

		readonly Action<string> _logWarning;
		readonly AndroidPortReverser _portReverser;
		bool _hasReversedPort;

		public ProxyServer(Action<string> logWarning, AndroidPortReverser portReverser, int port)
		{
			_socketServer = SocketServer.Start(port, ClientThread);
			_logWarning = logWarning;
			_portReverser = portReverser;
			_isEmpty.OnNext(Unit.Default);
		}

		readonly Subject<Unit> _isEmpty = new Subject<Unit>();
		public IObservable<Unit> IsEmpty
		{
			get { return _isEmpty; }
		}

		public void AddProject(int port, string code, string projectPath, string[] defines)
		{
			lock (_projects)
			{
				_projects[port] = new ProxyProjectEntry
				{
					Port = port,
					Code = code,
					Project = Project.Load(projectPath),
					Defines = defines,
				};
			}
		}

		public void RemoveProject(int port)
		{
			lock (_projects)
			{
				_projects.Remove(port);
				if(_projects.Count <= 0)
					_isEmpty.OnNext(Unit.Default);
			}
		}

		public void UpdateReversedPorts(bool shouldUpdate)
		{
			if (_hasReversedPort && shouldUpdate == false)
				return;

			_portReverser.ReversePortOrLogErrors(
				ReportFactory.FallbackReport,
				remotePort: _socketServer.LocalEndPoint.Port,
				localPort: _socketServer.LocalEndPoint.Port);

			_hasReversedPort = true;
		}

		void ClientThread(NetworkStream stream, EndPoint proxyClientAddress)
		{
			using (var streamReader = new BinaryReader(stream))
			using (var streamWriter = new BinaryWriter(stream))
			{
				bool codeIsIncorrect = true;
				while (codeIsIncorrect)
				{
					var codeOrPath = streamReader.ReadString();
					var code = "";
					var path = "";
					if (codeOrPath.Contains(Path.DirectorySeparatorChar))
						path = codeOrPath;
					else
						code = codeOrPath;

					var defines = streamReader.ReadString().Split(" ").Where(d => !string.IsNullOrWhiteSpace(d)).ToArray();

					GetMatchingProject(path, code, defines).MatchWith(
						some: entry =>
						{
							// ReSharper disable AccessToDisposedClosure
							streamWriter.Write("SUCCESS");
							WriteEndPoints(streamWriter, entry.Port);
							streamWriter.Write(entry.Project.Name);

							streamWriter.WriteArray(
								entry.Project.PackageReferences,
								(writer, data) =>
								{
									writer.Write(data.PackageName);
									writer.Write(data.PackageVersion ?? "0.0.0");
								});

							streamWriter.WriteArray(
								entry.Project.ProjectReferences,
								(writer, data) =>
								{
									writer.Write(data.ProjectPath);
								});

							codeIsIncorrect = false;

						},
						none: () => streamWriter.Write("DESIGNER_NOT_RUNNING"));


					streamWriter.Flush();
				}
			}
		}

		Optional<ProxyProjectEntry> GetMatchingProject(string path, string code, string[] defines)
		{
			ProxyProjectEntry[] entries;
			lock (_projects)
				entries = _projects.Values.ToArray();

			foreach (var entry in entries)
			{
				var match = entry.Project.FullPath == path || entry.Code == code;

				//Ignore defines for now https://github.com/fusetools/Fuse/issues/2359#issuecomment-256032410
				if (match && !defines.SetEquals(entry.Defines))
				{
					_logWarning("Warning: The preview running on the device has a different set of defines than the one running on your computer. This is normally OK.");

					if (defines.Any())
						_logWarning("The defines on the device are: " + ("'" + string.Join("','", defines) + "'."));
					else
						_logWarning("There are no defines on your device.");

					if (entry.Defines.Any())
						_logWarning("The defines on the your computer are: " + ("'" + string.Join("','", entry.Defines) + "'."));
					else
						_logWarning("There are no defines on your computer.");
				}

				if (match)
					return entry;
			}

			return Optional.None();
		}


		static void WriteEndPoints(BinaryWriter streamWriter, int port)
		{
			var endPoints = NetworkHelper
				.GetInterNetworkIps()
				.Select(ip => new IPEndPoint(ip, port))
				.ToArray();

			streamWriter.Write(endPoints.Length);
			foreach (var endPoint in endPoints)
			{
				streamWriter.Write(endPoint.Address.ToString());
				streamWriter.Write(endPoint.Port);
			}

			streamWriter.Flush();
		}

		public void Dispose()
		{
			_socketServer.Dispose();
		}

	}
}