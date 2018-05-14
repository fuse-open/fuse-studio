using System;
using System.Reactive.Subjects;

namespace Outracks.Fusion
{
	public static partial class Control
	{
		public static IControl BindAvailableSize(Func<Size<IObservable<Points>>, IControl> content)
		{
			return Bind(self => content(self.AvailableSize));
		}

		public static IControl BindNativeFrame(Func<Rectangle<IObservable<Points>>, IControl> content)
		{
			return Bind(self => content(self.NativeFrame));
		}

		public static IControl Bind(Func<IMountLocation, IControl> content)
		{
			return new BindControl(content);
		}
	}

	class BindControl : IControl
	{
		readonly BehaviorSubject<IMountLocation> _location = new BehaviorSubject<IMountLocation>(MountLocation.Unmounted);
		readonly IControl _control;
		
		public BindControl(Func<IMountLocation, IControl> content)
		{
			_control = content(_location.Switch());
		}

		public Action<IMountLocation> Mount
		{
			get { return _location.OnNext + _control.Mount; }
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
