using System;

namespace Outracks.Fusion
{
	public static partial class Transformation
	{
		public static IControl WithFixedPosition(this IControl control, Rectangle<IObservable<Points>> frame)
		{
			if (_withFixedPositionFactory != null)
				return _withFixedPositionFactory(control, frame);

			return control.WithFrame(frame);
		}		

		static Func<IControl, Rectangle<IObservable<Points>>, IControl> _withFixedPositionFactory;
		public static void Initialize(Func<IControl, Rectangle<IObservable<Points>>, IControl> factory)
		{
			_withFixedPositionFactory = factory;
		}
	}
}