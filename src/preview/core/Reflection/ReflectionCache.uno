using Uno;
using Uno.Collections;
using Uno.Reflection;
using Uno.Compiler.ExportTargetInterop;
using Outracks.Simulator.Runtime;

namespace Outracks.Simulator.Client
{
	using Bytecode;

	extern(CPLUSPLUS && REFLECTION) internal static class ReflectionCache
    {
    	static readonly Dictionary<Type, CppFunction[]> _functionCache;
    	static readonly Dictionary<Type, CppField[]> _fieldCache;
    	static readonly List<Dictionary<TypeName, Type>> _typeCache;

    	static ReflectionCache()
    	{
    		_functionCache = new Dictionary<Type, CppFunction[]>();
    		_fieldCache = new Dictionary<Type, CppField[]>();
    		_typeCache = new List<Dictionary<TypeName, Type>>();
    	}

    	public static CppFunction[] GetFunctions(Type type)
    	{
    		if (_functionCache.ContainsKey(type))
    			return _functionCache[type];

    		var functions = new List<CppFunction>();
    		var t = type;

    		while (t != null)
    		{
    			functions.AddRange(CppReflection.GetFunctions(t));
    			t = t.BaseType;
    		}

    		var array = functions.ToArray();
    		_functionCache.Add(type, array);
    		return array;
    	}

    	public static CppField[] GetFields(Type type)
    	{
    		if (_fieldCache.ContainsKey(type))
    			return _fieldCache[type];

    		var fields = new List<CppField>();
    		var t = type;

    		while (t != null)
    		{
    			fields.AddRange(CppReflection.GetFields(t));
    			t = t.BaseType;
    		}

    		var array = fields.ToArray();
    		_fieldCache.Add(type, array);
    		return array;
    	}

    	public static Type GetType(TypeName typeName)
    	{
    		foreach (var dict in _typeCache)
	    		if (dict.ContainsKey(typeName))
	    			return dict[typeName];

    		return null;
    	}

    	public static void AddToTypeCache(Dictionary<TypeName, Type> typeDictionary)
    	{
    		_typeCache.Add(typeDictionary);
    	}
    }
}
