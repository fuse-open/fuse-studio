using System;
using System.Collections.Generic;
using Outracks.Simulator.UXIL;
using Uno.UX.Markup.Reflection;
using Uno.UX.Markup.UXIL;

namespace Outracks.Simulator.CodeGeneration
{
	using Bytecode;

	class EventBindings
	{
		readonly Node _self;
		readonly Context _ctx;

		public EventBindings(Node self, Context ctx)
		{
			_self = self;
			_ctx = ctx;
		}

		public IEnumerable<Statement> SetEventBindings()
		{
			foreach (var e in _self.EventsWithHandler)
			{
				yield return HookEvent(e);
			}
		}

		Statement HookEvent(Event e)
		{
			var handler = e.Handler.MatchWith(
				(EventMethod m) => GetMethodGroupExpression(e, m),
				(EventBinding b) => GetMethodGroupExpression(e, b));

			return e.Facet.MatchWith(
				(IAttachedEvent a) => HookAttachedEvent(a, handler),
				(IRegularEvent r) => HookRegularEvent(e, handler));
		}

		Expression GetMethodGroupExpression(Event e, EventMethod b)
		{
			throw new NotSupportedException("Binding '" + b.Name + "' to Uno event handler method isn't supported.");
#if false
			var objectName = new Variable(b.Name.BeforeFirst("."));
			var methodName = new TypeMemberName(b.Name.AfterFirst("."));

			var delegateName = TypeName.Parse(e.Facet.DelegateName);
			return new MethodGroup(
				new ReadVariable(objectName), 
				methodName,
				Optional.None(),
				delegateName);
#endif
		}

		Expression GetMethodGroupExpression(Event e, EventBinding b)
		{
			var delegateName = TypeName.Parse(e.Facet.DelegateName);
			return new MethodGroup(
				b.Binding.GetExpression(_ctx),
				OnEvent,
				OnEventSignature,
				delegateName);
		}

		Statement HookAttachedEvent(IAttachedEvent a, Expression handler)
		{
			return new CallStaticMethod(a.GetStaticMemberName(), This, handler);
		}

		Statement HookRegularEvent(Event e, Expression handler)
		{
			return new AddEventHandler(This, e.GetMemberName(), handler);
		}

		static TypeMemberName OnEvent
		{
			get { return new TypeMemberName("OnEvent"); }
		}

		static Signature OnEventSignature
		{
			get
			{
				return new Signature(
					List.Create(
						new Parameter(Object, new Variable("o")), 
						new Parameter(EventArgs, new Variable("a"))),
					Optional.None());
			}
		}

		static TypeName Object
		{
			get { return TypeName.Parse("Uno.Object"); }
		}

		static TypeName EventArgs
		{
			get { return TypeName.Parse("Uno.EventArgs"); }
		}

		Expression This
		{
			get { return new ReadVariable(_ctx.Names[_self]); }
		}
	}
}
