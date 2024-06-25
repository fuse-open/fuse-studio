using System;
using System.IO;
using System.Linq;
using System.Text;
using NSubstitute;
using NUnit.Framework;
using SketchConverter;
using SketchConverter.API;
using SketchConverter.SketchModel;
using SketchConverter.SketchParser;

namespace SketchConverterTests
{
	[TestFixture]
	public class SketchParserTests
	{
		[Test]
		public void ParseLayerWithoutVerticalFlipPropertyDoesNotCrash()
		{
			var mockSketchArchive = MockSketchArchive.Create(Snippets.ShapeWithoutFlippedProps);
			var log = new MessageListLogger();

			var sketchDocument = new SketchParser(log).Parse(mockSketchArchive);

			var shapePath = sketchDocument.Pages[0].Layers[0] as SketchShapePath;
			Assert.NotNull(shapePath);
			Assert.That(shapePath.Path.IsClosed, Is.False);
			Assert.That(shapePath.IsFlippedVertical, Is.False);
			Assert.That(shapePath.IsFlippedHorizontal, Is.False);
			Assert.That(log.ErrorsAndWarnings().Count, Is.Zero);

		}

		[Test]
		public void ParseLayerWithVerticalFlip()
		{
			var mockSketchArchive = MockSketchArchive.Create(Snippets.ShapeWithVerticalFlip);
			var log = new MessageListLogger();

			var sketchDocument = new SketchParser(log).Parse(mockSketchArchive);

			var shapePath = sketchDocument.Pages[0].Layers[0] as SketchShapePath;
			Assert.NotNull(shapePath);
			Assert.That(shapePath.Path.IsClosed, Is.False);
			Assert.That(shapePath.IsFlippedVertical, Is.True);
			Assert.That(shapePath.IsFlippedHorizontal, Is.False);
			Assert.That(log.ErrorsAndWarnings().Count, Is.Zero);

		}

		[Test]
		public void ParseShapeGroupWithClippingMaskOn()
		{
			var mockSketchArchive = MockSketchArchive.Create(Snippets.MaskedShapeGroup);
			var log = Substitute.For<ILogger>();

			var sketchDocument = new SketchParser(log).Parse(mockSketchArchive);

			Assert.That(sketchDocument.Pages.Count, Is.EqualTo(1));
			var shapeGroup = sketchDocument.Pages.First().Layers.First() as SketchShapeGroup;
			Assert.That(shapeGroup, Is.Not.Null);
			Assert.That(shapeGroup.HasClippingMask, Is.True);
		}

		[Test]
		public void ParseSketchRectangleTest()
		{
			var mockSketchArchive = MockSketchArchive.Create(Snippets.RectangleJson);
			var log = Substitute.For<ILogger>();

			var sketchDocument = new SketchParser(log).Parse(mockSketchArchive);

			Assert.AreEqual(1, sketchDocument.Pages.Count);
			Assert.NotNull(sketchDocument.Pages[0].Layers);
			Assert.IsInstanceOf(typeof(SketchRectangle), sketchDocument.Pages[0].Layers[0]);

		}

		[Test]
		public void ParseSketchSymbolWithRectangleTest()
		{
			var mockSketchArchive = MockSketchArchive.Create(Snippets.SymbolMaster);
			var log = Substitute.For<ILogger>();

			var sketchDocument = new SketchParser(log).Parse(mockSketchArchive);

			Assert.NotNull(sketchDocument);
			Assert.AreEqual(1, sketchDocument.Pages.Count);
			Assert.NotNull(sketchDocument.Pages[0].Layers);
			Assert.IsInstanceOf(typeof(SketchSymbolMaster), sketchDocument.Pages[0].Layers[0]);

		}

		[Test]
		public void ParseNestedSketchSymbolTest()
		{
			var mockSketchArchive = MockSketchArchive.Create(Snippets.SymbolMasterWithInstance);
			var log = Substitute.For<ILogger>();

			var sketchDocument = new SketchParser(log).Parse(mockSketchArchive);
			Assert.NotNull(sketchDocument);
			Assert.AreEqual(1, sketchDocument.Pages.Count);
			Assert.NotNull(sketchDocument.Pages[0].Layers);
			Assert.IsInstanceOf<SketchSymbolMaster>(sketchDocument.Pages[0].Layers[0]);

			var symbolMaster = sketchDocument.Pages[0].Layers[0] as SketchSymbolMaster;
			Assert.AreEqual("Rectangle", symbolMaster.Name);
			Assert.AreEqual(1, symbolMaster.Layers.Count(l => l.GetType() == typeof(SketchSymbolInstance)));
		}

