using System;
using Outracks.Fusion;

namespace Outracks
{
	public class SubscriberTrackingProperty<T> : IProperty<T>
	{
		readonly IProperty<T> _property;
		readonly string _name;
		public SubscriberTrackingProperty(IProperty<T> property, string name)
		{
			_property = property;
			_name = name;
		}

		public IObservable<T> Value
		{
			get { return Track((IObservable<T>)_property, "Value"); }
		}

		public IObservable<bool> IsReadOnly
		{
			get { return Track(_property.IsReadOnly, "IsReadOnly"); }
		}

		public void Write(T value, bool save)
		{
			_property.Write(value, save);
		}

		IObservable<TU> Track<TU>(IObservable<TU> observable, string name)
		{
			return new SubscriberTrackingObservable<TU>(observable, count =>
				Console.WriteLine(_name + "." + name + " has " + count + " observers"));
		}

		Command Track(Command cmd, string name)
		{
			return Command.Create(new SubscriberTrackingObservable<Optional<Action>>(cmd.Action, count =>
				Console.WriteLine(_name + "." + name + " has " + count + " observers")));
		}

		public IDisposable Subscribe(IObserver<T> observer)
		{
			return _property.Subscribe(observer);
		}
	}
}