using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Outracks;
using Outracks.IO;
using Outracks.Simulator.Parser;
using Outracks.Simulator.Protocol;
using Uno.Logging;

namespace Fuse.Preview
{
	public interface ISimulatorBuilder
	{
		Optional<ProjectBuild> TryBuild(BuildProject args, Log log);
	}

	public class Builder
	{
		readonly ISubject<Optional<ProjectBuild>> _result = new ReplaySubject<Optional<ProjectBuild>>(1);
		readonly ISimulatorBuilder _simulatorBuilder;

		public Builder(ISimulatorBuilder simulatorBuilder)
		{
			_simulatorBuilder = simulatorBuilder;
		}

		public IObservable<Optional<ProjectBuild>> Result
		{
			get { return _result; }
		}


		readonly object _buildLock = new object();
		/// <remarks> This function updates the Result property as a side-effect </remarks>
		/// <returns> (Started (LogEvent)* [AssemblyBuilt] Ended)* </returns>
		public IObservable<IBinaryMessage> Build(IObservable<BuildProject> args)
		{
			return args
				.Switch(a => Observable.Defer(() =>
				{
					var log = new LogSubject(a.Id);
					return Observable
						.Start(
							() =>
							{
								lock (_buildLock)
									return _simulatorBuilder.TryBuild(a, log.Log);
							})
						.Do(_result.OnNext)
						.SelectMany(result =>
							result.HasValue
								? new IBinaryMessage[]
								{
									new AssemblyBuilt { Assembly = AbsoluteFilePath.Parse(result.Value.Assembly) },
									new Ended { Command = a, Success = true, BuildDirectory = AbsoluteDirectoryPath.Parse(a.OutputDir) },
								}
								: new IBinaryMessage[]
								{
									new Ended { Command = a, Success = false, BuildDirectory = AbsoluteDirectoryPath.Parse(a.OutputDir) }
								})
						.StartWith(new Started { Command = a })
						.Merge(log.Messages);
				}));
		}

	}
}