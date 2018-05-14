using System;
using System.Reactive.Linq;
using AppKit;
using Foundation;
using ObjCRuntime;

namespace Outracks.Fusion.OSX
{
	static class Target
	{
		public static IObservable<object> WhenChanged(this NSControl control)
		{
			return Observable.Create<object>(observer =>
			{
				control.Action = Target.Selector;
				control.Target = Target.Create(changed: () =>
					observer.OnNext(control.ObjectValue));

				return Disposable.Create(() =>
				{
					control.Action = null;
					control.Target = null;
				});
			});
		}

		public static readonly Selector Selector = new Selector("valueChange:");

		public static NSObject Create(Action changed)
		{
			return new DelegateObserver(changed);
		}
	}

	class DelegateObserver : NSObject
	{
		readonly Action _changed;

		public DelegateObserver(IntPtr handle) : base(handle)
		{			
		}

		public DelegateObserver(Action changed)
		{
			_changed = changed;
		}
		[Action("colorUpdate:")]
		public void ColorUpdate(NSObject sender)
		{
			_changed();
		}
		[Action("valueChange:")]
		public void ValueChange(NSObject sender)
		{
			_changed();
		}
	}
}
