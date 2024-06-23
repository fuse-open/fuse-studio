using System;
using System.Reactive;
using Outracks.IO;

namespace Outracks.Fusion
{
	public class DropOperation
	{
		public static DropOperation None
		{
			get
			{
				return new DropOperation()
				{
					CanDrop = _ => false,
					Drop = _ => { },
					OnError = Observer.Create<string>(_ => { }),
				};
			}
		}

		public static DropOperation Create(Func<AbsoluteFilePath, bool> canDrop, Action<AbsoluteFilePath> drop, IObserver<string> onError)
		{
			return new DropOperation
			{
				CanDrop = canDrop,
				Drop = drop,
				OnError = onError
			};
		}

		public Func<AbsoluteFilePath, bool> CanDrop {get; private set;}
		public Action<AbsoluteFilePath> Drop {get; private set;}
		public IObserver<string> OnError { get; private set; }

	}
}

