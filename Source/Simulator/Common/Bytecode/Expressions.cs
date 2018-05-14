using Uno;
using Uno.Collections;
using System.IO;

namespace Outracks.Simulator.Bytecode
{
	public sealed class LogicalOr : Expression
	{
		public readonly Expression Left;
		public readonly Expression Right;

		public override char ExpressionId { get { return ExpressionIdRegistry.LogicalOr; } }

		public LogicalOr(Expression left, Expression right)
		{
			Left = left;
			Right = right;
		}

		public override string ToString()
		{
			return "(" + Left + " || " + Right + ")";
		}

		public static void Write(LogicalOr o, BinaryWriter writer)
		{
			o.WriteExpression(writer);
		}

		protected override void WriteExpression(BinaryWriter writer)
		{
			Expression.Write(Left, writer);
			Expression.Write(Right, writer);
		}

		new public static LogicalOr Read(BinaryReader reader)
		{
			var _Left = Expression.Read(reader);
			var _Right = Expression.Read(reader);
			return new LogicalOr(
				_Left,
				_Right);
		}
	}

	public sealed class IsType : Expression
	{
		public readonly Expression Object;
		public readonly TypeName Type;
		public readonly bool IncludeSubtypes;

		public override char ExpressionId { get { return ExpressionIdRegistry.IsType; } }

		public IsType(Expression o, TypeName type, bool includeSubtypes = true)
		{
			Object = o;
			Type = type;
			IncludeSubtypes = includeSubtypes;
		}

		public static void Write(IsType o, BinaryWriter writer)
		{
			o.WriteExpression(writer);
		}

		protected override void WriteExpression(BinaryWriter writer)
		{
			Expression.Write(Object, writer);
			TypeName.Write(Type, writer);
			writer.Write(IncludeSubtypes);
		}

		new public static IsType Read(BinaryReader reader)
		{
			var _Object = Expression.Read(reader);
			var _Type = TypeName.Read(reader);
			var _IncludeSubtypes = reader.ReadBoolean();
			return new IsType(
				_Object,
				_Type,
				_IncludeSubtypes);
		}
	}

	public sealed class ReadVariable : Expression
	{
		public readonly Variable Variable;

		public override char ExpressionId { get { return ExpressionIdRegistry.ReadVariable; } }

		public ReadVariable(Variable variable)
		{
			Variable = variable;
		}

		public override string ToString()
		{
			return Variable.Name;
		}
		
		public static void Write(ReadVariable o, BinaryWriter writer)
		{
			o.WriteExpression(writer);
		}

		protected override void WriteExpression(BinaryWriter writer)
		{
			Variable.Write(Variable, writer);
		}

		new public static ReadVariable Read(BinaryReader reader)
		{
			var variable = Variable.Read(reader);
			return new ReadVariable(
				variable);
		}
	}

	public sealed class Instantiate : Expression
	{
		public readonly TypeName Type;
		public readonly ImmutableList<Expression> Arguments;

		public override char ExpressionId { get { return ExpressionIdRegistry.Instantiate; } }

		public Instantiate(TypeName type, params Expression[] arguments)
			: this(type, ((IEnumerable<Expression>)arguments).ToImmutableList())
		{ }

		public Instantiate(TypeName type, ImmutableList<Expression> arguments)
		{
			Type = type;
			Arguments = arguments;
		}

		public override string ToString()
		{
			return "new " + Type.FullName + "(" + Arguments.JoinToString(", ") + ")";
		}

		public static void Write(Instantiate o, BinaryWriter writer)
		{
			o.WriteExpression(writer);
		}

		protected override void WriteExpression(BinaryWriter writer)
		{
			TypeName.Write(Type, writer);
			List.Write(writer, Arguments, Expression.Write);
		}

		new public static Instantiate Read(BinaryReader reader)
		{
			var _Type = TypeName.Read(reader);
			var _Arguments = List.Read(reader, Expression.Read);
			return new Instantiate(
				_Type,
				_Arguments);
		}
	}

	public sealed class CallLambda : Expression
	{
		public readonly Expression Lambda;
		public readonly ImmutableList<Expression> Arguments;

		public override char ExpressionId { get { return ExpressionIdRegistry.CallLambda; } }

		public CallLambda(Expression lambda, params Expression[] arguments)
			: this(lambda, ((IEnumerable<Expression>)arguments).ToImmutableList())
		{ }

