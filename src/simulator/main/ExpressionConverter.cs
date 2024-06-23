using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Outracks.Simulator.Bytecode;
using Expression = Outracks.Simulator.Bytecode.Expression;

namespace Outracks.Simulator
{
	public static class ExpressionConverter
	{
		public static Expression BytecodeFromSimpleLambda(Expression<Action> expression)
		{
			return ToBytecode(expression.Body);
		}

		public static Expression ToBytecode(System.Linq.Expressions.Expression expression)
		{
			var e = expression as MethodCallExpression;
			if (e == null)
				throw new Exception("Expression must be method call");

			if (!e.Method.IsStatic)
				throw new Exception("Method call must be static");

			return new CallStaticMethod(
				new StaticMemberName(
					TypeName.Parse(e.Method.DeclaringType.FullName),
					new TypeMemberName(e.Method.Name)),
				e.Arguments.Select(EvaluateToBytecode).ToArray());
		}

		static Expression EvaluateToBytecode(System.Linq.Expressions.Expression expression)
		{
			return ConstantToByteCode(Evaluate(expression));
		}

		static object Evaluate(System.Linq.Expressions.Expression expression)
		{
			if (expression == null)
				return null;

			var ce = expression as ConstantExpression;
			if (ce != null) return ce.Value;

			var mce = expression as MethodCallExpression;
			if (mce != null) return mce.Method.Invoke(Evaluate(mce.Object), mce.Arguments.Select(Evaluate).ToArray());

			var me = expression as MemberExpression;
			if (me == null) throw new Exception("Expression not supported: " + expression);

			var member = me.Member;
			var field = member as FieldInfo;
			if (field != null)
				return field.GetValue(Evaluate(me.Expression));

			var prop = member as PropertyInfo;
			if (prop != null)
				return prop.GetValue(Evaluate(me.Expression));

			throw new Exception("Expression not supported: " + expression);
		}

		static Expression ConstantToByteCode(object obj)
		{
			var e = obj as Expression; if (e != null) return e;
			var b = obj as byte[]; if (b != null) return new BlobLiteral(b);
			var s = obj as string; if (s != null) return new StringLiteral(s);
			if (obj is bool) return new BooleanLiteral((bool) obj);
			if (obj is byte) return new NumberLiteral(NumberType.Byte, (byte) obj);
			if (obj is double) return new NumberLiteral(NumberType.Double, (double) obj);
			if (obj is float) return new NumberLiteral(NumberType.Float, (float) obj);
			if (obj is int) return new NumberLiteral(NumberType.Int, (int) obj);
			if (obj is sbyte) return new NumberLiteral(NumberType.SByte, (sbyte) obj);
			if (obj is short) return new NumberLiteral(NumberType.Short, (short) obj);
			if (obj is uint) return new NumberLiteral(NumberType.UInt, (uint) obj);
			if (obj is ushort) return new NumberLiteral(NumberType.UShort, (ushort) obj);

			throw new Exception("Constant not supported: " + obj);
		}
	}
}