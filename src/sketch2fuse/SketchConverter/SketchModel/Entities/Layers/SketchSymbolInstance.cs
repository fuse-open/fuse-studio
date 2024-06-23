using System;

namespace SketchConverter.SketchModel
{
	public class SketchSymbolInstance : SketchLayer
	{
		public readonly Guid SymbolId;

		public SketchSymbolInstance(SketchLayer layer, Guid symbolId) :base(layer)
		{
			SymbolId = symbolId;
		}
	}
}
