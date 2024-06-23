using Uno;
using Uno.Collections;

namespace Outracks.Simulator.Runtime
{
	using Bytecode;

	public class ScopeClosure
	{
		readonly Environment _scope;
		readonly IReflection _reflection;

		public ScopeClosure(Environment scope, IReflection reflection)
		{
			_reflection = reflection;
			_scope = scope;
		}

		/// <summary>
		/// Execute a lambda defined in _scope
		/// </summary>

		public object Execute(Lambda lambda, params object[] arguments)
		{
			var bodyEnv = new Environment(_scope);
			var bodyClosure = new ScopeClosure(bodyEnv, _reflection);

			bodyEnv.Bind(lambda.Signature.Parameters, arguments);

			foreach (var b in lambda.LocalVariables)
				bodyEnv.Bind(b.Variable, bodyClosure.Evaluate(b.Expression));

			return bodyClosure.Execute(lambda.Statements);
		}

		object Execute(IEnumerable<Statement> statements)
		{
			foreach (var statement in statements)
			{
				var e = statement as Expression;
				if (e != null)
				{
					Evaluate(e);
				}

				var r = statement as Return;
				if (r != null)
				{
					return Evaluate(r.Value);
				}
			}
			return null;
		}

		object Evaluate(Expression expression)
		{
			return expression.Match<object>(Evaluate,Evaluate,Evaluate,Evaluate,Evaluate,Evaluate,Evaluate,Evaluate,Evaluate,Evaluate,Evaluate,Evaluate,Evaluate,Evaluate,Evaluate);
		}

		object Evaluate(ReadVariable e)
		{
			return _scope.GetValue(e.Variable);
		}

		object Evaluate(Bytecode.MethodGroup g)
		{
			string[] parameters = null;
			if (g.MethodSignature.HasValue)
			{
				var sigParams = g.MethodSignature.Value.Parameters;
				parameters = new string[sigParams.Count];
				for (int i = 0; i < sigParams.Count; i++)
				{
					parameters[i] = sigParams[i].Type.FullName;
				}
			}

			return _reflection.CreateDelegate(Evaluate(g.Object), g.MethodName.Name, parameters, g.DelegateType.FullName);
		}

		object Evaluate(IsType t)
		{
			return t.IncludeSubtypes
				? _reflection.IsSubtype(Evaluate(t.Object), t.Type.FullName)
				: _reflection.IsType(Evaluate(t.Object), t.Type.FullName);
		}

		object Evaluate(LogicalOr o)
		{
			return (bool)Evaluate(o.Left) || (bool)Evaluate(o.Right);
		}

		object Evaluate(Instantiate i)
		{
			return _reflection.Instantiate(i.Type.FullName, Evaluate(i.Arguments));
		}

		object Evaluate(CallLambda i)
		{
			return Execute(Evaluate(i.Lambda), Evaluate(i.Arguments));
		}

		object Evaluate(CallStaticMethod m)
		{
			return _reflection.CallStatic(m.StaticMethod.TypeName.FullName, m.StaticMethod.MemberName.Name, Evaluate(m.Arguments));
		}

		object Evaluate(CallDynamicMethod m)
		{
			return _reflection.CallDynamic(Evaluate(m.Object), m.Method.Name, Evaluate(m.Arguments));
		}

		object Evaluate(ReadStaticField e)
		{
			return _reflection.GetStaticPropertyOrFieldValue(e.Field.TypeName.FullName, e.Field.MemberName.Name);
		}

		object Evaluate(ReadProperty e)
		{
			return _reflection.GetPropertyValue(Evaluate(e.Object), e.Property.Name);
		}

		object Evaluate(WriteProperty p)
		{
			_reflection.SetPropertyValue(Evaluate(p.Object), p.Property.Name, Evaluate(p.Value));
			return null;
		}

