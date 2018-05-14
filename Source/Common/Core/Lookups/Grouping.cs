using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Outracks
{
	public class Grouping<TKey, TValue> : IGrouping<TKey, TValue>
	{
		readonly IEnumerable<TValue> _values;

		public TKey Key { get; private set; }

		public Grouping(TKey key, IEnumerable<TValue> values)
		{
			Key = key;
			_values = values;
		}

		public IEnumerator<TValue> GetEnumerator()
		{
			return _values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
	}
}