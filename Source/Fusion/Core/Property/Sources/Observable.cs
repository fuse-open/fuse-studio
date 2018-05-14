using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Outracks
{
	public static partial class Property
	{
		/// <summary>
		/// This overload exists as a NOP to avoid Property.Create().AsProperty() becoming read-only due to ending up in the IObservable overload
		/// </summary>
		public static IProperty<T> AsProperty<T>(this IProperty<T> value)
		{
			return value;
		}
		
		public static IProperty<T> AsProperty<T>(this IObservable<T> value)
		{
			return new ObservableProperty<T>(value);
		}

		public static IProperty<T> AsProperty<T>(
			this ISubject<T, T> self, 
			IObservable<bool> isReadOnly = null)
		{
			return new WritableObservableProperty<T>(
				self,
				(v, save) => self.OnNext(v),
				isReadOnly ?? Observable.Return(false));
		}

		public static IProperty<T> AsProperty<T>(
			this IObservable<T> value,
			Action<T, bool> write,
			IObservable<bool> isReadOnly = null)
		{
			return new WritableObservableProperty<T>(
				value,
				write,
				isReadOnly ?? Observable.Return(false));
		}
	}

	sealed class ObservableProperty<T> : IProperty<T>
	{
		readonly IObservable<T> _observable;

		public ObservableProperty(IObservable<T> observable)
		{
			_observable = observable;
		}

		public IDisposable Subscribe(IObserver<T> observer)
		{
			return _observable.Subscribe(observer);
		}

		public IObservable<bool> IsReadOnly
		{
			get { return Observable.Return(true); }
		}

		public void Write(T value, bool save)
		{
			// NOP
		}
	}

	public class WritableObservableProperty<T> : IProperty<T>
	{
		readonly IObservable<T> _value;
		readonly IObservable<bool> _isReadOnly; 
		readonly Action<T, bool> _write;

		public WritableObservableProperty(IObservable<T> value, Action<T, bool> write, IObservable<bool> isReadOnly)
		{
			_value = value;
			_isReadOnly = isReadOnly;
			_write = write;
		}

		public IObservable<bool> IsReadOnly
		{
			get { return _isReadOnly; }
		}

		public void Write(T value, bool save)
		{
			_write(value, save);
		}

		public IDisposable Subscribe(IObserver<T> observer)
		{
			return _value.Subscribe(observer);
		}
	}

}