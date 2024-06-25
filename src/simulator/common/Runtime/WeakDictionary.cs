using Uno;
using Uno.IO;
using Uno.UX;
using Uno.Net;
using Uno.Collections;
using Uno.Diagnostics;

namespace Outracks.Simulator.Runtime
{
	public class HashableWeakReference
	{
		readonly Uno.WeakReference<object> _reference;

		public HashableWeakReference(Uno.WeakReference<object> reference)
		{
			_reference = reference;
		}

		public bool TryGetTarget(out object obj)
		{
			return _reference.TryGetTarget(out obj);
		}

		public override int GetHashCode()
		{
			object obj;
			if (!_reference.TryGetTarget(out obj)) return 0;
			return obj.GetHashCode();

		}

		public override bool Equals(object that)
		{
			if (ReferenceEquals(this, that)) return true;
			object a = null;
			object b = null;
			if (!_reference.TryGetTarget(out a))
				return false;
			if (!(that is HashableWeakReference) || !((HashableWeakReference) that)._reference.TryGetTarget(out b))
				return false;
			return a.Equals(b);
		}
	}

	public class WeakDictionary<TKey, TValue>
	{
		readonly Uno.Collections.Dictionary<HashableWeakReference, TValue> _dictionary =
			new Uno.Collections.Dictionary<HashableWeakReference, TValue>();

		public Uno.Collections.IEnumerable<Uno.Collections.KeyValuePair<HashableWeakReference, TValue>> AsEnumerable()
		{
			return _dictionary;
		}

		public void Clear()
		{
			_dictionary.Clear();
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			return _dictionary.TryGetValue(GetKey(key), out value);
		}

		public bool Remove(HashableWeakReference key)
		{
			return _dictionary.Remove(key);
		}

		public TValue this[TKey key]
		{
			get { return _dictionary[GetKey(key)]; }
			set { _dictionary[GetKey(key)] = value; }
		}

		static HashableWeakReference GetKey(TKey obj)
		{
			return new HashableWeakReference(new Uno.WeakReference<object>(obj));
		}
	}
}