		[Test]
		public void ParseShapePathWithStraightLine()
		{
			var mockSketchArchive = MockSketchArchive.Create(Snippets.ShapePathJson);
			var log = Substitute.For<ILogger>();

			var sketchDocument = new SketchParser(log).Parse(mockSketchArchive);
			Assert.NotNull(sketchDocument);
			Assert.AreEqual(1, sketchDocument.Pages.Count);
			Assert.NotNull(sketchDocument.Pages[0].Layers);
			Assert.IsInstanceOf<SketchShapePath>(sketchDocument.Pages[0].Layers[0]);

			var shapePath = sketchDocument.Pages[0].Layers[0] as SketchShapePath;
			Assert.IsFalse(shapePath.Path.IsClosed);
			Assert.AreEqual(2, shapePath.Path.Points.Count);
		}

		[Test]
		public void ParseShapePathWithLinesAndCurves()
		{
			var mockSketchArchive = MockSketchArchive.Create(Snippets.VectorPath);
			var log = Substitute.For<ILogger>();

			var sketchDocument = new SketchParser(log).Parse(mockSketchArchive);

			var shapePath = sketchDocument.Pages[0].Layers[0] as SketchShapePath;
			Assert.NotNull(shapePath);
			Assert.IsFalse(shapePath.Path.IsClosed);
			Assert.AreEqual(11, shapePath.Path.Points.Count);

			Assert.AreEqual(6, shapePath.Path.Points.Count(p => p.Mode == SketchCurvePoint.CurveMode.Line));
			Assert.AreEqual(5, shapePath.Path.Points.Count(p => p.Mode == SketchCurvePoint.CurveMode.Curve));
		}

		[Test]
		public void ParseStarShape()
		{
			var mockSketchArchive = MockSketchArchive.Create(Snippets.Star);
			var log = Substitute.For<ILogger>();

			var sketchDocument = new SketchParser(log).Parse(mockSketchArchive);

			var shapePath = sketchDocument.Pages[0].Layers[0] as SketchShapePath;
			Assert.NotNull(shapePath);
			Assert.AreEqual(10, shapePath.Path.Points.Count);
			Assert.AreEqual(10, shapePath.Path.Points.Count(p => p.Mode == SketchCurvePoint.CurveMode.Line));
			Assert.That(shapePath.Path.IsClosed, Is.True);
		}

		[Test]
		public void ParseCurve()
		{
			var mockSketchArchive = MockSketchArchive.Create(Snippets.Path1);
			var log = Substitute.For<ILogger>();

			var sketchDocument = new SketchParser(log).Parse(mockSketchArchive);

			var shapePath = sketchDocument.Pages[0].Layers[0] as SketchShapePath;
			Assert.NotNull(shapePath);
			Assert.That(shapePath.Path.IsClosed, Is.False);
			Assert.AreEqual(2, shapePath.Path.Points.Count);
			Assert.AreEqual(1, shapePath.Path.Points.Count(p => p.Mode == SketchCurvePoint.CurveMode.Line));
			Assert.AreEqual(1, shapePath.Path.Points.Count(p => p.Mode == SketchCurvePoint.CurveMode.Curve));
		}

		[Test]
		public void ParseThreePntCurve()
		{
			var mockSketchArchive = MockSketchArchive.Create(Snippets.Path2);
			var log = Substitute.For<ILogger>();

			var sketchDocument = new SketchParser(log).Parse(mockSketchArchive);

			var shapePath = sketchDocument.Pages[0].Layers[0] as SketchShapePath;
			Assert.NotNull(shapePath);
			Assert.That(shapePath.Path.IsClosed, Is.False);
			Assert.AreEqual(3, shapePath.Path.Points.Count);
			Assert.AreEqual(2, shapePath.Path.Points.Count(p => p.Mode == SketchCurvePoint.CurveMode.Line));
			Assert.AreEqual(1, shapePath.Path.Points.Count(p => p.Mode == SketchCurvePoint.CurveMode.Curve));
		}

