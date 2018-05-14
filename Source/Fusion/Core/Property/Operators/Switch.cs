using System;
using System.Reactive.Linq;

namespace Outracks
{
	public static partial class Property
	{
		public static IProperty<T> Switch<T>(this IObservable<IProperty<T>> self)
		{
			return new SwitchProperty<T>(self);
		}
	}

	sealed class SwitchProperty<T> : IProperty<T>
	{
		readonly IObservable<IProperty<T>> _source;
		readonly IObservable<T> _value;
		readonly IObservable<bool> _isReadOnly;

		public SwitchProperty(IObservable<IProperty<T>> source)
		{
			_source = source.Replay(1).RefCount();

			_value = ((IObservable<IObservable<T>>)_source).Switch().DistinctUntilChanged();
			_isReadOnly = _source.Select(p => p.IsReadOnly).Switch().DistinctUntilChanged();
		}

		public IDisposable Subscribe(IObserver<T> observer)
		{
			return _value.Subscribe(observer);
		}

		public IObservable<bool> IsReadOnly
		{
			get { return _isReadOnly; }
		}

		public void Write(T value, bool save)
		{
			_source.Take(1).Subscribe(property => property.Write(value, save));
		}
	}
}