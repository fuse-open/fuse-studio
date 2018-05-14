using System;
using System.Reactive.Linq;

namespace Outracks.Fusion
{
	public static partial class Control
	{
		public static readonly IControl Empty = new EmptyControl();
	}

	class EmptyControl : IControl
	{
		public Action<IMountLocation> Mount
		{
			get { return _ => { }; }
		}

		public IObservable<bool> IsRooted
		{
			get { return Observable.Return(false); }
		}

		public Size<IObservable<Points>> DesiredSize
		{
			get { return ObservableMath.ZeroSize; }
		}

		public object NativeHandle
		{
			get { return null; }
		}
	}
}
