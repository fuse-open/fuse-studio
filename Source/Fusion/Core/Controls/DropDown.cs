using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Outracks.Fusion
{
	public static class DropDown
	{
		public static IControl Create<T>(IProperty<T> selection = null, IObservable<IEnumerable<T>> values = null, bool nativeLook = true)
		{
			selection = selection ?? Property.Create(default(T));
			values = values ?? Observable.Return(Enumerable.Empty<T>());
			return Create(
				selection.Convert(
					convert: v => (object)v,
					convertBack: v => (T)v),
				values.SelectPerElement(v => (object)v),
				nativeLook);
		}

		public static IControl Create(IProperty<object> selection = null, IObservable<IEnumerable<object>> values = null, bool nativeLook = true)
		{
			return Implementation.Factory(
				selection ?? Property.Create(new object()), 
				values ?? Observable.Return(Enumerable.Empty<object>()),
				nativeLook);
		}

		public static class Implementation
		{
			public static Func<IProperty<object>, IObservable<IEnumerable<object>>, bool, IControl> Factory;
		}
	}
}