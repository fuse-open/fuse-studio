using System;
using System.Linq.Expressions;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace Outracks.Fusion.Windows
{
	public static class DataBinding
	{

		public static void BindNativeDefaults(this IMountLocation self, FrameworkElement element, IScheduler dispatcher)
		{
			var frame = self.NativeFrame;

			self.BindNativeProperty(
				dispatcher, "left", frame.Left(),
				position =>
				{
					if (!double.IsInfinity(position))
						Canvas.SetLeft(element, position);
				});
			self.BindNativeProperty(
				dispatcher, "top", frame.Top(),
				position =>
				{
					if (!double.IsInfinity(position))
						Canvas.SetTop(element, position);
				});
			self.BindNativeProperty(
				dispatcher, "height", frame.Height,
				height =>
				{
					if (!double.IsInfinity(height))
						element.Height = Math.Max(0, (height));
				});
			self.BindNativeProperty(
				dispatcher, "width", frame.Width,
				width =>
				{
					if (!double.IsInfinity(width))
						element.Width = Math.Max(0, (width));
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
							update((TElement)control.NativeHandle, v);
					}));
		}

		public static void BindNativeProperty<TWpfControl, TValue>(
			this IMountLocation fusionControl,
			TWpfControl wpfControl,
			Expression<Func<TWpfControl, TValue>> propertyExpression,
			IProperty<TValue> sourceProperty,
			string eventName = null)
			where TWpfControl : System.Windows.Controls.Control
		{
			var memberExpression = propertyExpression.Body as MemberExpression;
			PropertyInfo wpfProperty = null;
			if (memberExpression != null)
			{
				wpfProperty = memberExpression.Member as PropertyInfo;
			}
			if (wpfProperty == null)
				throw new ArgumentException("Unable to extract property name from expression");
			if (wpfProperty.SetMethod == null || !wpfProperty.SetMethod.IsPublic)
				throw new ArgumentException("Property does not have a public setter");

			sourceProperty = sourceProperty
				.ConnectWhile(fusionControl.IsRooted)
				.DistinctUntilChangedOrSet();

			var propertyGetter = propertyExpression.Compile();

			bool valueSetByUser = true;

			ObservableFromNativeEvent<EventArgs>(wpfControl, eventName ?? (wpfProperty.Name + "Changed"))
				.Select(_ => propertyGetter(wpfControl))
				.Where(v => valueSetByUser)
				.Subscribe(sourceProperty);

			sourceProperty
				.Select(v => new Action(() =>
				{
					valueSetByUser = false;
					try { wpfProperty.SetValue(wpfControl, v); }
					finally { valueSetByUser = true; }
				}))
				.Subscribe(action => Fusion.Application.MainThread.Schedule(action));

		}

		public static IObservable<T> ObservableFromNativeEvent<T>(object nativeControl, string name)
		{
			return Observable.Create<T>(async observer =>
				await Fusion.Application.MainThread.InvokeAsync(() =>
					Observable.FromEventPattern<T>(nativeControl, name)
						.Subscribe(pattern =>
							observer.OnNext(pattern.EventArgs))));
		}

		public static IObservable<Optional<FrameworkElement>> GetNativeControlWhileMounted(this IControl self)
		{
			return Fusion.Application.MainThread
				.InvokeAsync(() => self.NativeHandle)
				.ToObservable()
				.OfType<FrameworkElement>()
				.Select(element =>
					self.IsRooted.Select(isRooted => 
						isRooted
							? Optional.Some(element)
							: Optional.None()))
				.Switch();
		}
	}
}