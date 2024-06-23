using System;
using System.Linq.Expressions;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reflection;
using AppKit;

namespace Outracks.Fusion.Mac
{
	public static class DataBinding
	{
		public static void BindNativeDefaults(this IMountLocation self, NSView element, IScheduler dispatcher)
		{
			var frame = self.NativeFrame;

			self.BindNativeProperty(dispatcher, "position", frame.Position.Transpose(), position =>
			{
				element.SetFrameOrigin(position.ToPoint());
				element.NeedsDisplay = true;
			});

			self.BindNativeProperty(dispatcher, "size", frame.Size.Transpose(), size =>
			{
				element.SetFrameSize(size.Max(0, 0).ToSize());
				element.NeedsDisplay = true;
			});
		}

		public static void BindNativeProperty<TElement, TValue>(
			this IControl control,
			IScheduler dispatcher,
			string name,
			IObservable<TValue> value,
			Action<TElement, TValue> update)
		{
			value
				.ConnectWhile(control.IsRooted)
				.DistinctUntilChanged()
				.Subscribe(v =>
					dispatcher.Schedule(() =>
					{
						if (control.NativeHandle is TElement)
							update((TElement) control.NativeHandle, v);
					}));
		}

		public static void BindNativeProperty<TValue, TSome>(
			this IMountLocation fusionControl,
			NSControl nativeControl,
			Expression<Func<NSControl, TValue>> propertyExpression,
			IObservable<TSome> onChange,
			IProperty<TValue> sourceProperty)
		{
			var memberExpression = propertyExpression.Body as MemberExpression;
			PropertyInfo nativeProperty = null;
			if (memberExpression != null)
			{
				nativeProperty = memberExpression.Member as PropertyInfo;
			}
			if (nativeProperty == null)
				throw new ArgumentException("Unable to extract property name from expression");

			sourceProperty = sourceProperty
				.ConnectWhile(fusionControl.IsRooted)
				.DistinctUntilChangedOrSet();

			var propertyGetter = propertyExpression.Compile();

			var setByUser = false;

			onChange
				.Select(_ => propertyGetter(nativeControl))
				.Where(_ => setByUser)
				.Subscribe(sourceProperty);

			sourceProperty
				.Select(
					v => (Action) (() =>
					{
						setByUser = false;
						try
						{
							nativeProperty.SetValue(nativeControl, v);
						}
						finally
						{
							setByUser = true;
						}
					}))
				.Subscribe(action => {
										Fusion.Application.MainThread.Schedule(action);
				});
		}

		public static IObservable<T> ObservableFromNativeEvent<T>(object nativeControl, string name)
		{
			return Observable.Create<T>(async observer =>
				await Fusion.Application.MainThread.InvokeAsync(() =>
					Observable.FromEventPattern<T>(nativeControl, name)
						.Subscribe(pattern =>
							observer.OnNext(pattern.EventArgs))));
		}
	}
}