		object Evaluate(AddEventHandler e)
		{
			_reflection.AddEventHandler(Evaluate(e.Object), e.Event.Name, Evaluate(e.Handler));
			return null;
		}

		object Evaluate(RemoveEventHandler e)
		{
			_reflection.RemoveEventHandler(Evaluate(e.Object), e.Event.Name, Evaluate(e.Handler));
			return null;
		}

		object[] Evaluate(ImmutableList<Expression> arguments)
		{
			var objects = new object[arguments.Count];
			for (int i = 0; i < arguments.Count; i++)
			{
				objects[i] = Evaluate(arguments[i]);
			}
			return objects;
		}

		// Lambdas

		object Execute(object lambda, object[] arguments)
		{
			var f0 = lambda as Func<object>;
			if (f0 != null) return f0();
			var f1 = lambda as Func<object, object>;
			if (f1 != null) return f1(arguments[0]);
			var f2 = lambda as Func<object, object, object>;
			if (f2 != null) return f2(arguments[0], arguments[1]);
			var f3 = lambda as Func<object, object, object, object>;
			if (f3 != null) return f3(arguments[0], arguments[1], arguments[2]);
			var f4 = lambda as Func<object, object, object, object, object>;
			if (f4 != null) return f4(arguments[0], arguments[1], arguments[2], arguments[3]);
			var f5 = lambda as Func<object, object, object, object, object, object>;
			if (f5 != null) return f5(arguments[0], arguments[1], arguments[2], arguments[3], arguments[4]);
			var f6 = lambda as Func<object, object, object, object, object, object, object>;
			if (f6 != null) return f6(arguments[0], arguments[1], arguments[2], arguments[3], arguments[4], arguments[5]);
			var f7 = lambda as Func<object, object, object, object, object, object, object, object>;
			if (f7 != null) return f7(arguments[0], arguments[1], arguments[2], arguments[3], arguments[4], arguments[5], arguments[6]);
			var f8 = lambda as Func<object, object, object, object, object, object, object, object, object>;
			if (f8 != null) return f8(arguments[0], arguments[1], arguments[2], arguments[3], arguments[4], arguments[5], arguments[6], arguments[7]);
			var f9 = lambda as Func<object, object, object, object, object, object, object, object, object, object>;
			if (f9 != null) return f9(arguments[0], arguments[1], arguments[2], arguments[3], arguments[4], arguments[5], arguments[6], arguments[7], arguments[8]);
			var f10 = lambda as Func<object, object, object, object, object, object, object, object, object, object, object>;
			if (f10 != null) return f10(arguments[0], arguments[1], arguments[2], arguments[3], arguments[4], arguments[5], arguments[6], arguments[7], arguments[8], arguments[9]);
			var f11 = lambda as Func<object, object, object, object, object, object, object, object, object, object, object, object>;
			if (f11 != null) return f11(arguments[0], arguments[1], arguments[2], arguments[3], arguments[4], arguments[5], arguments[6], arguments[7], arguments[8], arguments[9], arguments[10]);
			var f12 = lambda as Func<object, object, object, object, object, object, object, object, object, object, object, object, object>;
			if (f12 != null) return f12(arguments[0], arguments[1], arguments[2], arguments[3], arguments[4], arguments[5], arguments[6], arguments[7], arguments[8], arguments[9], arguments[10], arguments[11]);
			var f13 = lambda as Func<object, object, object, object, object, object, object, object, object, object, object, object, object, object>;
			if (f13 != null) return f13(arguments[0], arguments[1], arguments[2], arguments[3], arguments[4], arguments[5], arguments[6], arguments[7], arguments[8], arguments[9], arguments[10], arguments[11], arguments[12]);
			var f14 = lambda as Func<object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>;
			if (f14 != null) return f14(arguments[0], arguments[1], arguments[2], arguments[3], arguments[4], arguments[5], arguments[6], arguments[7], arguments[8], arguments[9], arguments[10], arguments[11], arguments[12], arguments[13]);
			var f15 = lambda as Func<object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>;
			if (f15 != null) return f15(arguments[0], arguments[1], arguments[2], arguments[3], arguments[4], arguments[5], arguments[6], arguments[7], arguments[8], arguments[9], arguments[10], arguments[11], arguments[12], arguments[13], arguments[14]);
			var f16 = lambda as Func<object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>;
			if (f16 != null) return f16(arguments[0], arguments[1], arguments[2], arguments[3], arguments[4], arguments[5], arguments[6], arguments[7], arguments[8], arguments[9], arguments[10], arguments[11], arguments[12], arguments[13], arguments[14], arguments[15]);

