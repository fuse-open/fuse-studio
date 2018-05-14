using System.Collections.Generic;
using NUnit.Framework;
using SketchConverter.SketchModel;
using SketchConverter.UxBuilder;

namespace SketchConverterTests
{
	public class SymbolClassNameBuilderTests
	{
		private SymbolClassNameBuilder _builder;
		private SketchSymbolMaster _symbolMaster;

		[SetUp]
		public void SetUp()
		{
			_builder = new SymbolClassNameBuilder();
			_symbolMaster = DocumentBuilder.SketchSymbolMaster("TheArtistFormerlyKnownAsPrince");
			_builder.Init(new List<SketchSymbolMaster> {_symbolMaster});
		}

		[Test]
		public void GivenIdOfExistingSymbolReturnsThat()
		{
			Assert.That(
				_builder.GetClassName(_symbolMaster),
				Does.Contain("TheArtistFormerlyKnownAsPrince"));
		}

		[Test]
		public void GivenIdOfNonExistingSymbolMasterThrows()
		{
			var otherSymbol = DocumentBuilder.SketchSymbolMaster("OtherSymbol");
			var message = "Could not find symbol master 'OtherSymbol' (SymbolId '"+ otherSymbol.SymbolId + "').";
			Assert.That(
				() => _builder.GetClassName(otherSymbol),
				Throws.TypeOf<UxBuilderException>().With.Message.EqualTo(message));
		}

		[Test]
		public void SymbolInstanceReferringToNonExistingSymbolMasterThrows()
		{
			var instance = DocumentBuilder.SymbolInstanceOf(DocumentBuilder.SketchSymbolMaster("OtherMaster"), "Cymbal");
			var message = "The symbol 'Cymbal' is an instance of a symbol we can't find. Could it be that it's defined in another Sketch file? That is currently not supported. (SymbolId '" + instance.SymbolId + "')";
			Assert.That(
				() => _builder.GetClassName(instance),
				Throws.TypeOf<UxBuilderException>().With.Message.EqualTo(message));
		}
	}
}