		[Test]
		public void ParseErrorInLayerLogsAndSkips()
		{
			var mockSketchArchive = MockSketchArchive.Create(Snippets.PathInvalidJson);
			var log = new MessageListLogger();

			var document = new SketchParser(log).Parse(mockSketchArchive);
			Assert.That(document.Pages.Count, Is.EqualTo(1));
			Assert.That(document.Pages[0].Layers.Count, Is.EqualTo(0));
			Assert.That(log.ErrorsAndWarnings().Count, Is.EqualTo(1));
			Assert.That(log.ErrorsAndWarnings()[0], Does.Match("Failed to parse layer"));
		}

		[Test]
		public void ParseEmptySketchDocument()
		{
			// Create Sketch document where page has an empty layer list
			// When saving an empty sketch file, the document-class doesn't have an do_objectID

			var mockSketchArchive = Substitute.For<ISketchArchive>();
			mockSketchArchive
				.OpenFile("document.json")
				.Returns(
					new MemoryStream(Encoding.ASCII.GetBytes(
						@"{
							'_class': 'document',
							'pages': []
						 }"
					)));

			var log = new MessageListLogger();

			var sketchDocument = new SketchParser(log).Parse(mockSketchArchive);

			Assert.NotNull(sketchDocument);
		}

		[Test]
		public void UnsupportedLayerWarnsAndSkips()
		{
			var mockSketchArchive = MockSketchArchive.Create(Snippets.JellyfishLayerJson);
			var log = new MessageListLogger();

			var document = new SketchParser(log).Parse(mockSketchArchive);
			Assert.That(document.Pages.Count, Is.EqualTo(1));
			Assert.That(document.Pages[0].Layers.Count, Is.EqualTo(0));
			Assert.That(log.ErrorsAndWarnings().Count, Is.EqualTo(1));
			Assert.That(log.ErrorsAndWarnings()[0], Does.Match("Skipping layer 'Jellyfish' of unsupported type 'jellyfish'"));
		}


		// Hide all the uglyness of building a mock sketch archive
		static class MockSketchArchive
		{
			public static ISketchArchive Create(string pageContent)
			{
				var pageGuid = Guid.NewGuid();
				var document = GenerateDocumentJson(new[] {pageGuid});
				var mockSketchArchive = Substitute.For<ISketchArchive>();
				mockSketchArchive.OpenFile("document.json").Returns(document);
				mockSketchArchive.OpenFile("pages/" + pageGuid + ".json")
					.Returns(GenerateSketchPage(pageGuid, pageContent));
				return mockSketchArchive;
			}

			static Stream GenerateDocumentJson(Guid[] ids)
			{
				var jsondoc =
					@"{""_class"": ""document"", ""do_objectID"": """ + Guid.NewGuid() + @""",""pages"": [";

				foreach (var id in ids)
				{
					jsondoc += PageRef(id);
				}
				jsondoc += @"]}";

				var memstream = new MemoryStream(Encoding.ASCII.GetBytes(jsondoc));

				return memstream;
			}

			static Stream GenerateSketchPage(Guid id, string content)
			{
				var json = @"{""_class"":""page"",""do_objectID"":""" + id + @""",""name"":""somename""," + Frame() +
						   @",""layers"":[";
				json += content;
				json += @"]}";
				var memstream = new MemoryStream(Encoding.ASCII.GetBytes(json));
				return memstream;
			}

			private static string PageRef(Guid id)
			{
				return @"
			{
				""_class"": ""MSJSONFileReference"",
				""_ref"": ""pages/" + id
					   + @""",
				""_ref_class"": ""MSImmutablePage""
			},";
			}

			private static string Frame()
			{
				return @"
				""frame"": {
				""_class"": ""rect"",
				""constrainProportions"": false,
				""height"": 100,
				""width"": 100,
				""x"": 0,
				""y"": 0
			}";
			}
		}
	}
}
