using System;
using System.Collections.Generic;

namespace Outracks.Simulator.Client
{
	public class MemoizingTypeMap : ITypeMap
	{
		readonly IDictionary<string, Type> _cache = new Dictionary<string, Type>();
		readonly ITypeMap _typeMap;
		public MemoizingTypeMap(ITypeMap typeMap)
		{
			_typeMap = typeMap;
		}

		public Type ResolveType(string typeName)
		{
			Type type;
			if (!_cache.TryGetValue(typeName, out type))
				_cache[typeName] = type = _typeMap.ResolveType(typeName);
			return type;
		}
	}
}