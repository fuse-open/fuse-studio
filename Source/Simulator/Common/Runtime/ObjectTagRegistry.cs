using System;
using Uno;
using Uno.IO;
using Uno.Net;
using Uno.UX;
using Uno.Collections;
using Uno.Diagnostics;

namespace Outracks.Simulator.Runtime
{
	using Protocol;

	public static class ObjectTagRegistry
	{
		static int _visualId = 0;
		static readonly Dictionary<string, List<object>> _tagToObjects = new Dictionary<string, List<object>>();

		public static Dictionary<string, List<object>> TagToObjects { get { return _tagToObjects; } }
		public static Dictionary<object, string> ObjectToTag = new Dictionary<object,string>();
		
		public static event Action<object, int, string> OnObjectTagRegistered;

		
		public static void Clear()
		{
			TagToObjects.Clear();
			ObjectToTag.Clear();
		}

		public static object RegisterObjectTag(object obj, string tagHash)
		{
			List<object> objects = null;
			if (!_tagToObjects.TryGetValue(tagHash, out objects))
				_tagToObjects[tagHash] = objects = new List<object>();

			objects.Add(obj);

			ObjectToTag[obj] = tagHash;

			if (OnObjectTagRegistered != null)
				OnObjectTagRegistered(obj, ++_visualId, tagHash);

			foreach (var each in _eaches)
				each.OnObjectCreated(tagHash, obj);


			return obj;
		}
		
		public static void DisposeAndUnregister(string tag)
		{
			var objects = GetObjectsWithTag(tag);
			_tagToObjects.Remove(tag);

			foreach (var obj in objects)
			{
				try
				{
					var disposable = obj as IDisposable;
					if (disposable != null)
						disposable.Dispose();
				}
				catch (Exception e)
				{
					debug_log e.ToString();
				}
			}
		}

		public static void TryExecuteOnObjectsWithTag(string tag, Action<object> action)
		{
			foreach (var obj in GetObjectsWithTag(tag))
			{
				try
				{
					action(obj);
				}
				catch (Exception e)
				{
					debug_log e.ToString();
				}
			}
		}

		public static object GetFirstObjectWithTag(string tag)
		{
			foreach (var obj in GetObjectsWithTag(tag))
			{
				try
				{
					return obj;
				}
				catch (Exception e)
				{
					debug_log
					e.ToString();
				}
			}
			return null;
		}

		public static IEnumerable<object> GetObjectsWithTag(string tag)
		{
			List<object> objects = null;
			if (_tagToObjects.TryGetValue(tag, out objects))
				return objects;

			return new object[0];
		}


		public static string GetTagHash(object obj)
		{
			string tagHash = null;
			ObjectToTag.TryGetValue(obj, out tagHash);
			return tagHash;
		}

		public static IDisposable Each(string tag, Func<object, object> func)
		{
			var each = new CurrentEach
			{
				_id = tag,
				_func = func,
			};

			foreach (var obj in GetObjectsWithTag(tag))
			{
				try
				{
					each.OnObjectCreated(obj);
				}
				catch (Exception e)
				{
					debug_log e.ToString();
				}
			}

			_eaches.Add(each);

			return each;
		}

		static List<CurrentEach> _eaches = new List<CurrentEach>();

		class CurrentEach : IDisposable
		{
			public string _id;
			public Func<object, object> _func;
			
			readonly List<IDisposable> _garbage = new List<IDisposable>();
			
			public void Dispose()
			{
				_eaches.Remove(this);
				foreach (var disposable in _garbage)
					disposable.Dispose();
			}

			public void OnObjectCreated(string id, object obj)
			{
				if (id != _id)
					return;

				OnObjectCreated(obj);
			}

			public void OnObjectCreated(object obj)
			{
				var disposable = _func(obj) as IDisposable;
				if (disposable != null)
					_garbage.Add(disposable);
			}
		}

	}
}
