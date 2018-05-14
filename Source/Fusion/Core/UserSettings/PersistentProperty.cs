using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Outracks.Fusion
{
	class PersistentProperty<T> : IProperty<Optional<T>>
	{
		readonly PersistentSettings _settings;

		readonly string _name;
		readonly BehaviorSubject<Optional<T>> _value;

		public PersistentProperty( PersistentSettings settings, string name, Optional<T> value)
		{
			_settings = settings;
			_name = name;
			_value = new BehaviorSubject<Optional<T>>(value);
		}

		public IDisposable Subscribe(IObserver<Optional<T>> observer)
		{
			return _value.Subscribe(observer);
		}

		public IObservable<bool> IsReadOnly
		{
			get { return Observable.Return(false); }
		}

		public void Write(Optional<T> value, bool save)
		{
			_value.OnNext(value);
			_settings.Write(_name, value);
			if (save) _settings.SaveSettings();
		}
	}
}