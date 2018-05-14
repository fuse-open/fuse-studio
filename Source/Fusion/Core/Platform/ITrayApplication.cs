using System;

namespace Outracks.Fusion
{
	public interface ITrayApplication : INotifier
	{
		IObservable<int> UserClicked { get; }
	}

}