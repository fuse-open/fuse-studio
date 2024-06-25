using System;

namespace SketchConverter.SketchModel
{
	public class SketchSymbolMaster : SketchLayer
	{
		public readonly Guid SymbolId;
		public SketchSymbolMaster(SketchLayer parentLayer, Guid symbolId) : base(parentLayer)
		{
			SymbolId = symbolId;
		}
	}
}
