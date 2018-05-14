using System;

namespace Outracks.Fusion
{
	public static partial class Transformation
	{
		public static IControl WithTransformation(this IControl control, IObservable<Matrix> matrix)
		{
			return _withTransformationFactory(control, matrix);
		}

		static Func<IControl, IObservable<Matrix>, IControl> _withTransformationFactory;
		public static void Initialize(Func<IControl, IObservable<Matrix>, IControl> factory)
		{
			_withTransformationFactory = factory;
		}
	}
}