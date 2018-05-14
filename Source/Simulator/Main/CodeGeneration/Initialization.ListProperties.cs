using System.Collections.Generic;
using Outracks.Simulator.Bytecode;
using Outracks.Simulator.UXIL;
using Uno.UX.Markup.UXIL;

namespace Outracks.Simulator.CodeGeneration
{
	class ListProprties
	{
		readonly Node _self;
		readonly Context _ctx;

		public ListProprties(Node self, Context ctx)
		{
			_self = self;
			_ctx = ctx;
		}

		public IEnumerable<Statement> SetListProperties()
		{
			foreach (var prop in _self.ListPropertiesWithValues)
			{
				foreach (var value in prop.Sources)
					yield return AddToList(prop, value.GetExpression(_ctx));
			}
		}

		Statement AddToList(Property prop, Expression value)
		{
			if (prop.IsAttachedProperty())
				return new NoOperation("attached list property not supported");

			var list = new ReadProperty(This, prop.GetMemberName());
			return new CallDynamicMethod(list, Add, value);
		}

		static TypeMemberName Add
		{
			get { return new TypeMemberName("Add"); }
		}


		Expression This
		{
			get { return new ReadVariable(_ctx.Names[_self]); }
		}

	}
}

