using Uno;
using Uno.Text;
using Uno.Collections;
using Uno.Compiler.ExportTargetInterop;
using Uno.Reflection;
using Outracks.Simulator.Runtime;

namespace Outracks.Simulator.Client
{
	using Bytecode;

	public interface ITypeMap
	{
		Type ResolveType(string typeName);
	}

	extern(CPLUSPLUS && REFLECTION)
	public class SimpleTypeMap : ITypeMap
	{
		// TODO: This mapping should be done offline
		Dictionary<string, Type> _builtins = new Dictionary<string, Type>
		{
			{ "object", typeof(object) },
			{ "string", typeof(string) },
			{ "texture2D", typeof(texture2D) },
			{ "textureCube", typeof(textureCube) },
			{ "bool", typeof(bool) },
			{ "char", typeof(char) },
			{ "byte", typeof(byte) },
			{ "byte2", typeof(byte2) },
			{ "byte4", typeof(byte4) },
			{ "sbyte", typeof(sbyte) },
			{ "sbyte2", typeof(sbyte2) },
			{ "sbyte4", typeof(sbyte4) },
			{ "short", typeof(short) },
			{ "short2", typeof(short2) },
			{ "short4", typeof(short4) },
			{ "ushort", typeof(ushort) },
			{ "ushort2", typeof(ushort2) },
			{ "ushort4", typeof(ushort4) },
			{ "int", typeof(int) },
			{ "int2", typeof(int2) },
			{ "int3", typeof(int3) },
			{ "int4", typeof(int4) },
			{ "uint", typeof(uint) },
			{ "long", typeof(long) },
			{ "ulong", typeof(ulong) },
			{ "float", typeof(float) },
			{ "float2", typeof(float2) },
			{ "float3", typeof(float3) },
			{ "float4", typeof(float4) },
			{ "float2x2", typeof(float2x2) },
			{ "float3x3", typeof(float3x3) },
			{ "float4x4", typeof(float4x4) },
			{ "double", typeof(double) },
		};
	
		public Type ResolveType(string typeName)
		{
			return ResolveType(TypeName.Parse(typeName));
		}

		Type ResolveType(TypeName typeName)
		{
			var t = Type.GetType(typeName.WithGenericSuffix.FullName);

			if (t == null && !_builtins.TryGetValue(typeName.FullName, out t))
				throw new InvalidOperationException("'" + typeName + "' was not found");

			if (typeName.IsParameterizedGenericType)
				t = t.MakeGenericType(typeName.GenericArgumentsRecursively.Select((Func<TypeName, Type>)ResolveType).ToArray());

			if (t.ContainsGenericParameters)
				throw new InvalidOperationException("'" + typeName + "' is not a closed type");

			return t;
		}
	}

	interface IProperty
	{
		Type DeclaringType { get; }
		IFunction GetMethod { get; }
		IFunction SetMethod { get; }
	}

	interface IEvent
	{
		Type DeclaringType { get; }
		IFunction AddMethod { get; }
		IFunction RemoveMethod { get; }
	}

	extern(CPLUSPLUS && REFLECTION)
	public sealed class NativeReflection : IReflection
	{
		readonly ITypeMap _typeMap;

		public NativeReflection(ITypeMap typeMap)
		{
			_typeMap = typeMap;
		}

		public object GetPropertyValue(object obj, string propertyName)
		{
			var prop = ResolveProperty(obj, propertyName, null);
			var getter = prop.GetMethod;
			if (getter == null)
				throw new InvalidOperationException("Property '" + prop.DeclaringType + "." + propertyName + "' has no getter");

			return prop.GetMethod.TryInvoke(obj, new object[0]);
		}

		public void SetPropertyValue(object obj, string propertyName, object value)
		{
			var prop = ResolveProperty(obj, propertyName, value != null ? value.GetType() : null);
			var setter = prop.SetMethod;
			if (setter == null)
				throw new InvalidOperationException("Property '" + prop.DeclaringType + "." + propertyName + "' has no setter");

			setter.TryInvoke(obj, new[] { value });
		}

