using NUnit.Framework;
using RegressionTests;

namespace RegressionTestsTests
{
	public class OutputCleanerTests
	{
		[Test]
		public void LeavesMostThingsAlone_WhichSoundsNiceWhyDontPeopleJustLeaveMeAloneToo()
		{
			Assert.That(OutputCleaner.RemovePaths(""), Is.EqualTo(""));
			Assert.That(OutputCleaner.RemovePaths("foo"), Is.EqualTo("foo"));
			Assert.That(OutputCleaner.RemovePaths("not/a/path.ux"), Is.EqualTo("not/a/path.ux"));
			Assert.That(OutputCleaner.RemovePaths("not\\a\\path.ux"), Is.EqualTo("not\\a\\path.ux"));
		}

		[Test]
		public void RemovesMacPaths()
		{
			Assert.AreEqual("<path removed>lol.ux", OutputCleaner.RemovePaths("/tmp/lol.ux"));
			Assert.AreEqual("<path removed>lal.sketch", OutputCleaner.RemovePaths("/var/tmp/lal.sketch"));
			Assert.AreEqual("<path removed>lal.ux", OutputCleaner.RemovePaths("/var/t-m-p/lal.ux"));
			Assert.AreEqual(
				"INFO: Wrote 'Sketch.FillFlatColor' to '<path removed>Sketch.FillFlatColor.ux'",
				OutputCleaner.RemovePaths("INFO: Wrote 'Sketch.FillFlatColor' to '/var/folders/dj/qjbkn7k520n2229lwtx1hg6c0000gn/T/SketchConverterRegressionTests/AllFillTypes/Sketch.FillFlatColor.ux'"));
			Assert.AreEqual(
				"INFO: Converting <path removed>AllFillTypes.sketch created with Sketch 48.2 variant NONAPPSTORE build97",
				OutputCleaner.RemovePaths("INFO: Converting /Users/knatten/code/SketchImporter/files/Sketch43/AllFillTypes.sketch created with Sketch 48.2 variant NONAPPSTORE build97"));
			Assert.AreEqual(
				"Running : desert-moon-zwj-sketch43 (<path removed>desert-moon-zwj-sketch43.sketch)",
				OutputCleaner.RemovePaths("Running : desert-moon-zwj-sketch43 (/Users/knatten/code/SketchImporter/files/Sketch43/desert-moon-zwj-sketch43.sketch)"));
		}

		[Test]
		public void RemovesWindowsPaths()
		{
			Assert.AreEqual(@"<path removed>lol.ux", OutputCleaner.RemovePaths(@"C:\dir\lol.ux"));
			Assert.AreEqual(@"<path removed>lal.sketch", OutputCleaner.RemovePaths(@"C:\dir\dar\lal.sketch"));
			Assert.AreEqual(@"<path removed>lal.ux", OutputCleaner.RemovePaths(@"C:\dir\d-a-r\lal.ux"));
			Assert.AreEqual(
				@"INFO: Wrote 'Sketch.FillFlatColor' to '<path removed>Sketch.FillFlatColor.ux'",
				OutputCleaner.RemovePaths(@"INFO: Wrote 'Sketch.FillFlatColor' to 'C:\BuildAgent-1\temp\buildTmp\SketchConverterRegressionTests\AllFillTypes\Sketch.FillFlatColor.ux'"));
			Assert.AreEqual(
				@"INFO: Converting <path removed>AllFillTypes.sketch created with Sketch 48.2 variant NONAPPSTORE build97",
				OutputCleaner.RemovePaths(@"INFO: Converting C:\BuildAgent-1\temp\buildTmp\SketchConverterRegressionTests\AllFillTypes\AllFillTypes.sketch created with Sketch 48.2 variant NONAPPSTORE build97"));
		}
	}
}