			var a0 = lambda as Action;
			if (a0 != null) a0();
			var a1 = lambda as Action<object>;
			if (a1 != null) a1(arguments[0]);
			var a2 = lambda as Action<object, object>;
			if (a2 != null) a2(arguments[0], arguments[1]);
			var a3 = lambda as Action<object, object, object>;
			if (a3 != null) a3(arguments[0], arguments[1], arguments[2]);
			var a4 = lambda as Action<object, object, object, object>;
			if (a4 != null) a4(arguments[0], arguments[1], arguments[2], arguments[3]);
			var a5 = lambda as Action<object, object, object, object, object>;
			if (a5 != null) a5(arguments[0], arguments[1], arguments[2], arguments[3], arguments[4]);
			var a6 = lambda as Action<object, object, object, object, object, object>;
			if (a6 != null) a6(arguments[0], arguments[1], arguments[2], arguments[3], arguments[4], arguments[5]);
			var a7 = lambda as Action<object, object, object, object, object, object, object>;
			if (a7 != null) a7(arguments[0], arguments[1], arguments[2], arguments[3], arguments[4], arguments[5], arguments[6]);
			var a8 = lambda as Action<object, object, object, object, object, object, object, object>;
			if (a8 != null) a8(arguments[0], arguments[1], arguments[2], arguments[3], arguments[4], arguments[5], arguments[6], arguments[7]);
			var a9 = lambda as Action<object, object, object, object, object, object, object, object, object>;
			if (a9 != null) a9(arguments[0], arguments[1], arguments[2], arguments[3], arguments[4], arguments[5], arguments[6], arguments[7], arguments[8]);
			var a10 = lambda as Action<object, object, object, object, object, object, object, object, object, object>;
			if (a10 != null) a10(arguments[0], arguments[1], arguments[2], arguments[3], arguments[4], arguments[5], arguments[6], arguments[7], arguments[8], arguments[9]);
			var a11 = lambda as Action<object, object, object, object, object, object, object, object, object, object, object>;
			if (a11 != null) a11(arguments[0], arguments[1], arguments[2], arguments[3], arguments[4], arguments[5], arguments[6], arguments[7], arguments[8], arguments[9], arguments[10]);
			var a12 = lambda as Action<object, object, object, object, object, object, object, object, object, object, object, object>;
			if (a12 != null) a12(arguments[0], arguments[1], arguments[2], arguments[3], arguments[4], arguments[5], arguments[6], arguments[7], arguments[8], arguments[9], arguments[10], arguments[11]);
			var a13 = lambda as Action<object, object, object, object, object, object, object, object, object, object, object, object, object>;
			if (a13 != null) a13(arguments[0], arguments[1], arguments[2], arguments[3], arguments[4], arguments[5], arguments[6], arguments[7], arguments[8], arguments[9], arguments[10], arguments[11], arguments[12]);
			var a14 = lambda as Action<object, object, object, object, object, object, object, object, object, object, object, object, object, object>;
			if (a14 != null) a14(arguments[0], arguments[1], arguments[2], arguments[3], arguments[4], arguments[5], arguments[6], arguments[7], arguments[8], arguments[9], arguments[10], arguments[11], arguments[12], arguments[13]);
			var a15 = lambda as Action<object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>;
			if (a15 != null) a15(arguments[0], arguments[1], arguments[2], arguments[3], arguments[4], arguments[5], arguments[6], arguments[7], arguments[8], arguments[9], arguments[10], arguments[11], arguments[12], arguments[13], arguments[14]);
			var a16 = lambda as Action<object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>;
			if (a16 != null) a16(arguments[0], arguments[1], arguments[2], arguments[3], arguments[4], arguments[5], arguments[6], arguments[7], arguments[8], arguments[9], arguments[10], arguments[11], arguments[12], arguments[13], arguments[14], arguments[15]);

