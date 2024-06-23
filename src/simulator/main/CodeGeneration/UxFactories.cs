using Outracks.Simulator.Bytecode;
using Outracks.Simulator.Runtime;
using Outracks.Simulator.UXIL;
using Uno.UX.Markup.UXIL;

namespace Outracks.Simulator.CodeGeneration
{
	public static class FactoryExpression
	{
		public static Expression GetTemplateExpression(this TemplateNode tn, Context ctx)
		{
			return new Instantiate(
				TypeName.Parse(typeof(UxTemplate).FullName),
				tn.GenerateScopeConstructor(
					simulatedType: tn.ProducedType.GetTypeName(),
					producedType: tn.ProducedType.GetTypeName(),
					ctx: ctx),
                new StringLiteral(tn.Case),
                new BooleanLiteral(tn.IsDefaultCase));
		}
	}
}
