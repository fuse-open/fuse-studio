using Outracks.Simulator.Bytecode;
using Outracks.Simulator.Runtime;
using Uno;
using Uno.Compiler.ExportTargetInterop;

namespace Outracks.Simulator.Client
{
	[DotNetType]
	public class DotNetReflection : IReflection
	{
		public static DotNetReflection Load(string outputDir)
		{
			throw new Uno.Exception("Not DotNet backend");
		}

		public object CallDynamic(object instance, string methodName, params object[] arguments) { throw new Exception(); }
		public object CallStatic(string typeName, string methodName, params object[] arguments) { throw new Exception(); }

		public void SetPropertyValue(object instance, string propertyName, object value) { throw new Exception(); }
		public object GetPropertyValue(object instance, string propertyName) { throw new Exception(); }

		public object GetStaticPropertyOrFieldValue(string typeName, string memberName) { throw new Exception(); }

		public object CreateDelegate(object instance, string methodName, string[] methodArgumentTypes, string delegateTypeName) { throw new Exception(); }

		public void AddEventHandler(object instance, string member, object handlerDelegate) { throw new Exception(); }
		public void RemoveEventHandler(object instance, string member, object handlerDelegate) { throw new Exception(); }

		public object Instantiate(string typeName, params object[] args) { throw new Exception(); }
		public bool IsSubtype(object obj, string typeName) { throw new Exception(); }
		public bool IsType(object obj, string typeName) { throw new Exception(); }
		public object GetEnumValue(string enumType, string valueName) { throw new Exception(); }
	}


	// Since DotNetReflection is implementing the .Net build of IReflection we have to wrap it in an uno class
	public class DotNetReflectionWrapper : IReflection
	{
		public DotNetReflection _reflection { get; set; }

		public DotNetReflectionWrapper(DotNetReflection reflection)
		{
			_reflection = reflection;
		}

		public object CallDynamic(object instance, string methodName, params object[] arguments) 
		{
			return _reflection.CallDynamic(instance, methodName, arguments); 
		}

		public object CallStatic(string typeName, string methodName, params object[] arguments)
		{
			return _reflection.CallStatic(typeName, methodName, arguments);
		}

		public void SetPropertyValue(object instance, string propertyName, object value)
		{
			_reflection.SetPropertyValue(instance, propertyName, value);
		}
		public object GetPropertyValue(object instance, string propertyName)
		{
			return _reflection.GetPropertyValue(instance, propertyName);
		}

		public object GetStaticPropertyOrFieldValue(string typeName, string memberName)
		{
			return _reflection.GetStaticPropertyOrFieldValue(typeName, memberName);
		}

		public object CreateDelegate(object instance, string methodName, string[] methodArgumentTypes, string delegateTypeName)
		{
			return _reflection.CreateDelegate(instance, methodName, methodArgumentTypes, delegateTypeName);
		}

		public void AddEventHandler(object instance, string member, object handlerDelegate)
		{
			_reflection.AddEventHandler(instance, member, handlerDelegate);
		}
		public void RemoveEventHandler(object instance, string member, object handlerDelegate)
		{
			_reflection.RemoveEventHandler(instance, member, handlerDelegate);
		}

		public object Instantiate(string typeName, params object[] args)
		{
			return _reflection.Instantiate(typeName, args);
		}
		public bool IsSubtype(object obj, string typeName)
		{
			return _reflection.IsSubtype(obj, typeName);
		}
		public bool IsType(object obj, string typeName)
		{
			return _reflection.IsType(obj, typeName);
		}
		public object GetEnumValue(string enumType, string valueName)
		{
			return _reflection.GetEnumValue(enumType, valueName);
		}
	}
}