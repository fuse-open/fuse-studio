using Uno.UX.Markup.UXIL;

namespace Outracks.Simulator.CodeGeneration
{
	using Bytecode;
	using Runtime;
	using UXIL;

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
