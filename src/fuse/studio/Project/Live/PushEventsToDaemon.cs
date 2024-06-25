using System;
using System.Reactive.Linq;
using Fuse.Preview;
using Outracks.Fuse.Protocol;
using Outracks.Fuse.Protocol.Messages;
using Outracks.IO;
using Outracks.Simulator;
using Outracks.Simulator.Protocol;
using BuildIssueTypeData = Outracks.Fuse.Protocol.BuildIssueTypeData;

namespace Outracks.Fuse.Live
{
	public static class PushEventsToDaemon
	{
		public static IDisposable Start(
			IObservable<IBinaryMessage> messages,
			IMessagingService daemon,
			AbsoluteFilePath projectPath,
			Guid projectId,
			BuildTarget target)
		{
			// TODO: do something with reset from the daemon
//			var reset = daemon.BroadcastedEvents<ResetPreviewEvent>(false);

			var daemonEvents = Observable.Merge<IEventData>(
				messages.TryParse(Started.MessageType, Started.ReadDataFrom).SelectSome(started =>
					BinaryMessage.TryParse(started.Command, BuildProject.MessageType, BuildProject.ReadDataFrom).Select(build =>
						new BuildStartedData
						{
							BuildId = build.Id,
							ProjectPath = projectPath.NativePath,
							BuildType = BuildTypeData.FullCompile,
							ProjectId = projectId,
							Target = target,
						}).Or(
					BinaryMessage.TryParse(started.Command, GenerateBytecode.MessageType, GenerateBytecode.ReadDataFrom).Select(reify =>
						new BuildStartedData
						{
							BuildId = reify.Id,
							ProjectPath = projectPath.NativePath,
							BuildType = BuildTypeData.LoadMarkup,
							ProjectId = projectId,
							Target = target,
						}))),

				messages.TryParse(Ended.MessageType, Ended.ReadDataFrom).SelectSome(ended =>
					BinaryMessage.TryParse(ended.Command, BuildProject.MessageType, BuildProject.ReadDataFrom).Select(build =>
						new BuildEndedData
						{
							BuildId = build.Id,
							Status = ended.Success ? BuildStatus.Success : BuildStatus.Error,
						}).Or(
					BinaryMessage.TryParse(ended.Command, GenerateBytecode.MessageType, GenerateBytecode.ReadDataFrom).Select(reify =>
						new BuildEndedData
						{
							BuildId = reify.Id,
							Status = ended.Success ? BuildStatus.Success : BuildStatus.Error,
						}))),

				messages
					.TryParse(BuildLogged.MessageType, BuildLogged.ReadDataFrom)
					.Select(e => new BuildLoggedData
					{
						BuildId = e.BuildId,
						Message = e.Text,
					}),

				messages
					.TryParse(BuildIssueDetected.MessageType, BuildIssueDetected.ReadDataFrom)
					.Select(e => new BuildIssueDetectedData
					{
						BuildId = e.BuildId,
						Path = e.Source.HasValue ? e.Source.Value.File : "",
						IssueType = e.Severity.ToPluginBuildEventType(),
						Message = e.Message.Replace("\r", "\0"),
						ErrorCode = e.Code,
						StartPosition = e.Source.TryGetStartPosition().OrDefault(),
						EndPosition = e.Source.TryGetEndPosition().OrDefault(),
					}),

				messages
					.TryParse(RegisterName.MessageType, RegisterName.ReadDataFrom)
					.Select(r => new RegisterClientEvent
					{
						DeviceId = r.DeviceId,
						DeviceName = r.DeviceName,
						ProjectId = projectId.ToString()
					}),

				messages
					.TryParse(DebugLog.MessageType, DebugLog.ReadDataFrom)
					.Select(l => new LogEvent
					{
						DeviceId = l.DeviceId,
						DeviceName = l.DeviceName,
						ProjectId = projectId.ToString(),
						ConsoleType = ConsoleType.DebugLog,
						Message = l.Message,
						Timestamp = DateTime.Now
					}),

				messages
					.TryParse(UnhandledException.MessageType, UnhandledException.ReadDataFrom)
					.Select(e => new ExceptionEvent
					{
						DeviceId = e.DeviceId,
						DeviceName = e.DeviceName,
						ProjectId = projectId.ToString(),
						Type = e.Type,
						Message = e.Message,
						StackTrace = e.StackTrace,
						Timestamp = DateTime.Now
					}));

			return daemonEvents.Subscribe(daemon.Broadcast);
		}

		public static BuildIssueTypeData ToPluginBuildEventType(this BuildIssueType type)
		{
			switch (type)
			{
				case BuildIssueType.Error:
					return BuildIssueTypeData.Error;
				case BuildIssueType.FatalError:
					return BuildIssueTypeData.FatalError;
				case BuildIssueType.Message:
					return BuildIssueTypeData.Message;
				case BuildIssueType.Warning:
					return BuildIssueTypeData.Warning;
				default:
					return BuildIssueTypeData.Unknown;
			}

		}

		static Optional<Protocol.Messages.TextPosition> TryGetStartPosition(this Optional<SourceReference> source)
		{
			return source
				.SelectMany(s => s.Location)
				.Select(s => new Protocol.Messages.TextPosition
				{
					Line = s.Line,
					Character = s.Character,
				});
		}

		static Optional<Protocol.Messages.TextPosition> TryGetEndPosition(this Optional<SourceReference> source)
		{
			return Optional.None();
		}
	}
}