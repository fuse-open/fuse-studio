
using Uno;
using System.IO;

namespace Outracks.Simulator.Bytecode
{
	public abstract class Expression : Statement
//		, IMatchTypes<
//			ReadVariable,

//			Literal,
//			Lambda,
//			MethodGroup,

//			IsType,
//			LogicalOr,

//			Instantiate,
//			CallSimulatedMethod,

//			CallStaticMethod,
//			CallDynamicMethod,
//			ReadStaticField,
//			ReadProperty,
//			WriteProperty,
//			AddEventHandler,
//			RemoveEventHandler>
	{
		public abstract char ExpressionId { get; }
		protected abstract void WriteExpression(BinaryWriter writer);

		public T Match<T>(
			Func<ReadVariable, T> a1,
			Func<Literal, T> a2,
			Func<Lambda, T> a3,
			Func<MethodGroup, T> a4,
			Func<IsType, T> a5,
			Func<LogicalOr, T> a6,
			Func<Instantiate, T> a7,
			Func<CallLambda, T> a8,
			Func<CallStaticMethod, T> a9,
			Func<CallDynamicMethod, T> a10,
			Func<ReadStaticField, T> a11,
			Func<ReadProperty, T> a12,
			Func<WriteProperty, T> a13,
			Func<AddEventHandler, T> a15,
			Func<RemoveEventHandler, T> a16)
		{
			var self = this;
			var t1 = self as ReadVariable; if (t1 != null) return a1(t1);
			var t2 = self as Literal; if (t2 != null) return a2(t2);
			var t3 = self as Lambda; if (t3 != null) return a3(t3);
			var t4 = self as MethodGroup; if (t4 != null) return a4(t4);
			var t5 = self as IsType; if (t5 != null) return a5(t5);
			var t6 = self as LogicalOr; if (t6 != null) return a6(t6);
			var t7 = self as Instantiate; if (t7 != null) return a7(t7);
			var t8 = self as CallLambda; if (t8 != null) return a8(t8);
			var t9 = self as CallStaticMethod; if (t9 != null) return a9(t9);
			var t10 = self as CallDynamicMethod; if (t10 != null) return a10(t10);
			var t11 = self as ReadStaticField; if (t11 != null) return a11(t11);
			var t12 = self as ReadProperty; if (t12 != null) return a12(t12);
			var t13 = self as WriteProperty; if (t13 != null) return a13(t13);
			var t15 = self as AddEventHandler; if (t15 != null) return a15(t15);
			var t16 = self as RemoveEventHandler; if (t16 != null) return a16(t16);
			throw new NonExhaustiveMatch();
		}

		public void Match(
			Action<ReadVariable> a1,
			Action<Literal> a2,
			Action<Lambda> a3,
			Action<MethodGroup> a4,
			Action<IsType> a5,
			Action<LogicalOr> a6,
			Action<Instantiate> a7,
			Action<CallLambda> a8,
			Action<CallStaticMethod> a9,
			Action<CallDynamicMethod> a10,
			Action<ReadStaticField> a11,
			Action<ReadProperty> a12,
			Action<WriteProperty> a13,
			Action<AddEventHandler> a15,
			Action<RemoveEventHandler> a16)
		{
			var self = this;
			var t1 = self as ReadVariable; if (t1 != null) { a1(t1); return; }
			var t2 = self as Literal; if (t2 != null) { a2(t2); return; }
			var t3 = self as Lambda; if (t3 != null) { a3(t3); return; }
			var t4 = self as MethodGroup; if (t4 != null) { a4(t4); return; }
			var t5 = self as IsType; if (t5 != null)  { a5(t5); return; }
			var t6 = self as LogicalOr; if (t6 != null) { a6(t6); return; }
			var t7 = self as Instantiate; if (t7 != null) { a7(t7); return; }
			var t8 = self as CallLambda; if (t8 != null) { a8(t8); return; }
			var t9 = self as CallStaticMethod; if (t9 != null) { a9(t9); return; }
			var t10 = self as CallDynamicMethod; if (t10 != null) { a10(t10); return; }
			var t11 = self as ReadStaticField; if (t11 != null) { a11(t11); return; }
			var t12 = self as ReadProperty; if (t12 != null) { a12(t12); return; }
			var t13 = self as WriteProperty; if (t13 != null) { a13(t13); return; }
			var t15 = self as AddEventHandler; if (t15 != null) { a15(t15); return; }
			var t16 = self as RemoveEventHandler; if (t16 != null) { a16(t16); return; }
			throw new NonExhaustiveMatch();
		}

		public static void Write(Expression expression, BinaryWriter writer)
		{
			writer.Write(expression.ExpressionId);
			expression.WriteExpression(writer);
		}

		protected override void WriteStatement(BinaryWriter writer)
		{
			Expression.Write(this, writer);
		}

		new public static Func<BinaryReader, Expression> Read = _Read;

		new public static Expression _Read(BinaryReader reader)
		{
			var c = reader.ReadChar();
			switch (c)
			{
				case ExpressionIdRegistry.ReadVariable: return ReadVariable.Read(reader);
				case ExpressionIdRegistry.Literal: return Literal.Read(reader);
				case ExpressionIdRegistry.Lambda: return Lambda.Read(reader);
				case ExpressionIdRegistry.MethodGroup: return MethodGroup.Read(reader);
				case ExpressionIdRegistry.IsType: return IsType.Read(reader);
				case ExpressionIdRegistry.LogicalOr: return LogicalOr.Read(reader);
				case ExpressionIdRegistry.Instantiate: return Instantiate.Read(reader);
				case ExpressionIdRegistry.CallLambda: return CallLambda.Read(reader);
				case ExpressionIdRegistry.CallStaticMethod: return CallStaticMethod.Read(reader);
				case ExpressionIdRegistry.CallDynamicMethod: return CallDynamicMethod.Read(reader);
				case ExpressionIdRegistry.ReadStaticField: return ReadStaticField.Read(reader);
				case ExpressionIdRegistry.ReadProperty: return ReadProperty.Read(reader);
				case ExpressionIdRegistry.WriteProperty: return WriteProperty.Read(reader);
				case ExpressionIdRegistry.AddEventHandler: return AddEventHandler.Read(reader);
				case ExpressionIdRegistry.RemoveEventHandler: return RemoveEventHandler.Read(reader);
			}
			throw new InvalidDataException();
		}

		public override char StatementId
		{
			get { return StatementIdRegistry.Expression; }
		}

		public static Expression Throw(Exception e)
		{
			throw e;
		}
	}
}