			return null;
		}

		object Evaluate(Lambda p)
		{
			if (p.Signature.ReturnType.HasValue)
			{
				switch (p.Signature.Parameters.Count)
				{
					case 0: return (Func<object>)new LambdaClosure(p, Execute).Func;
					case 1: return (Func<object, object>)new LambdaClosure(p, Execute).Func;
					case 2: return (Func<object, object, object>)new LambdaClosure(p, Execute).Func;
					case 3: return (Func<object, object, object, object>)new LambdaClosure(p, Execute).Func;
					case 4: return (Func<object, object, object, object, object>)new LambdaClosure(p, Execute).Func;
					case 5: return (Func<object, object, object, object, object, object>)new LambdaClosure(p, Execute).Func;
					case 6: return (Func<object, object, object, object, object, object, object>)new LambdaClosure(p, Execute).Func;
					case 7: return (Func<object, object, object, object, object, object, object, object>)new LambdaClosure(p, Execute).Func;
					case 8: return (Func<object, object, object, object, object, object, object, object, object>)new LambdaClosure(p, Execute).Func;
					case 9: return (Func<object, object, object, object, object, object, object, object, object, object>)new LambdaClosure(p, Execute).Func;
					case 10: return (Func<object, object, object, object, object, object, object, object, object, object, object>)new LambdaClosure(p, Execute).Func;
					case 11: return (Func<object, object, object, object, object, object, object, object, object, object, object, object>)new LambdaClosure(p, Execute).Func;
					case 12: return (Func<object, object, object, object, object, object, object, object, object, object, object, object, object>)new LambdaClosure(p, Execute).Func;
					case 13: return (Func<object, object, object, object, object, object, object, object, object, object, object, object, object, object>)new LambdaClosure(p, Execute).Func;
					case 14: return (Func<object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>)new LambdaClosure(p, Execute).Func;
					case 15: return (Func<object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>)new LambdaClosure(p, Execute).Func;
				}
			}
			else
			{
				switch (p.Signature.Parameters.Count)
				{
					case 0: return (Action)new LambdaClosure(p, Execute).Action;
					case 1: return (Action<object>)new LambdaClosure(p, Execute).Action;
					case 2: return (Action<object, object>)new LambdaClosure(p, Execute).Action;
					case 3: return (Action<object, object, object>)new LambdaClosure(p, Execute).Action;
					case 4: return (Action<object, object, object, object>)new LambdaClosure(p, Execute).Action;
					case 5: return (Action<object, object, object, object, object>)new LambdaClosure(p, Execute).Action;
					case 6: return (Action<object, object, object, object, object, object>)new LambdaClosure(p, Execute).Action;
					case 7: return (Action<object, object, object, object, object, object, object>)new LambdaClosure(p, Execute).Action;
					case 8: return (Action<object, object, object, object, object, object, object, object>)new LambdaClosure(p, Execute).Action;
					case 9: return (Action<object, object, object, object, object, object, object, object, object>)new LambdaClosure(p, Execute).Action;
					case 10: return (Action<object, object, object, object, object, object, object, object, object, object>)new LambdaClosure(p, Execute).Action;
					case 11: return (Action<object, object, object, object, object, object, object, object, object, object, object>)new LambdaClosure(p, Execute).Action;
					case 12: return (Action<object, object, object, object, object, object, object, object, object, object, object, object>)new LambdaClosure(p, Execute).Action;
					case 13: return (Action<object, object, object, object, object, object, object, object, object, object, object, object, object>)new LambdaClosure(p, Execute).Action;
					case 14: return (Action<object, object, object, object, object, object, object, object, object, object, object, object, object, object>)new LambdaClosure(p, Execute).Action;
					case 15: return (Action<object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>)new LambdaClosure(p, Execute).Action;
				}
			}
			throw new Exception("Illegal parameter count");
		}

