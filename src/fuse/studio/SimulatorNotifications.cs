using System;
using System.Reactive.Linq;
using Outracks.Fusion;
using Outracks.Simulator.Protocol;

namespace Outracks.Fuse.Studio
{
	static class SimulatorNotifications
	{
		public static IControl Create(IObservable<IBinaryMessage> fromSimulator, Command rebuild, IProperty<bool> logViewIsExpanded)
		{
			// The idea here is that we track the state of the project on disk, based on what the simulator tells us when things causes reify/rebuild/

			var buildStarted = fromSimulator
				.TryParse(Started.MessageType, Started.ReadDataFrom)
				.Where(m => m.Command.Type == BuildProject.MessageType);

			var buildEnded = fromSimulator
				.TryParse(Ended.MessageType, Ended.ReadDataFrom)
				.Where(m => m.Command.Type == BuildProject.MessageType);

			var buildFailed = buildEnded.Where(s => s.Success == false);
			var buildSucceeded = buildEnded.Where(s => s.Success == true);

			var reifyStarted = fromSimulator
				.TryParse(Started.MessageType, Started.ReadDataFrom)
				.Where(m => m.Command.Type == GenerateBytecode.MessageType);

			var reifyEnded = fromSimulator
				.TryParse(Ended.MessageType, Ended.ReadDataFrom)
				.Where(m => m.Command.Type == GenerateBytecode.MessageType);

			var reifyFailed = reifyEnded.Where(s => s.Success == false);
			var reifySucceeded = reifyEnded.Where(s => s.Success == true);

			var buildRequired = fromSimulator
				.TryParse(RebuildRequired.MessageType, RebuildRequired.ReadDataFrom);

//			var reifyRequired = fromSimulator
//				.TryParse(ReifyRequired.MessageType, ReifyRequired.ReadDataFrom);

			// What notifications and buttons should be shown in response to what

			return Observable
				.Merge(
					buildRequired.Select(_ => Notification.Create("Rebuild is required for changes to take effect", Tuple.Create("Rebuild", rebuild))),
					buildFailed.Select(_ => Notification.Create("Build failed, see log for details", Tuple.Create("Rebuild", rebuild)).OnMouse(Command.Enabled(() => logViewIsExpanded.Write(true)))),
					reifyFailed.Select(_ => Notification.Create("Auto reload failed, see log for details").OnMouse(Command.Enabled(() => logViewIsExpanded.Write(true)))),
					buildStarted.Select(_ => Control.Empty),
					buildSucceeded.Select(_ => Control.Empty),
					reifyStarted.Select(_ => Control.Empty),
					reifySucceeded.Select(_ => Control.Empty))
				.StartWith(Control.Empty)
				.ObserveOn(Application.MainThread)
				.Switch();
		}


		public static IControl CreateBusyIndicator(IObservable<IBinaryMessage> fromSimulator)
		{
			var buildIndicator = BuildIndicator(
				fromSimulator: fromSimulator,
				messageType: BuildProject.MessageType,
				title: "Building...",
				foreground: Theme.BuildBarForeground,
				background: Theme.BuildBarBackground);

			var reifyIndicator = BuildIndicator(
				fromSimulator: fromSimulator,
				messageType: GenerateBytecode.MessageType,
				title: "Reifying...",
				foreground: Theme.ReifyBarForeground,
				background: Theme.ReifyBarBackground);

			return Observable
				.Merge(
					buildIndicator,
					reifyIndicator)
				.StartWith(Control.Empty)
				.ObserveOn(Application.MainThread)
				.Switch();
		}

		static IObservable<IControl> BuildIndicator(
			IObservable<IBinaryMessage> fromSimulator,
			string messageType,
			string title,
			IObservable<Color> foreground,
			IObservable<Color> background)
		{
			var started = fromSimulator
				.TryParse(Started.MessageType, Started.ReadDataFrom)
				.Where(m => m.Command.Type == messageType).Select(_ => true);

			var ended = fromSimulator
				.TryParse(Ended.MessageType, Ended.ReadDataFrom)
				.Where(m => m.Command.Type == messageType).Select(_ => false);

			// stream of bool that is true from 250 ms after messageType started and false when messageType ends
			var indicator =
				started
					.Merge(ended)
					.Throttle(TimeSpan.FromMilliseconds(250))
					.Merge(ended)
					.DistinctUntilChanged()
					.Select(
						building => building
							? Busy.Create(title, background, foreground)
							: Control.Empty);
			return indicator;
		}
	}
}