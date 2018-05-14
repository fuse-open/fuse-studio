using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using Outracks.Simulator.Runtime;

namespace Outracks.Simulator.Client
{
	public interface ITypeMap
	{
		Type ResolveType(string typeName);
	}

	public class DotNetReflection : IReflection
	{
		public static DotNetReflection Load(string outputDir)
		{
			return Load(outputDir, DotNetBuild.LoadMetadata(outputDir));
		}

		public static DotNetReflection Load(string outputDir, DotNetBuild metadata)
		{
			SetUnmanagedLibraryDirectory(outputDir);

			return new DotNetReflection(
				new MemoizingTypeMap(
					new TypeMap(
						new TypeAliasNameResolver2(metadata.GeneratedTypeNames),
						Assembly.LoadFrom(Path.Combine(outputDir, metadata.Assembly)),
						typeof (string).Assembly,
						typeof (DotNetReflection).Assembly,
						typeof (Uno.Application).Assembly
					)
				)
			);
		}

		static void SetUnmanagedLibraryDirectory(string dir)
		{
			// We need a workaround for Windows
			if (Path.DirectorySeparatorChar == '\\')
			{
				// Problem #1
				// - Uno.Native.dll & related unmanaged libraries are copied by 'uno build' to the output directory of Outracks.Simulator.Common.Uno.unoproj
				// - Fuse.Preview.Service.csproj depends on Outracks.Simulator.Common.Uno.dll, and indirectly on Uno.Native.dll, but without copying the unmanaged libraries
				// - At runtime, Uno.Native.dll lazy loads it's unmanaged libraries, but a since a diffent Uno.Native.dll than we think is used, it doesn't find the unmanaged libraries (on Windows)

				// We can make Windows find the correct unmanaged libraries using this system call.
				SetDllDirectory(dir);
			}

			// Problem #2
			// - Uno.Native.dll uses a Initialize(string dllDir) method to initialize itself
			// - When lazy initializing, dllDir is detected automatically based on the location of the calling assembly
			// - Since Fuse Studio also has a copy of the calling assembly (UnoCore.dll), there are no unmanaged libraries at this location either

			// We can solve this by calling Initialize(string dllDir) ourselves.

			// Problem #3
			// - Uno.Native.dll and related unmanaged libraries are considered legacy code, and currently in the process of being removed
			// - When removed, Uno.Native.dll and Initialize(string dllDir) will no longer exist, so the following code would stop working

			// We can solve this by loading Uno.Native.dll using reflection and just early-out if things don't exist.

			var asm = Assembly.LoadFrom(Path.Combine(dir, "Uno.Native.dll"));
			if (asm == null) return;

			var type = asm.GetType("Uno.Native.NativeLib");
			if (type == null) return;

			var method = type.GetMethod("Initialize", new[] { typeof(string) });
			if (method == null) return;

			method.Invoke(null, new object[] { dir });

			// - The above code is eventually safe to remove, but SetDllDirectory() will likely still be needed because of the 'copy of UnoCore.dll'-fluke
			// - In the future, UnoHost will likely be moved completely to Uno where we have more control over this, so Fuse Studio don't have to deal with such things.
		}

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		static extern bool SetDllDirectory(string path);
		readonly ITypeMap _typeMap;

		public DotNetReflection(ITypeMap typeMap)
		{
			_typeMap = typeMap;
		}
	
		public object GetPropertyValue(object obj, string propertyName)
		{
			var prop = ResolveProperty(obj, propertyName);
			var getter = prop.GetMethod;
			if (getter == null)
				throw new InvalidOperationException("Property `" + propertyName + "` has no getter");

			return prop.GetMethod.TryInvoke(obj, new object[0]);
		}

		public void SetPropertyValue(object obj, string propertyName, object value)
		{
			var prop = ResolveProperty(obj, propertyName);
			var setter = prop.SetMethod;
			if (setter == null) 
				throw new InvalidOperationException("Property `" + propertyName + "` has no setter");
			
			setter.TryInvoke(obj, new[] { value });
		}

		static PropertyInfo ResolveProperty(object obj, string propertyName)
		{
			if (obj == null)
				throw new ArgumentNullException("obj");

			var objType = obj.GetType();
			var props = objType.GetProperties();

			for (var type = objType; type != null; type = type.BaseType)
			{
				var p = props.FirstOrDefault(x => x.DeclaringType == type && x.Name == propertyName);
				if (p != null)
					return p;
			}

			throw new MemberNotFound(objType.FullName, propertyName);
		}

		public object CreateDelegate(object instance, string methodName, string[] methodArgumentTypes, string delegateTypeName)
		{
			var type = instance.GetType();

			var method = methodArgumentTypes != null
				? type.GetMethod(
					methodName,
					methodArgumentTypes.Select(_typeMap.ResolveType).ToArray())
				: type.GetMethod(methodName);

			if (method == null)
				throw new MemberNotFound(type.FullName, methodName);

			return method.CreateDelegate(_typeMap.ResolveType(delegateTypeName), instance);
		}

		public void AddEventHandler(object instance, string member, object handlerDelegate)
		{
			var type = instance.GetType();
			var theEvent = type.GetEvent(member);
			if (theEvent == null)
				throw new MemberNotFound(type.FullName, member);

			theEvent.AddEventHandler(instance, ((System.Delegate)handlerDelegate).Cast(theEvent.EventHandlerType));
		}