		// Literals

		object Evaluate(Literal e)
		{
			return e.Match<object>(Evaluate, Evaluate, Evaluate, Evaluate, Evaluate);
		}

		object Evaluate(BlobLiteral e) { return e.Bytes; }

		object Evaluate(BooleanLiteral e) { return e.BooleanValue; }

		object Evaluate(StringLiteral e) { return e.StringValue; }

		object Evaluate(NumberLiteral l)
		{
			switch (l.NumberType)
			{
				case NumberType.Double: return (double)l.DoubleValue;
				case NumberType.Float: return (float)l.DoubleValue;
				case NumberType.Int: return (int)l.DoubleValue;
				case NumberType.UInt: return (uint)l.DoubleValue;
				case NumberType.Short: return (short)l.DoubleValue;
				case NumberType.UShort: return (ushort)l.DoubleValue;
				case NumberType.SByte: return (sbyte)l.DoubleValue;
				case NumberType.Byte: return (byte)l.DoubleValue;
			}
			throw new ArgumentException("Invalid number type " + l.NumberType);
		}

		object Evaluate(EnumLiteral i)
		{
			return _reflection.GetEnumValue(i.Value.TypeName.FullName, i.Value.MemberName.Name);
		}


	}


	class LambdaClosure
	{
		readonly Lambda _lambda;
		readonly Func<Lambda, object[], object> _execute;

		public LambdaClosure(Lambda lambda, Func<Lambda, object[], object> execute)
		{
			_lambda = lambda;
			_execute = execute;
		}

		public void Action()
		{
			Func();
		}

		public void Action(object a1)
		{
			Func(a1);
		}

		public void Action(object a1, object a2)
		{
			Func(a1, a2);
		}

		public void Action(object a1, object a2, object a3)
		{
			Func(a1, a2, a3);
		}

		public void Action(object a1, object a2, object a3, object a4)
		{
			Func(a1, a2, a3, a4);
		}

		public void Action(object a1, object a2, object a3, object a4, object a5)
		{
			Func(a1, a2, a3, a4, a5);
		}

		public void Action(object a1, object a2, object a3, object a4, object a5, object a6)
		{
			Func(a1, a2, a3, a4, a5, a6);
		}

		public void Action(object a1, object a2, object a3, object a4, object a5, object a6, object a7)
		{
			Func(a1, a2, a3, a4, a5, a6, a7);
		}

		public void Action(object a1, object a2, object a3, object a4, object a5, object a6, object a7, object a8)
		{
			Func(a1, a2, a3, a4, a5, a6, a7, a8);
		}

		public void Action(object a1, object a2, object a3, object a4, object a5, object a6, object a7, object a8, object a9)
		{
			Func(a1, a2, a3, a4, a5, a6, a7, a8, a9);
		}

		public void Action(object a1, object a2, object a3, object a4, object a5, object a6, object a7, object a8, object a9, object a10)
		{
			Func(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10);
		}

		public void Action(object a1, object a2, object a3, object a4, object a5, object a6, object a7, object a8, object a9, object a10, object a11)
		{
			Func(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11);
		}

		public void Action(object a1, object a2, object a3, object a4, object a5, object a6, object a7, object a8, object a9, object a10, object a11, object a12)
		{
			Func(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12);
		}

