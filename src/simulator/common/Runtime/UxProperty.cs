using Uno;
using Uno.UX;
using Uno.Collections;

namespace Outracks.Simulator.Runtime
{
	public class UxProperty<T> : Property<T>
	{
		readonly Action<object, object, object> _setter;
		readonly Func<object, object> _getter;
		readonly Uno.UX.PropertyObject _obj;
		readonly bool _supportsOriginSetter;

		public UxProperty(
			Action<object, object, object> setter,
			Func<object, object> getter,
			Uno.UX.PropertyObject obj,
			string name,
			bool supportsOriginSetter)
			: base(new Uno.UX.Selector(name))
		{
			_setter = setter;
			_getter = getter;
			_obj = obj;
			_supportsOriginSetter = supportsOriginSetter;
		}

		public override Uno.UX.PropertyObject Object
		{
			get { return _obj; }
		}

		public override bool SupportsOriginSetter
		{
			get { return _supportsOriginSetter; }
		}

		extern(DESIGNER)
		public override void Set(Uno.UX.PropertyObject obj, T value, Uno.UX.IPropertyListener origin)
		{
			_setter(obj, value, origin);
		}

		extern(DESIGNER)
		public override T Get(PropertyObject obj)
		{
			var res = _getter(obj);
			return res == null ? (typeof(T) == typeof(string) ? (T)(object)"" : default(T)) : (T)res;
		}
	}
}
