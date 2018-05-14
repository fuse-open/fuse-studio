using System;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Outracks.Fusion
{
	public interface INotifier : IDisposable
	{
		Task<NotificationFeedback> Show(Notification notification);
	}

	public enum NotifyType
	{
		Error,
		Info
	}

	public class Notification
	{
		public NotifyType Type { get; set; }
		public Optional<string> PrimaryAction { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }

		public Optional<TimeSpan> Timeout { get; set; }

		public Notification()
		{
			Title = "Hi there";
			Description = "Have a nice day!";
			Timeout = Optional.None();
		}
	}

	public enum NotificationFeedback
	{
		PrimaryActionTaken,
		Dismissed,
		//DismissedByTimeout
	}

}