		IProperty ResolveProperty(object obj, string propertyName, Type propertyType)
		{
			if (obj == null)
				throw new ArgumentNullException("obj");

			var objType = obj.GetType();

			try
			{
				return objType.GetProperty(propertyName, propertyType);
			}
			catch (Exception e)
			{
				throw new MemberNotFound(objType.FullName, propertyName);
			}

		}

		public object CreateDelegate(object instance, string methodName, string[] methodArgumentTypes, string delegateTypeName)
		{
			var type = instance.GetType();

			var method = methodArgumentTypes != null
				? type.GetMethod(methodName, methodArgumentTypes.Select<string, Type>(_typeMap.ResolveType).ToArray())
				: type.GetMethod(methodName);

			if (method == null)
				throw new MemberNotFound(type.FullName, methodName);

			return method.CreateDelegate(_typeMap.ResolveType(delegateTypeName), instance);
		}


		public void AddEventHandler(object instance, string member, object handlerDelegate)
		{
			var type = instance.GetType();

			var theEvent = type.GetEvent(member, handlerDelegate.GetType());
			if (theEvent == null)
				throw new MemberNotFound(type.FullName, member);

			var adder = theEvent.AddMethod;
			if (adder == null)
				throw new InvalidOperationException("Event '" + theEvent.DeclaringType + "." + member + "' has no add method");

			adder.TryInvoke(instance, new []{ handlerDelegate });
		}

		public void RemoveEventHandler(object instance, string member, object handlerDelegate)
		{
			var type = instance.GetType();

			var theEvent = type.GetEvent(member, handlerDelegate.GetType());
			if (theEvent == null)
				throw new MemberNotFound(type.FullName, member);

			var remover = theEvent.AddMethod;
			if (remover == null)
				throw new InvalidOperationException("Event '" + theEvent.DeclaringType + "." + member + "' has no remover method");

			remover.TryInvoke(instance, new []{ handlerDelegate });
		}

		public object CallDynamic(object obj, string methodName, params object[] arguments)
		{
			var type = obj.GetType();

			return type.FindUnambiguousMethod(methodName, arguments).TryInvoke(obj, arguments);
		}


		public object Instantiate(string typeName, params object[] args)
		{
			return CallStatic(typeName, ".ctor", args);
		}

		public object CallStatic(string typeName, string methodName, params object[] arguments)
		{
			var type = _typeMap.ResolveType(typeName);
			if (type == null)
				throw new TypeNotFound(typeName);

			return type.FindUnambiguousMethod(methodName, arguments).TryInvoke(null, arguments);
		}

		public void SetStaticField(string typeName, string fieldName, object value)
		{
			var type = _typeMap.ResolveType(typeName);
			if (type == null)
				throw new TypeNotFound(typeName);

			var field = type.GetField(fieldName);//.Name, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
			if (field == null)
				throw new MemberNotFound(typeName, fieldName);

			field.SetValue(null, value);
		}

		public object GetStaticPropertyOrFieldValue(string typeName, string memberName)
		{
			var type = _typeMap.ResolveType(typeName);
			if (type == null)
				throw new TypeNotFound(typeName);

			var prop = type.GetProperty(memberName, null);
		    if (prop != null && prop.GetMethod != null)
		        return prop.GetMethod.Invoke(null, new object[0]);

			var field = type.GetField(memberName);
			if (field != null)
				return field.GetValue(null);

			throw new MemberNotFound(typeName, memberName);
		}

		public bool IsSubtype(object obj, string typeName)
		{
			var type = _typeMap.ResolveType(typeName);
			if (type == null) return false;
			return type.IsInstanceOfType(obj);
		}

		public bool IsType(object obj, string typeName)
		{
			return obj.GetType().FullName == typeName;
		}

		public object GetEnumValue(string enumType, string valueName)
		{
			var type = _typeMap.ResolveType(enumType);
			return Enum.Parse(type, valueName);
		}

	}

