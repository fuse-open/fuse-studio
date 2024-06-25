using Outracks.Simulator.Bytecode;
using Outracks.Simulator.UXIL;
using Uno.UX.Markup.UXIL;

namespace Outracks.Simulator.CodeGeneration
{
	static class ClassExpression
	{
		public static Expression GetClassExpression(this ClassNode cn, Context ctx)
		{
			return cn.GenerateScopeConstructor(
				simulatedType: cn.GeneratedClassName.ToTypeName(),
				producedType: cn.BaseType.GetTypeName(),
				ctx: ctx);
		}

	}
}