		public CallLambda(Expression lanbda, ImmutableList<Expression> arguments)
		{
			Lambda = lanbda;
			Arguments = arguments;
		}

		public override string ToString()
		{
			return Lambda + "(" + Arguments.JoinToString(", ") + ")";
		}

		public static void Write(CallLambda o, BinaryWriter writer)
		{
			Expression.Write(o.Lambda, writer);
			List.Write(writer, o.Arguments, Expression.Write);
		}

		protected override void WriteExpression(BinaryWriter writer)
		{
			CallLambda.Write(this, writer);
		}

		new public static CallLambda Read(BinaryReader reader)
		{
			var lambda = Expression.Read(reader);
			var arguments = List.Read(reader, Expression.Read);
			return new CallLambda(
				lambda,
				arguments);
		}
	}

	public sealed class CallStaticMethod : Expression
	{
		public readonly StaticMemberName StaticMethod;
		public readonly ImmutableList<Expression> Arguments;

		public override char ExpressionId { get { return ExpressionIdRegistry.CallStaticMethod; } }

		public CallStaticMethod(StaticMemberName staticMethod, params Expression[] arguments)
			: this(staticMethod, ((IEnumerable<Expression>)arguments).ToImmutableList())
		{ }

		public CallStaticMethod(StaticMemberName staticMethod, ImmutableList<Expression> arguments)
		{
			StaticMethod = staticMethod;
			Arguments = arguments;
		}

		public override string ToString()
		{
			return StaticMethod + "(" + Arguments.JoinToString(", ") + ")";
		}

		public static void Write(CallStaticMethod o, BinaryWriter writer)
		{
			o.WriteExpression(writer);
		}

		protected override void WriteExpression(BinaryWriter writer)
		{
			StaticMemberName.Write(StaticMethod, writer);
			List.Write(writer, Arguments, Expression.Write);
		}

		new public static CallStaticMethod Read(BinaryReader reader)
		{
			var _StaticMethod = StaticMemberName.Read(reader);
			var _Arguments = List.Read(reader, Expression.Read);
			return new CallStaticMethod(
				_StaticMethod,
				_Arguments);
		}
	}

	public sealed class CallDynamicMethod : Expression
	{
		public readonly Expression Object;
		public readonly TypeMemberName Method;
		public readonly ImmutableList<Expression> Arguments;

		public override char ExpressionId { get { return ExpressionIdRegistry.CallDynamicMethod; } }

		public CallDynamicMethod(Expression o, TypeMemberName method, params Expression[] arguments)
			: this(o, method, ((IEnumerable<Expression>)arguments).ToImmutableList())
		{ }

		public CallDynamicMethod(Expression o, TypeMemberName method, ImmutableList<Expression> arguments)
		{
			Object = o;
			Method = method;
			Arguments = arguments;
		}

		public override string ToString()
		{
			return Object + "." + Method.Name + "(" + Arguments.JoinToString(", ") + ")";
		}

		public static void Write(CallDynamicMethod o, BinaryWriter writer)
		{
			o.WriteExpression(writer);
		}

		protected override void WriteExpression(BinaryWriter writer)
		{
			Expression.Write(Object, writer);
			TypeMemberName.Write(Method, writer);
			List.Write(writer, Arguments, Expression.Write);
		}

		new public static CallDynamicMethod Read(BinaryReader reader)
		{
			var _Object = Expression.Read(reader);
			var _Method = TypeMemberName.Read(reader);
			var _Arguments = List.Read(reader, Expression.Read);
			return new CallDynamicMethod(
				_Object,
				_Method,
				_Arguments);
		}
	}

	public sealed class ReadStaticField : Expression
	{
		public readonly StaticMemberName Field;

		public override char ExpressionId { get { return ExpressionIdRegistry.ReadStaticField; } }

		public ReadStaticField(StaticMemberName field)
		{
			Field = field;
		}

		public override string ToString()
		{
			return Field.TypeName.FullName + "." + Field.MemberName.Name;
		}

		public static void Write(ReadStaticField o, BinaryWriter writer)
		{
			o.WriteExpression(writer);
		}

		protected override void WriteExpression(BinaryWriter writer)
		{
			StaticMemberName.Write(Field, writer);
		}

		new public static ReadStaticField Read(BinaryReader reader)
		{
			var _Field = StaticMemberName.Read(reader);
			return new ReadStaticField(
				_Field);
		}
	}

