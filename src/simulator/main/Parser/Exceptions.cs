namespace Outracks.Simulator.Parser
{
	public class UserCodeContainsErrors : BuildFailed
	{
		public override string Message
		{
			get { return "Errors were encountered while building the project"; }
		}
	}

	class UnknownError : UserCodeContainsErrors
	{
		readonly string _message;
		public override string Message { get { return _message; } }

		public UnknownError(string message)
		{
			_message = message;
		}

	}

	class InvalidMarkup : UserCodeContainsErrors
	{
		public readonly string File;
		public readonly Optional<TextPosition> Position;
		readonly string _message;
		public override string Message { get { return _message; } }

		public InvalidMarkup(string file, Optional<TextPosition> position, string message)
		{
			File = file;
			Position = position;
			_message = message;
		}
	}

	class TypeNotFound : UserCodeContainsErrors
	{
		public readonly string TypeName;
		public readonly string File;

		public TypeNotFound(string typeName, string file)
		{
			TypeName = typeName;
			File = file;
		}
	}

	class UnknownBaseClass : UserCodeContainsErrors
	{
		public readonly string BaseClassName;
		public readonly string DeclaredClassName;
		public readonly string DeclaringFile;

		public UnknownBaseClass(string baseClassName, string declaredClassName, string declaringFile)
		{
			BaseClassName = baseClassName;
			DeclaredClassName = declaredClassName;
			DeclaringFile = declaringFile;
		}
	}

	class UnknownMemberType : UserCodeContainsErrors
	{
		public readonly string TypeName;
		public readonly string MemberName;
		public readonly string DeclaringClassName;
		public readonly string DeclaringFile;

		public UnknownMemberType(IMemberNode member, OuterClassNode declaringNode)
			: this(member.TypeName, member.Name, declaringNode.GeneratedTypeName, declaringNode.DeclaringFile)
		{ }

		public UnknownMemberType(string typeName, string memberName, string declaringClassName, string declaringFile)
		{
			TypeName = typeName;
			MemberName = memberName;
			DeclaringClassName = declaringClassName;
			DeclaringFile = declaringFile;
		}
	}

	class CyclicClassHierarchy : UserCodeContainsErrors
	{
	}

	class TypeNameCollision : UserCodeContainsErrors
	{
		public readonly string TypeName;
		public TypeNameCollision(string typeName)
		{
			TypeName = typeName;
		}
	}
}
