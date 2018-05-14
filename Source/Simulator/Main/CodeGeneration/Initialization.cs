using System.Collections.Generic;
using System.Linq;
using Outracks.Simulator.Bytecode;
using Uno.UX.Markup.UXIL;

namespace Outracks.Simulator.CodeGeneration
{
	static class Initializations
	{
		public static IEnumerable<Statement> GetInitializations(this IEnumerable<Node> nodes, Context ctx)
		{
			return nodes.SelectMany(n => InitializeObject(n, ctx));
		}

		static IEnumerable<Statement> InitializeObject(this Node node, Context ctx)
		{
			return new[]
			{
				new SingleProperties(node, ctx).InitializeValues(),
				new ListProprties(node, ctx).SetListProperties(),
				new EventBindings(node, ctx).SetEventBindings() 
			}.SelectMany(s => s);
		}
	}
}