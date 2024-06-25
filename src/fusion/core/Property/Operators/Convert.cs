using System;
using System.Reactive.Linq;

namespace Outracks
{
	public static partial class Property
	{
		public static IProperty<TResult> Convert<TArgs, TResult>(
			this IProperty<TArgs> self,
			Func<TArgs, TResult> convert,
			Func<TResult, TArgs> convertBack)
		{
			return new ConvertedProperty<TArgs, TResult>(self, convert, (oldValue, newValue) => convertBack(newValue));
		}

		public static IProperty<TResult> Convert<TArgs, TResult>(
			this IProperty<TArgs> self,
			Func<TArgs, TResult> convert,
			Func<Optional<TArgs>, TResult, TArgs> convertBack)
		{
			return new ConvertedProperty<TArgs, TResult>(self, convert, convertBack);
		}
	}

	class ConvertedProperty<TArgs, TResult> : IProperty<TResult>
	{
		Optional<TArgs> _lastValue;

		readonly IObservable<TResult> _value;
		readonly IProperty<TArgs> _source;
		readonly Func<Optional<TArgs>, TResult, TArgs> _convertBack;

		public ConvertedProperty(
			IProperty<TArgs> source,
			Func<TArgs, TResult> convert,
			Func<Optional<TArgs>, TResult, TArgs> convertBack)
		{
			_source = source;
			_convertBack = convertBack;
			_value = source.Do(value => _lastValue = value).Select(convert);
		}

		public IDisposable Subscribe(IObserver<TResult> observer)
		{
			return _value.Subscribe(observer);
		}

		public IObservable<bool> IsReadOnly
		{
			get { return _source.IsReadOnly; }
		}

		public void Write(TResult value, bool save)
		{
			_source.Write(_convertBack(_lastValue, value), save);
		}
	}
}