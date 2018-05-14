using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using SketchConverter;
using SketchConverter.SketchModel;

namespace SketchConverterTests
{
	public static class DocumentBuilder
	{

		public static SketchSymbolInstance SymbolInstanceOf(SketchSymbolMaster symbol, string name = null)
		{
			return new SketchSymbolInstance(
				SketchLayer(id: Guid.NewGuid(), frame: Frame, name:name),
				symbol.SymbolId);
		}

		public static SketchDocument SketchDocument()
		{
			var document = new SketchDocument(Guid.NewGuid(),new List<SketchPage>
				{
					new SketchPage
					(
						Guid.NewGuid(),
						Frame,
						"Symbols",
						new List<SketchLayer>
						{
						}
					)
				}
			);
			return document;
		}

		public static SketchSymbolMaster SketchSymbolMaster(string symbolName)
		{
			var sketchPath =
				new SketchPath(
					CurvePointUtils.RectanglePath(
						new CornerRadius(0)),
					true);
			var rectangle = new SketchRectangle(
				SketchLayer(frame: Frame),
				sketchPath, SketchBooleanOperation.NoOperation);

			var sketchSymbolMaster = new SketchSymbolMaster(
					SketchLayer(id:Guid.NewGuid(), name:symbolName, frame: Frame,
					children: new List<SketchLayer> {rectangle}),
				Guid.NewGuid()
			);
			rectangle.Parent = sketchSymbolMaster;
			return sketchSymbolMaster;
		}

		public static SketchRectangle Rectangle(string name)
		{
			return new SketchRectangle(
				SketchLayer(id: Guid.NewGuid(), name: name, frame: Frame),
				new SketchPath(CurvePointUtils.RectanglePath(new CornerRadius(0)),
							   true),
				SketchBooleanOperation.NoOperation);
		}

		public static SketchSymbolMaster SketchSymbolMasterWithNoLayers(string symbolName)
		{
			return new SketchSymbolMaster(
				SketchLayer(id: Guid.NewGuid(), name:symbolName, frame:Frame),
				Guid.NewGuid());
		}

		private static SketchLayer SketchLayer(
			Guid id = default(Guid),
			SketchLayer parent = null,
			SketchRect frame = null,
			string name = null,
			bool nameIsFixed = false,
			double rotation = 0,
			bool flippedVertically = false,
			bool flippedHorizontally = false,
			IReadOnlyList<SketchLayer> children = null)
		{
			var alignment = new SketchAlignment(
				new SketchAxisAlignment(false, false, false),
				new SketchAxisAlignment(false, false, false)); //Can make parameter later if needed
			return new SketchLayer(
				id,
				parent,
				frame,
				name,
				nameIsFixed,
				alignment,
				rotation,
				flippedVertically,
				flippedHorizontally,
				Optional.None(),
				children ?? new List<SketchLayer>());
		}

		public static SketchDocument WithSymbol(this SketchDocument document, SketchSymbolMaster symbol)
		{
			// Fixme just find the Symbols page
			Assert.That(document.Pages.Count == 1);
			Assert.That(document.Pages.First().Name == "Symbols");
			var firstPage = document.Pages.First();
			var newLayers = new List<SketchLayer>(firstPage.Layers) {symbol};
			var newPage = new SketchPage(Guid.NewGuid(), firstPage.Frame, firstPage.Name, newLayers);
			return new SketchDocument(document.Id, new List<SketchPage>{newPage});
		}

		public static SketchSymbolMaster WithLayer(this SketchSymbolMaster symbol, SketchLayer layer)
		{
			var newLayers = new List<SketchLayer>(symbol.Layers) {layer};
			var newParent = new SketchLayer(symbol, newLayers);
			var newSymbol = new SketchSymbolMaster(newParent, symbol.SymbolId);
			layer.Parent = newSymbol;
			return newSymbol;
		}

		private static readonly SketchRect Frame = new SketchRect(0, 0, 100, 100, false);
	}
}
