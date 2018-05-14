using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using NSubstitute;
using NUnit.Framework;
using SketchConverter;
using SketchConverter.SketchParser;

namespace SketchConverterTests
{
	[TestFixture]
	public class CompatibilityCheckerTest
	{
		[Test]
		public void SketchFileProducedWithNewerVersionThanSupportedGivesWarning()
		{
			var archive = Substitute.For<ISketchArchive>();
			var appVerion = 3.14;
			var version = CompatibilityChecker.SketchCompatibilityVersion + 1;
			var compatibiltyVersion = version;

			var json = GenerateMetaJson(appVerion, compatibiltyVersion, version);
			using (var jsonStream = new MemoryStream(Encoding.ASCII.GetBytes(json))) {
				archive.OpenFile("meta.json")
					   .Returns(jsonStream);

				var log = new MessageListLogger();

				CompatibilityChecker.Check("dummy.sketch", archive, log);

				Assert.AreEqual(1, log.ErrorsAndWarnings().Count);
				Assert.AreEqual(3, log.Messages.Count);
				Assert.That(log.ErrorsAndWarnings().First(),
							Does.StartWith("WARNING:" +"\t" + "The sketch file you are converting to UX has been created with a newer version"));
			}
		}

		[Test]
		public void SketchFileProducedWithOlderVersionThanSupportedGivesWarning()
		{
			var archive = Substitute.For<ISketchArchive>();
			var appVerion = 3.14;
			var version = CompatibilityChecker.SketchCompatibilityVersion - 3;
			var compatibiltyVersion = version;

			var json = GenerateMetaJson(appVerion, compatibiltyVersion, version);
			using (var jsonStream = new MemoryStream(Encoding.ASCII.GetBytes(json))) {
				archive.OpenFile("meta.json")
					   .Returns(jsonStream);

				var log = new MessageListLogger();

				CompatibilityChecker.Check("dummy.sketch", archive, log);

				Assert.AreEqual(1, log.ErrorsAndWarnings().Count);
				Assert.AreEqual(3, log.Messages.Count);
				Assert.That(log.ErrorsAndWarnings().First(),
							Does.StartWith("WARNING:" +"\t" + "The sketch file was created with an older version"));
			}
		}

		[Test]
		public void SketchFileWithoutCompatibilityVersionDoesNotFail()
		{
			// The first sketch-files in the open format doesn't seem to have compatibilityVersion field
			var archive = Substitute.For<ISketchArchive>();
			var appVersion = 3.14;
			var version = 91;

			var json =
				@"{
				'appVersion': '" + appVersion + @"',
				'build': 45422,
				'variant': 'NONAPPSTORE',
				'version': '" + version + @"'
			}";

			using (var jsonStream = new MemoryStream(Encoding.ASCII.GetBytes(json))) {
				archive.OpenFile("meta.json")
					.Returns(jsonStream);

				var log = new MessageListLogger();

				CompatibilityChecker.Check("dummy.sketch", archive, log);

				Assert.AreEqual(1, log.ErrorsAndWarnings().Count);
				Assert.AreEqual(3, log.Messages.Count);
				Assert.That(log.ErrorsAndWarnings().First(),
					Does.StartWith("WARNING:" +"\t" + "The sketch file was created with an older version"));
			}
		}

		[Test]
		public void IsCultureInsensitive()
		{
			var archive = Substitute.For<ISketchArchive>();
			var appVerion = 3.14;
			var version = CompatibilityChecker.SketchCompatibilityVersion;
			var compatibiltyVersion = version;

			var json = GenerateMetaJson(appVerion, compatibiltyVersion, version);
			using (var jsonStream = new MemoryStream(Encoding.ASCII.GetBytes(json)))
			{
				archive.OpenFile("meta.json")
					.Returns(jsonStream);

				var log = new MessageListLogger();

				Thread.CurrentThread.CurrentCulture = new CultureInfo("nb-NO");
				CompatibilityChecker.Check("dummy.sketch", archive, log);
			}
		}

		private static string GenerateMetaJson(double appVerion,
											  int compatibilityVersion,
											  int version)
		{
			return
			@"{
				'appVersion': '" + appVerion + @"',
				'build': 45422,
				'compatibilityVersion': '" + compatibilityVersion + @"',
				'variant': 'NONAPPSTORE',
				'version': '" + version + @"'
			}";
		}
	}
}