		public void RemoveEventHandler(object instance, string member, object handlerDelegate)
		{
			var type = instance.GetType();
			var theEvent = type.GetEvent(member);
			if (theEvent == null)
				throw new MemberNotFound(type.FullName, member);

			theEvent.RemoveEventHandler(instance, ((System.Delegate)handlerDelegate).Cast(theEvent.EventHandlerType));
		}

		public object CallDynamic(object obj, string methodName, params object[] arguments)
		{
			var type = obj.GetType();
			
			return FindUnambiguousMethod(type, methodName, arguments).TryInvoke(obj, arguments);
		}

		public object CallStatic(string typeName, string methodName, params object[] arguments)
		{
			var type = _typeMap.ResolveType(typeName);
			if (type == null)
				throw new TypeNotFound(typeName);

			return FindUnambiguousMethod(type, methodName, arguments).TryInvoke(null, arguments);
		}

		static MethodInfo FindUnambiguousMethod(Type type, string methodName, object[] arguments)
		{
			var methods = type.GetMethods()
				.Where(m => m.Name == methodName)
				.Where(m => m.GetParameters().ParametersMatch(arguments))
				.Take(2)
				.ToArray();

			if (methods.Length == 0)
				throw new MemberNotFound(type.FullName, methodName);

			if (methods.Length > 1)
				throw new UnambiguousMethodNotFound(type.FullName, methodName);

			return methods[0];
		}

		public object CallStatic(string typeName, string methodName, string[] genericArguments, object[] arguments)
		{
			var type = _typeMap.ResolveType(typeName);
			if (type == null)
				throw new TypeNotFound(typeName);

			var method = type.GetMethod(methodName).MakeGenericMethod(genericArguments.Select(t => _typeMap.ResolveType(t)).ToArray());
			if (method == null)
				throw new MemberNotFound(type.FullName, methodName);

			return method.TryInvoke(null, arguments);
		}

		public void SetStaticField(string typeName, string fieldName, object value)
		{
			var type = _typeMap.ResolveType(typeName);
			if (type == null)
				throw new TypeNotFound(typeName);

			var field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
			if (field == null)
				throw new MemberNotFound(typeName, fieldName);

			TryInvokeExtension.Try(
				() =>
				{
					field.SetValue(null, value);
					return null;
				});
		}

		public object GetStaticPropertyOrFieldValue(string typeName, string memberName)
		{
			var type = _typeMap.ResolveType(typeName);
			if (type == null)
				throw new TypeNotFound(typeName);

			var prop = type.GetProperty(memberName);
			if (prop != null)
				return prop.GetMethod.Invoke(null, new object[0]);

			var field = type.GetField(memberName);
			if (field != null)
				return field.GetValue(null);

			throw new MemberNotFound(typeName, memberName);
		}

		public  object Instantiate(string typeName, params object[] args)
		{
			var type = _typeMap.ResolveType(typeName);
			if (type == null)
				throw new TypeNotFound(typeName);

			return TryInvokeExtension.Try(
				() => (type.IsValueType && args.Length == 0) 
					? FormatterServices.GetUninitializedObject(type)
					: type.GetConstructors().First(c => c.GetParameters().ParametersMatch(args)).Invoke(args));
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

	public static class DelegateUtility
	{
		public static bool ParametersMatch(this ParameterInfo[] prms, object[] arguments)
		{
			if (arguments.Length > prms.Length)
				return false;

			for (int i = 0; i < prms.Length; i++)
			{
				var param = prms[i];
				if (arguments.Length < i + 1)
				{
					return param.HasDefaultValue;
				}
				var arg = arguments[i];

				if (arg == null)
				{
					// all non-value types can accept null
					if (param.ParameterType.IsValueType)
						return false;
				}
				else
				{
					if (!param.ParameterType.IsInstanceOfType(arg))
						return false;
				}
			}

			return true;
		}

		// From http://code.logos.com/blog/2008/07/casting_delegates.html
		public static System.Delegate Cast(this System.Delegate source, Type type)
		{
			if (source == null)
				return null;

			System.Delegate[] delegates = source.GetInvocationList();
			if (delegates.Length == 1)
				return System.Delegate.CreateDelegate(type,
					delegates[0].Target, delegates[0].Method);

			System.Delegate[] delegatesDest = new System.Delegate[delegates.Length];
			for (int nDelegate = 0; nDelegate < delegates.Length; nDelegate++)
				delegatesDest[nDelegate] = System.Delegate.CreateDelegate(type,
					delegates[nDelegate].Target, delegates[nDelegate].Method);
			return System.Delegate.Combine(delegatesDest);
		}
	}
	static class TryInvokeExtension
	{
		public static object TryInvoke(this MethodInfo mi, object obj, params object[] value)
		{
			return Try(() => mi.Invoke(obj, value));
		}

		public static object Try(Func<object> func)
		{
			try
			{
				return func();
			}
			catch (TargetInvocationException tie)
			{
				ExceptionDispatchInfo.Capture(tie.InnerException ?? tie).Throw();
				throw tie.InnerException ?? tie; // this will never happen
			}
		}
	}
}