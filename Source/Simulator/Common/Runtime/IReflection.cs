using Uno;

namespace Outracks.Simulator.Runtime
{
	using Bytecode;

	public class MemberNotFound : Exception
	{
		public readonly string Type;
		public readonly string Member;

		public MemberNotFound(string type, string member)
			: base("Member '" + member + "' could not be found on object of type '" + type + "'")
		{
			Type = type;
			Member = member;
		}
	}

	public class TypeNotFound : Exception
	{
		public readonly string Type;

		public TypeNotFound(string type)
			: base("Type '" + type + "' could not be found (recompile required?)")
		{
			Type = type;
		}
	}

	public class UnambiguousMethodNotFound : Exception
	{
		public readonly string Type;
		public readonly string Member;

		public UnambiguousMethodNotFound(string type, string member)
			: base("Call to '" + member + "' method is ambigious on object of type '" + type + "'")
		{
			Type = type;
			Member = member;
		}
	}

	public interface IReflection
	{
		object CallDynamic(object instance, string methodName, params object[] arguments);
		object CallStatic(string typeName, string methodName, params object[] arguments);

		void SetPropertyValue(object instance, string propertyName, object value);
		object GetPropertyValue(object instance, string propertyName);

		object GetStaticPropertyOrFieldValue(string typeName, string memberName);

		object CreateDelegate(object instance, string methodName, string[] methodArgumentTypes, string delegateTypeName);

		void AddEventHandler(object instance, string member, object handlerDelegate);
		void RemoveEventHandler(object instance, string member, object handlerDelegate);

		object Instantiate(string typeName, params object[] args);
		bool IsSubtype(object obj, string typeName);
		bool IsType(object obj, string typeName);
		object GetEnumValue(string enumType, string valueName);
	}
}