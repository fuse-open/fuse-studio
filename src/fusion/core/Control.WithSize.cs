using System;
using System.Reactive.Linq;

namespace Outracks.Fusion
{
	public static partial class Control
	{
		// Override width

		public static IControl WithWidth(this IControl control, Points width)
		{
			return control.WithWidth(Observable.Return(width));
		}
		public static IControl WithWidth(this IControl control, Func<IControl, IObservable<Points>> width)
		{
			return control.WithWidth(width(control));
		}
		public static IControl WithWidth(this IControl control, IObservable<Points> width)
		{
			return control.WithDimension(Axis2D.Horizontal, width);
		}


		// Override height

		public static IControl WithHeight(this IControl control, Points height)
		{
			return control.WithHeight(Observable.Return(height));
		}

		public static IControl WithHeight(this IControl control, Func<IControl, IObservable<Points>> height)
		{
			return control.WithHeight(height(control));
		}

		public static IControl WithHeight(this IControl control, IObservable<Points> height)
		{
			return control.WithDimension(Axis2D.Vertical, height);
		}


		// Override dimension

		public static IControl WithDimension(this IControl control, Axis2D axis, Func<IControl, IObservable<Points>> size)
		{
			return control.WithDimension(axis, size(control));
		}

		public static IControl WithDimension(this IControl control, Axis2D axis, IObservable<Points> size)
		{
			return control.WithSize(control.DesiredSize.WithAxis(axis, _ => size));
		}

		// Override size

		public static IControl WithSize(this IControl control, Size<Points> size)
		{
			return control.WithSize(
				Size.Create(
					Observable.Return(size.Width),
					Observable.Return(size.Height)));
		}

		public static IControl WithSize(this IControl control, Size<IObservable<Points>> desiredSize)
		{
			return new ControlWithSize(control, desiredSize);
		}
	}

	class ControlWithSize : IControl
	{
		readonly IControl _control;

		public ControlWithSize(IControl control, Size<IObservable<Points>> desiredSize)
		{
			_control = control;
			DesiredSize = desiredSize;
		}

		public Action<IMountLocation> Mount
		{
			get { return _control.Mount; }
		}

		public Size<IObservable<Points>> DesiredSize
		{
			get; private set;
		}

		public object NativeHandle
		{
			get { return _control.NativeHandle; }
		}

		public IObservable<bool> IsRooted
		{
			get { return _control.IsRooted; }
		}
	}
}
