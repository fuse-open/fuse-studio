using System;
using System.Reactive.Concurrency;

namespace Outracks.Fusion
{
	public static class PropertyBindingExtensions
	{
		public static IDisposable Subscribe<T>(this IObservable<T> self, IProperty<T> property)
		{
			return self.Subscribe(v => property.Write(v, save: false));
		}

		/// <summary>
		/// Binds an observable to an update handler while control is rooted.
		/// </summary>
		/// <typeparam name="TValue">Type value bound</typeparam>
		public class OneWayPropertyBinding<TValue>
		{
			private readonly IObservable<TValue> _value;
			private IDisposable _valueSubscription;
			private Optional<TValue> _lastValue;
			private readonly IScheduler _dispatcher;
			private readonly Action<TValue> _update;

			private OneWayPropertyBinding(IObservable<TValue> value, IScheduler dispatcher, Action<TValue> update)
			{
				_value = value;
				_dispatcher = dispatcher;
				_update = update;
			}

			private void IsRootedChanged(bool b)
			{
				if (b)
				{
					if (_valueSubscription == null)
					{
						_valueSubscription = _value.Subscribe(OnValueChanged);
					}
				}
				else
				{
					if (_valueSubscription != null)
					{
						_valueSubscription.Dispose();
						_valueSubscription = null;
					}
				}
			}

			private void OnValueChanged(TValue v)
			{
				if (_lastValue.HasValue && Equals(_lastValue.Value, v))
					return;

				//Console.WriteLine(_name + " changed from " + _lastValue + " to " + v);
				_lastValue = v;
				_dispatcher.Schedule(Update);
			}

			void Update()
			{
				_update(_lastValue.Value);
			}

			public static void Bind(
				IObservable<bool> isRooted,
				IScheduler dispatcher,
				IObservable<TValue> value,
				Action<TValue> update,
				string name)
			{
				isRooted.Subscribe(new OneWayPropertyBinding<TValue>(value, dispatcher, update).IsRootedChanged);
			}
		}

		public static void BindNativeProperty<TValue>(
			IObservable<bool> isRooted,
			IScheduler dispatcher,
			string name,
			IObservable<TValue> value,
			Action<TValue> update)
		{
			OneWayPropertyBinding<TValue>.Bind(isRooted, dispatcher, value, update, name);
		}

		public static void BindNativeProperty<TValue>(
			this IMountLocation control,
			IScheduler dispatcher,
			string name,
			IObservable<TValue> value,
			Action<TValue> update)
		{
			BindNativeProperty(control.IsRooted, dispatcher, name, value, update);
		}

		public static void BindNativeProperty<TValue>(
			this IMountLocation control,
			IScheduler dispatcher,
			string name,
			Optional<IObservable<TValue>> value,
			Action<TValue> update)
		{
			value.Do(v => BindNativeProperty(control.IsRooted, dispatcher, name, v, update));
		}

		public static void BindNativeProperty<TValue>(
			this IControl control,
			IScheduler dispatcher,
			string name,
			IObservable<TValue> value,
			Action<TValue> update)
		{
			BindNativeProperty(control.IsRooted, dispatcher, name, value, update);
		}
	}
}