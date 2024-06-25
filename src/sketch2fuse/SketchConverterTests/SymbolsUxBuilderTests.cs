using System.IO;
using System.Linq;
using NUnit.Framework;
using SketchConverter.UxBuilder;

namespace SketchConverterTests
{
	[TestFixture]
	public class SymbolsUxBuilderTests
	{
		private string _outputDirectory;

		[SetUp]
		public void FixtureSetup()
		{
			_outputDirectory = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Sketch");
			Directory.CreateDirectory(_outputDirectory);
		}

		[TearDown]
		public void CleanUpTestDirectory()
		{
			// delete files in _outputDirectory between each test method so that we can
			// reuse symbol names and also not leave a mess.
			Directory.Delete(_outputDirectory, true);
		}

		[Test]
		public void NoSymbolsInSketchDocumentProducesInfo()
		{
			var document = DocumentBuilder.SketchDocument();
			var logger = new MessageListLogger();
			var builder = new SymbolsUxBuilder(logger);

			builder.Build(document, _outputDirectory);
			var files = Directory.EnumerateFiles(_outputDirectory);
			Assert.IsEmpty(files);
			Assert.AreEqual(1, logger.Messages.Count);
			Assert.That(logger.Messages.First(), Does.Match("INFO.*No UX generated"));
		}

		[Test]
		public void OneSymbolCreateOneUxClass()
		{
			var sketchSymbolMaster = DocumentBuilder.SketchSymbolMaster("MySymbol");
			var document = DocumentBuilder.SketchDocument()
				.WithSymbol(sketchSymbolMaster);

			var logger = new MessageListLogger();
			var builder = new SymbolsUxBuilder(logger);

			builder.Build(document, _outputDirectory);

			var files = Directory.EnumerateFiles(_outputDirectory);
			Assert.AreEqual(1, files.Count());
			Assert.IsTrue(files.First().EndsWith("MySymbol.ux"));
		}


		[Test]
		public void TwoSketchSymbolsProducesTwoUxClassesInTwoFiles()
		{
			var document = DocumentBuilder.SketchDocument()
				.WithSymbol(DocumentBuilder.SketchSymbolMaster("MySymbol").WithLayer(DocumentBuilder.Rectangle("R")))
				.WithSymbol(DocumentBuilder.SketchSymbolMaster("MySymbol2").WithLayer(DocumentBuilder.Rectangle("R2")));

			var logger = new MessageListLogger();
			var builder = new SymbolsUxBuilder(logger);

			builder.Build(document, _outputDirectory);


			var files = Directory.EnumerateFiles(_outputDirectory).ToList();
			Assert.AreEqual(2, files.Count());
			Assert.IsTrue(files.First().EndsWith("MySymbol.ux"));
			Assert.IsTrue(files.Last().EndsWith("MySymbol2.ux"));
			Assert.AreEqual(0, logger.ErrorsAndWarnings().Count);
		}

		[Test]
		public void NestedSketchSymbolCreatesUxInstance()
		{
			var childSymbol = DocumentBuilder.SketchSymbolMaster("Inner").WithLayer(DocumentBuilder.Rectangle("R1"));

			var document = DocumentBuilder.SketchDocument()
				.WithSymbol(childSymbol)
				.WithSymbol(DocumentBuilder.SketchSymbolMaster("Outer").WithLayer(DocumentBuilder.SymbolInstanceOf(childSymbol)));

			var logger = new MessageListLogger();
			var builder = new SymbolsUxBuilder(logger);
			builder.Build(document, _outputDirectory);

			var files = Directory.EnumerateFiles(_outputDirectory);
			Assert.AreEqual(2, files.Count());
			Assert.IsTrue(files.First().EndsWith("Inner.ux"));
			Assert.IsTrue(files.Last().EndsWith("Outer.ux"));
			Assert.AreEqual(0, logger.ErrorsAndWarnings().Count);

			var ux = File.ReadAllText(files.Last());

			Assert.IsTrue(ux.Contains("<Sketch.Inner"));
			Assert.IsTrue(ux.Contains("</Sketch.Inner>"));
		}

		[Test]
		public void SymbolWithNoLayersDoesNotGetWritten()
		{
			var document = DocumentBuilder.SketchDocument()
				.WithSymbol(DocumentBuilder.SketchSymbolMasterWithNoLayers("Harry"));

			var logger = new MessageListLogger();
			var builder = new SymbolsUxBuilder(logger);
			builder.Build(document, _outputDirectory);

			var file = Directory.EnumerateFiles(_outputDirectory);
			Assert.AreEqual(0, file.Count());

			Assert.AreEqual(1, logger.ErrorsAndWarnings().Count);
			Assert.That(logger.ErrorsAndWarnings().First(),
				Does.Match("Skipping symbol 'Sketch.Harry' which has no supported layers."));
		}

		[Test]
		public void SymbolWithInvalidCharacterInNameGetsSkippedAndLogged()
		{
			var sketchSymbolMaster = DocumentBuilder.SketchSymbolMaster("a b");
			var document = DocumentBuilder.SketchDocument()
				.WithSymbol(sketchSymbolMaster);

			var logger = new MessageListLogger();
			var builder = new SymbolsUxBuilder(logger);

			builder.Build(document, _outputDirectory);

			var files = Directory.EnumerateFiles(_outputDirectory);
			Assert.That(files.Count(), Is.EqualTo(0));
			Assert.That(logger.ErrorsAndWarnings().Count, Is.EqualTo(1));
			Assert.That(logger.ErrorsAndWarnings()[0], Does.Contain("The symbol name 'a b' contains an invalid character"));
		}

		[Test]
		public void SymbolWithReservedNameGetsSkippedAndLogged()
		{
			var sketchSymbolMaster = DocumentBuilder.SketchSymbolMaster("class");
			var document = DocumentBuilder.SketchDocument()
				.WithSymbol(sketchSymbolMaster);

			var logger = new MessageListLogger();
			var builder = new SymbolsUxBuilder(logger);

			builder.Build(document, _outputDirectory);

			var files = Directory.EnumerateFiles(_outputDirectory);
			Assert.That(files.Count(), Is.EqualTo(0));
			Assert.That(logger.ErrorsAndWarnings().Count, Is.EqualTo(1));
			Assert.That(logger.ErrorsAndWarnings()[0], Does.Contain("The symbol name 'class' is a reserved word."));
		}

		[Test]
		public void TwoSymbolsWithSameNameGivesErrorAndOnlyWritesOne()
		{
			var document = DocumentBuilder.SketchDocument()
				.WithSymbol(DocumentBuilder.SketchSymbolMaster("NameOfTheGame").WithLayer(DocumentBuilder.Rectangle("R")))
				.WithSymbol(DocumentBuilder.SketchSymbolMaster("NameOfTheGame").WithLayer(DocumentBuilder.Rectangle("R")));

			var logger = new MessageListLogger();
			var builder = new SymbolsUxBuilder(logger);

			builder.Build(document, _outputDirectory);


			var files = Directory.EnumerateFiles(_outputDirectory).ToList();
			Assert.That(files.Count(), Is.EqualTo(1));
			Assert.That(files.First(), Does.EndWith("NameOfTheGame.ux"));
			Assert.That(logger.ErrorsAndWarnings().Count, Is.EqualTo(1));
			Assert.That(logger.ErrorsAndWarnings()[0], Does.Match("More than one symbol named 'NameOfTheGame' was found!"));
		}
	}
}