	public sealed class ReadProperty : Expression
	{
		public readonly Expression Object;
		public readonly TypeMemberName Property;

		public override char ExpressionId { get { return ExpressionIdRegistry.ReadProperty; } }

		public ReadProperty(Expression o, TypeMemberName property)
		{
			Object = o;
			Property = property;
		}

		public override string ToString()
		{
			return Object + "." + Property.Name;
		}

		public static void Write(ReadProperty o, BinaryWriter writer)
		{
			o.WriteExpression(writer);
		}

		protected override void WriteExpression(BinaryWriter writer)
		{
			Expression.Write(Object, writer);
			TypeMemberName.Write(Property, writer);
		}

		new public static ReadProperty Read(BinaryReader reader)
		{
			var _Object = Expression.Read(reader);
			var _Property = TypeMemberName.Read(reader);
			return new ReadProperty(
				_Object,
				_Property);
		}
	}


	public sealed class WriteProperty : Expression
	{
		public readonly Expression Object;
		public readonly TypeMemberName Property;
		public readonly Expression Value;

		public override char ExpressionId { get { return ExpressionIdRegistry.WriteProperty; } }

		public WriteProperty(Expression o, TypeMemberName property, Expression value)
		{
			Object = o;
			Property = property;
			Value = value;
		}

		public override string ToString()
		{
			return Object + "." + Property.Name + " = " + Value;
		}

		public static void Write(WriteProperty o, BinaryWriter writer)
		{
			o.WriteExpression(writer);
		}

		protected override void WriteExpression(BinaryWriter writer)
		{
			Expression.Write(Object, writer);
			TypeMemberName.Write(Property, writer);
			Expression.Write(Value, writer);
		}

		new public static WriteProperty Read(BinaryReader reader)
		{
			var _Object = Expression.Read(reader);
			var _Property = TypeMemberName.Read(reader);
			var _Value = Expression.Read(reader);
			return new WriteProperty(
				_Object,
				_Property,
				_Value);
		}
	}

	public sealed class AddEventHandler : Expression
	{
		public readonly Expression Object;
		public readonly TypeMemberName Event;
		public readonly Expression Handler;

		public override char ExpressionId { get { return ExpressionIdRegistry.AddEventHandler; } }

		public AddEventHandler(Expression o, TypeMemberName ev, Expression handler)
		{
			Object = o;
			Event = ev;
			Handler = handler;
		}

		public override string ToString()
		{
			return Object + "." + Event.Name + " += " + Handler;
		}

		public static void Write(AddEventHandler o, BinaryWriter writer)
		{
			o.WriteExpression(writer);
		}

		protected override void WriteExpression(BinaryWriter writer)
		{
			Expression.Write(Object, writer);
			TypeMemberName.Write(Event, writer);
			Expression.Write(Handler, writer);
		}

		new public static AddEventHandler Read(BinaryReader reader)
		{
			var _Object = Expression.Read(reader);
			var _Event = TypeMemberName.Read(reader);
			var _Handler = Expression.Read(reader);
			return new AddEventHandler(
				_Object,
				_Event,
				_Handler);
		}
	}

	public sealed class RemoveEventHandler : Expression
	{
		public readonly Expression Object;
		public readonly TypeMemberName Event;
		public readonly Expression Handler;

		public override char ExpressionId { get { return ExpressionIdRegistry.RemoveEventHandler; } }

		public RemoveEventHandler(Expression o, TypeMemberName ev, Expression handler)
		{
			Object = o;
			Event = ev;
			Handler = handler;
		}

		public override string ToString()
		{
			return Object + "." + Event.Name + " -= " + Handler;
		}

		public static void Write(RemoveEventHandler o, BinaryWriter writer)
		{
			o.WriteExpression(writer);
		}

		protected override void WriteExpression(BinaryWriter writer)
		{
			Expression.Write(Object, writer);
			TypeMemberName.Write(Event, writer);
			Expression.Write(Handler, writer);
		}

		new public static RemoveEventHandler Read(BinaryReader reader)
		{
			var _Object = Expression.Read(reader);
			var _Event = TypeMemberName.Read(reader);
			var _Handler = Expression.Read(reader);
			return new RemoveEventHandler(
				_Object,
				_Event,
				_Handler);
		}
	}

}