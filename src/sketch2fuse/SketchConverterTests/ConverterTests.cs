using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using SketchConverter;
using SketchConverter.SketchModel;
using SketchConverter.SketchParser;
using SketchConverter.UxBuilder;

namespace SketchConverterTests
{
	[TestFixture]
	public class ConverterTests
	{
		[Test]
		public void WhenOneFileDoesntExist_ReportsErrorAndKeepsConverting()
		{
			var parser = Substitute.For<ISketchParser>();
			var guid = Guid.NewGuid();
			var sketchDocument = new SketchDocument(guid, new List<SketchPage>());
			parser.Parse(Arg.Any<SketchArchive>()).Returns(_ => sketchDocument);

			var builder = Substitute.For<IUxBuilder>();
			var log = new MessageListLogger();
			var converter = new Converter(parser, builder, log);

			var doesntExist = Path.Combine(TempDirectory, "IDontExist.sketch");
			var arrowSketch = Path.Combine(FilesDirectory, "Arrow.sketch");
			converter.Convert(new[] { doesntExist, arrowSketch }, TempDirectory);

			//Got error for the first file
			Assert.That(log.ErrorsAndWarnings().Count, Is.EqualTo(1));
			Assert.That(log.ErrorsAndWarnings()[0], Does.Match("Can't convert non-existing Sketch-file.*IDontExist.sketch"));

			//Called builder for the second file, and only that
			builder.ReceivedWithAnyArgs(1).Build(null, null);
			builder.Received(1).Build(sketchDocument, TempDirectory);
		}

		[Test]
		public void ConvertingNonZipArchiveSketchFileReportsError()
		{
			var invalidSketchArchive = Path.Combine(Path.GetTempPath(), "invalid.sketch");
			using (var sw = new StreamWriter(invalidSketchArchive))
			{
				sw.WriteLine("This is not a sketch archive");
			}

			var log = new MessageListLogger();
			var converter = new Converter(new SketchParser(log), new SymbolsUxBuilder(log), log);
			converter.Convert(new [] {invalidSketchArchive}, Path.GetTempPath());
			Assert.That(log.ErrorsAndWarnings().First(), Does.Match(invalidSketchArchive.Replace(@"\",@"\\") + ".*Can't open sketch file. This file might be on an old format."));

			File.Delete(invalidSketchArchive);
		}

		[Test]
		public void CanBeCalledTwice()
		{
			var log = new MessageListLogger();
			var converter = new Converter(new SketchParser(log), new SymbolsUxBuilder(log), log);
			var arrowSketch = Path.Combine(FilesDirectory, "Arrow.sketch");
			Assert.That(File.Exists(arrowSketch));
			converter.Convert(new [] {arrowSketch}, TempDirectory);
			Assert.That(log.ErrorsAndWarnings(), Is.Empty);
			converter.Convert(new [] {arrowSketch}, TempDirectory);
			Assert.That(log.ErrorsAndWarnings(), Is.Empty);
		}

		[Test]
		public void WhenOneFileFailsParsing_ReportsErrorAndKeepsConverting()
		{
			var parser = Substitute.For<ISketchParser>();
			var guid = Guid.NewGuid();
			var sketchDocument = new SketchDocument(guid, new List<SketchPage>());
			parser.Parse(Arg.Any<SketchArchive>()).Returns(
				_ => { throw new SketchParserException("Whenever I adjust my sail, I fail."); },
				_ => sketchDocument);

			var builder = Substitute.For<IUxBuilder>();
			var log = new MessageListLogger();
			var converter = new Converter(parser, builder, log);

			//Just some arbitrary valid sketch files to make sure `Load` works
			var arrowSketch = Path.Combine(FilesDirectory, "Arrow.sketch");
			var lineSketch = Path.Combine(FilesDirectory, "Line.sketch");
			converter.Convert(new [] {arrowSketch, lineSketch}, TempDirectory);

			//Got error for the first file
			Assert.That(log.ErrorsAndWarnings().Count, Is.EqualTo(1));
			Assert.That(log.ErrorsAndWarnings()[0], Does.Match("Arrow.sketch.*I fail"));

			//Called builder for the second file, and only that
			builder.ReceivedWithAnyArgs(1).Build(null, null);
			builder.Received(1).Build(sketchDocument, TempDirectory);
		}

		[Test]
		//This is important since we for instance parse pages in parallel
		public void ReportsAggregateExceptions()
		{
			var manyExceptions = new AggregateException(new List<Exception> {new Exception("foo"), new Exception("bar")});
			var parser = Substitute.For<ISketchParser>();
			parser.Parse(Arg.Any<SketchArchive>()).Throws(manyExceptions);

			var builder = Substitute.For<IUxBuilder>();
			var log = new MessageListLogger();
			var converter = new Converter(parser, builder, log);

			//Arbitrary existing file, just so we can get to `parser.Parse()`
			var arrowSketch = Path.Combine(FilesDirectory, "Arrow.sketch");

			converter.Convert(new [] {arrowSketch}, TempDirectory);
			Assert.That(log.ErrorsAndWarnings().Count, Is.EqualTo(2));
			Assert.That(log.ErrorsAndWarnings(), Has.Some.Match("foo"));
			Assert.That(log.ErrorsAndWarnings(), Has.Some.Match("bar"));
		}

		static ConverterTests()
		{
			var codeBase = Assembly.GetExecutingAssembly().CodeBase;
			var uri = new UriBuilder(codeBase);
			var path = Uri.UnescapeDataString(uri.Path);
			AssemblyDirectory = Path.GetDirectoryName(path);
			FilesDirectory = Path.Combine(AssemblyDirectory, "..", "..", "..", "files", "Sketch43");
			Assert.That(FilesDirectory, Does.Exist);
			TempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
		}

		[SetUp]
		public void SetUp()
		{
			Directory.CreateDirectory(TempDirectory);
		}

		[TearDown]
		public void TearDown()
		{
			Directory.Delete(TempDirectory, true);
		}

		private static readonly string AssemblyDirectory;
		private static readonly string FilesDirectory;
		private static readonly string TempDirectory;
	}
}
