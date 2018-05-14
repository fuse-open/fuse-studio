using System;
using System.Collections.Generic;
using SketchConverter.SketchModel;

namespace SketchConverter.UxBuilder
{
	public class SymbolClassNameBuilder
	{
		private const string Prefix = "Sketch";
		private readonly Dictionary<Guid, string> _symbolClasses = new Dictionary<Guid, string>();

		public void Init(IEnumerable<SketchSymbolMaster> symbols)
		{
			foreach (var symbol in symbols)
			{
				Add(symbol);
			}
		}

		public string GetClassName(SketchSymbolMaster symbolMaster)
		{
			try
			{
				return _symbolClasses[symbolMaster.SymbolId];
			}
			catch (KeyNotFoundException)
			{
				throw new UxBuilderException("Could not find symbol master '" + symbolMaster.Name + "' (SymbolId '"+ symbolMaster.SymbolId +"').");
			}
		}

		public string GetClassName(SketchSymbolInstance symbolInstance)
		{
			try
			{
				return _symbolClasses[symbolInstance.SymbolId];
			}
			catch (KeyNotFoundException)
			{
				throw new UxBuilderException("The symbol '" + symbolInstance.Name + "' is an instance of a symbol we can't find. Could it be that it's defined in another Sketch file? That is currently not supported. (SymbolId '" + symbolInstance.SymbolId + "')");
			}
		}

		private void Add(SketchSymbolMaster symbol)
		{
			_symbolClasses.Add(symbol.SymbolId, Prefix + "." + symbol.Name);
		}

	}

}
