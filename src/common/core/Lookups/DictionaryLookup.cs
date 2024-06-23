using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Outracks
{
	class DictionaryLookup<TKey, TValue, TValues> : ILookup<TKey, TValue> where TValues : IEnumerable<TValue>
	{
		readonly IDictionary<TKey, TValues> _dictionary;

		readonly IEnumerable<IGrouping<TKey, TValue>> _groups;

		public DictionaryLookup(IDictionary<TKey, TValues> dictionary)
		{
			_dictionary = dictionary;
			_groups = _dictionary.Select(k => (IGrouping<TKey, TValue>)new Grouping<TKey, TValues>(k.Key, new[] { k.Value }));
		}

		public IEnumerator<IGrouping<TKey, TValue>> GetEnumerator()
		{
			return _groups.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		public bool Contains(TKey key)
		{
			return _dictionary.ContainsKey(key);
		}

		public int Count
		{
			get { return _dictionary.Count; }
		}

		public IEnumerable<TValue> this[TKey key]
		{
			get
			{
				TValues value;
				if (_dictionary.TryGetValue(key, out value))
					return value;

				return Enumerable.Empty<TValue>();
			}
		}
	}
}