		public void Action(object a1, object a2, object a3, object a4, object a5, object a6, object a7, object a8, object a9, object a10, object a11, object a12, object a13)
		{
			Func(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13);
		}

		public void Action(object a1, object a2, object a3, object a4, object a5, object a6, object a7, object a8, object a9, object a10, object a11, object a12, object a13, object a14)
		{
			Func(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14);
		}

		public void Action(object a1, object a2, object a3, object a4, object a5, object a6, object a7, object a8, object a9, object a10, object a11, object a12, object a13, object a14, object a15)
		{
			Func(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15);
		}

		public void Action(object a1, object a2, object a3, object a4, object a5, object a6, object a7, object a8, object a9, object a10, object a11, object a12, object a13, object a14, object a15, object a16)
		{
			Func(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15, a16);
		}

		public object Func()
		{
			return _execute(_lambda, new object[0]);
		}

		public object Func(object a1)
		{
			return _execute(_lambda, new[] { a1 });
		}

		public object Func(object a1, object a2)
		{
			return _execute(_lambda, new[] { a1, a2 });
		}

		public object Func(object a1, object a2, object a3)
		{
			return _execute(_lambda, new [] { a1, a2, a3 });
		}

		public object Func(object a1, object a2, object a3, object a4)
		{
			return _execute(_lambda, new[] { a1, a2, a3, a4 });
		}

		public object Func(object a1, object a2, object a3, object a4, object a5)
		{
			return _execute(_lambda, new[] { a1, a2, a3, a4, a5 });
		}

		public object Func(object a1, object a2, object a3, object a4, object a5, object a6)
		{
			return _execute(_lambda, new[] { a1, a2, a3, a4, a5, a6 });
		}

		public object Func(object a1, object a2, object a3, object a4, object a5, object a6, object a7)
		{
			return _execute(_lambda, new[] { a1, a2, a3, a4, a5, a6, a7 });
		}

		public object Func(object a1, object a2, object a3, object a4, object a5, object a6, object a7, object a8)
		{
			return _execute(_lambda, new[] { a1, a2, a3, a4, a5, a6, a7, a8 });
		}

		public object Func(object a1, object a2, object a3, object a4, object a5, object a6, object a7, object a8, object a9)
		{
			return _execute(_lambda, new[] { a1, a2, a3, a4, a5, a6, a7, a8, a9 });
		}

		public object Func(object a1, object a2, object a3, object a4, object a5, object a6, object a7, object a8, object a9, object a10)
		{
			return _execute(_lambda, new[] { a1, a2, a3, a4, a5, a6, a7, a8, a9, a10 });
		}

		public object Func(object a1, object a2, object a3, object a4, object a5, object a6, object a7, object a8, object a9, object a10, object a11)
		{
			return _execute(_lambda, new[] { a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11 });
		}

		public object Func(object a1, object a2, object a3, object a4, object a5, object a6, object a7, object a8, object a9, object a10, object a11, object a12)
		{
			return _execute(_lambda, new[] { a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12 });
		}

		public object Func(object a1, object a2, object a3, object a4, object a5, object a6, object a7, object a8, object a9, object a10, object a11, object a12, object a13)
		{
			return _execute(_lambda, new[] { a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13 });
		}

		public object Func(object a1, object a2, object a3, object a4, object a5, object a6, object a7, object a8, object a9, object a10, object a11, object a12, object a13, object a14)
		{
			return _execute(_lambda, new[] { a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14 });
		}

		public object Func(object a1, object a2, object a3, object a4, object a5, object a6, object a7, object a8, object a9, object a10, object a11, object a12, object a13, object a14, object a15)
		{
			return _execute(_lambda, new[] { a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15 });
		}

		public object Func(object a1, object a2, object a3, object a4, object a5, object a6, object a7, object a8, object a9, object a10, object a11, object a12, object a13, object a14, object a15, object a16)
		{
			return _execute(_lambda, new[] { a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15, a16 });
		}
	}
}