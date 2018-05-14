using Uno;
using Uno.Collections;
using Uno.Compiler.ExportTargetInterop;
using Uno.Reflection;
using Outracks.Simulator.Runtime;

namespace Outracks.Simulator.Client
{
	using Bytecode;

	extern(CPLUSPLUS && REFLECTION) internal static class ReflectionExtensions
	{

		static readonly string PropGetPrefix = "get_";
		static readonly string PropSetPrefix = "set_";
		static readonly string EventAdderPrefix = "add_";
		static readonly string EventRemovePrefix = "remove_";
		static readonly TypeMemberName ConstructorName = new TypeMemberName(".ctor");

		public static Type[] GetTypes(this object[] objects)
		{
			if (objects == null)
				return null;

			var types = new Type[objects.Length];
			for (int i = 0; i < objects.Length; i++)
				types[i] = objects[i].GetType();

			return types;
		}

		public static Type[] FindTypes(this TypeName[] typeName)
		{
			var types = new Type[typeName.Length];

			for (int i = 0; i < typeName.Length; i++)
				types[i] = typeName[i].FindType();

			return types;
		}

		public static Type FindType(this TypeName typeName)
		{
			return ReflectionCache.GetType(typeName) ?? Type.GetType(typeName.FullName, true);
		}

		public static CppFunction FindConstructor(this Type type, params Type[] paramTypes)
		{
			return type.FindFunction(ConstructorName, paramTypes);
		}

		public static CppFunction FindPropertyGetter(this Type type, TypeMemberName typeMemberName)
		{
			var getterName = new TypeMemberName(PropGetPrefix + typeMemberName.Name);
			return type.FindFunction(getterName);
		}

		public static CppFunction FindPropertySetter(this Type type, TypeMemberName typeMemberName, Type argType)
		{
			var setterName = new TypeMemberName(PropSetPrefix + typeMemberName.Name);
			return type.FindFunction(setterName, argType);
		}

		public static CppFunction FindEventAddFunction(this Type type, TypeMemberName typeMemberName, object delegateObj)
		{
			var eventAddName = new TypeMemberName(EventAdderPrefix + typeMemberName.Name);
			return type.FindFunction(eventAddName, delegateObj.GetType());
		}

		public static CppFunction FindEventRemoveFunction(this Type type, TypeMemberName typeMemberName, object delegateObj)
		{
			var eventRemoveName = new TypeMemberName(EventRemovePrefix + typeMemberName.Name);
			return type.FindFunction(eventRemoveName, delegateObj.GetType());
		}

		public static CppField FindField(this Type type, TypeMemberName fieldName)
		{
			var fields = ReflectionCache.GetFields(type);
			for (int i = 0; i < fields.Length; i++)
			{
				var f = fields[i];
				if (f.Name == fieldName.Name)
					return f;
			}
			return CppField.Null;
		}

		public static Type[] GetParameterTypes(this Signature methodSignature)
		{
			var parameters = methodSignature.Parameters;
        	var types = new Type[parameters.Count];

        	for (int i = 0; i < parameters.Count; i++)
        	{
        		types[i] = parameters[i].Type.FindType();
        	}

        	return types;
		}

		public static CppFunction FindFunction(this Type type, TypeMemberName memberName, params Type[] paramTypes)
		{
			return FindFunctionOverload(FindFunctionsByName(type, memberName), paramTypes);
		}

		static CppFunction FindFunctionOverload(CppFunction[] functions, Type[] paramTypes)
		{
			for (int i = 0; i < functions.Length; i++)
			{
				if (CheckArgumentTypes(functions[i].ParameterTypes, paramTypes))
					return functions[i];
			}

			return CppFunction.Null;
		}

		static bool CheckArgumentTypes(Type[] paramTypes, Type[] argumentTypes)
		{
			if (paramTypes.Length != argumentTypes.Length)
				return false;

			for (int i = 0; i < paramTypes.Length; i++)
			{
				var param = paramTypes[i];
				var arg = argumentTypes[i];
				if (!arg.IsSubclassOf(param))
					return false;
			}
			return true;
		}


		static CppFunction[] FindFunctionsByName(Type type, TypeMemberName memberName)
		{
			var name = memberName.Name;
			var functions = ReflectionCache.GetFunctions(type);
			var matchingFunctions = new List<CppFunction>();
			for (int i = 0; i < functions.Length; i++)
			{
				if (functions[i].Name == name)
					matchingFunctions.Add(functions[i]);
			}
			return matchingFunctions.ToArray();
		}

	}

}
