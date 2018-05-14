using System;
using System.Reactive.Linq;

namespace Outracks.Fusion
{
	public static partial class Control
	{		
		public static IControl WithFrame(this IControl control, Rectangle<Points> frame, Size<IObservable<Points>>? availableSize = null)
		{
			return control.WithFrame(Observable.Return(frame).Transpose(), availableSize);
		}

		public static IControl WithFrame(
			this IControl control, 
			Rectangle<IObservable<Points>> frame, 
			Size<IObservable<Points>>? availableSize = null)
		{
			return control.WithFrame(_ => frame.MoveTo(_.Position), _ => availableSize ?? _);
		}

		public static IControl WithFrame(
			this IControl control,
			Func<Rectangle<IObservable<Points>>, Rectangle<IObservable<Points>>> frame,
			Func<Size<IObservable<Points>>, Size<IObservable<Points>>> availableSize = null)
		{
			return control.WithMountLocation(prev => 
				new MountLocation.Mutable
				{
					IsRooted = prev.IsRooted,
					NativeFrame = frame(prev.NativeFrame), 
					AvailableSize = availableSize != null 
						? availableSize(prev.AvailableSize) 
						: prev.AvailableSize,
				});
		}
		
		public static IControl WithMountLocation(
			this IControl control, 
			Func<IMountLocation, IMountLocation> transform)
		{
			return new ControlWithMountLocation(control, transform);
		}
	}

	class ControlWithMountLocation : IControl
	{
		readonly IControl _control;
		readonly Func<IMountLocation, IMountLocation> _transform;

		public ControlWithMountLocation(IControl control, Func<IMountLocation, IMountLocation> transform)
		{
			_control = control;
			_transform = transform;
		}

		public Action<IMountLocation> Mount
		{
			get { return location => _control.Mount(_transform(location)); }
		}

		public Size<IObservable<Points>> DesiredSize
		{
			get { return _control.DesiredSize; }
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