	extern(CPLUSPLUS && REFLECTION)
	static class TryInvokeExtension
	{
		public static IFunction FindUnambiguousMethod(this Type type, string methodName, object[] arguments)
		{
			var methods = new List<IFunction>();

			for (var t = type; t != null; t = t.BaseType)
			{
				foreach (var m in t.GetMethods())
				{
					if (m.Name == methodName && m.ParametersMatch(arguments))
						methods.Add(m);

					if (methods.Count >= 2)
						break;
				}

				if (methods.Count > 0)
					break;
			}

			if (methods.Count == 0)
				throw new MemberNotFound(type.FullName, methodName);

			if (methods.Count > 1)
				throw new UnambiguousMethodNotFound(type.FullName, methodName);

			return methods[0];
		}

		public static bool ParametersMatch(this IFunction m, object[] arguments)
		{
			var prms = m.ParameterTypes;
			if (prms.Length != arguments.Length)
				return false;

			for (int i = 0; i < prms.Length; i++)
			{
				var param = prms[i];
				var arg = arguments[i];

				if (arg == null)
				{
					// all non-value types can accept null
					if (param.IsValueType)
						return false;
				}
				else
				{
					if (!param.IsInstanceOfType(arg))
						return false;
				}
			}

			return true;
		}

		public static IFunction GetMethod(this Type type, string name, params Type[] parameterTypes)
		{
		    for (var t = type; t != null; t = t.BaseType)
		    {
		        foreach (var f in t.GetMethods())
		        {
		            if (f.Name != name ||
                        f.ParameterTypes.Length != parameterTypes.Length)
		                continue;

		            var found = true;
		            for (int i = 0; i < parameterTypes.Length; i++)
		            {
		                if (parameterTypes[i] != null && !parameterTypes[i].IsSubclassOf(f.ParameterTypes[i]))
		                {
		                    found = false;
		                    break;
		                }
		            }

		            if (found)
		                return f;
		        }
		    }

		    return null;
		}

		public static CppFunction[] GetMethods(this Type type)
		{
			return CppReflection.GetFunctions(type);
		}

		public static IProperty GetProperty(this Type type, string name, Type propertyType)
		{
			return new CppProperty(type, propertyType, name);
		}

		public static IField GetField(this Type type, string name)
		{
		    if (type == null)
		        return null;
		    var f = CppReflection.FindField(type, name);
			return f.IsNull ? GetField(type.BaseType, name) : (IField)f;
		}

		public static IEvent GetEvent(this Type type, string name, Type propertyType)
		{
			return new CppEvent(type, propertyType, name);
		}

		public static bool IsInstanceOfType(this Type type, object obj)
		{
			return obj.GetType().IsSubclassOf(type);
		}

		public static object TryInvoke(this IFunction mi, object obj, params object[] value)
		{
			return mi.Invoke(obj, value);
		}

	}

	extern(CPLUSPLUS && REFLECTION)
	class CppEvent : IEvent
	{
		readonly Type _declaringType;
		readonly Type _type;
		readonly string _name;

		public CppEvent(Type declaringType, Type type, string name)
		{
			_declaringType = declaringType;
			_type = type;
			_name = name;
		}

	    public Type DeclaringType
	    {
	        get { return _declaringType; }
	    }

		public IFunction AddMethod
		{
			get { return _declaringType.GetMethod("add_" + _name, _type); }
		}

		public IFunction RemoveMethod
		{
			get { return _declaringType.GetMethod("remove_" + _name, _type); }
		}
	}

	extern(CPLUSPLUS && REFLECTION)
	class CppProperty : IProperty
	{
		readonly Type _declaringType;
		readonly Type _type;
		readonly string _name;

		public CppProperty(Type declaringType, Type type, string name)
		{
			_declaringType = declaringType;
			_type = type;
			_name = name;
		}

	    public Type DeclaringType
	    {
	        get { return _declaringType; }
	    }

		public IFunction GetMethod
		{
			get { return _declaringType.GetMethod("get_" + _name); }
		}

		public IFunction SetMethod
		{
			get { return _declaringType.GetMethod("set_" + _name, _type); }
		}
	}

}
