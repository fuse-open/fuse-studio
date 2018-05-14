using System;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Outracks.Fuse.Designer;

namespace Outracks.Fuse.Protocol.Tests.Main
{
	class GlVersionParsingTests
	{
		IReport _report;

		[SetUp]
		public void SetUp()
		{
			_report = Substitute.For<IReport>();
		}

		[Test]
		public void ReportsVersionAndVendorAndRenderer()
		{
			var messages = GlVersionParsing.GetLogMessages(new OpenGlVersion("3.1", "Acme", "3Dfx Voodoo"), _report).ToList();
			Assert.AreEqual("OpenGL Version: 3.1\n", messages[0]);
			Assert.AreEqual("OpenGL Vendor: Acme\n", messages[1]);
			Assert.AreEqual("OpenGL Renderer: 3Dfx Voodoo\n", messages[2]);
		}

		[Test]
		public void WarnsWhenCantParseGlVersion()
		{
			var messages = GlVersionParsing.GetLogMessages(new OpenGlVersion("lol.meh", "Acme", "3Dfx Voodoo"), _report).ToList();
			Assert.Contains("Warning: Failed to detect OpenGL version. The required version is 2.1, your computer reports 'lol.meh'", messages);
			_report.Received(1).Warn("OpenGL parsing error: 'lol.meh'", ReportTo.Headquarters | ReportTo.Log);
		}

		[Test]
		public void ErrorsWhenOpenGlVersionIsNotSupported()
		{

			var messages = GlVersionParsing.GetLogMessages(new OpenGlVersion("1.1.0", "Acme", "3Dfx Voodoo"), _report).ToList();
			var expected = "Error: The required OpenGL version is 2.1, your computer reports '1.1.0'";
			_report.Received(1).Error(expected, ReportTo.Log);
			Assert.Contains(expected, messages);
		}

		[Test]
		public void NoErrorsOrWarningsWhenEverythingIsFine()
		{
			var messages = GlVersionParsing.GetLogMessages(new OpenGlVersion("3.1", "Acme", "3Dfx Voodoo"), _report).ToList();
			Assert.IsFalse(messages.Any(s => s.Contains("Warning")));
			Assert.IsFalse(messages.Any(s => s.Contains("Error")));
		}

		[Test]
		public void ParsesExpectedStrings()
		{
			Assert.AreEqual(new Version(4, 5), CreateAndParse("4.5.0 NVIDIA 382.05").Value);
			Assert.AreEqual(new Version(3, 1), CreateAndParse("3.1 blah 1.2").Value);
			Assert.AreEqual(new Version(1, 1), CreateAndParse("1.1 foo").Value);
			Assert.AreEqual(new Version(2, 3), CreateAndParse("  2.3 foo  ").Value);
			Assert.AreEqual(new Version(1, 5), CreateAndParse("1.5").Value);
			Assert.AreEqual(new Version(2, 3), CreateAndParse("  2.3  ").Value);
			Assert.AreEqual(new Version(1, 1), CreateAndParse("1.1.0").Value); //Gode gamle GDI Generic
			Assert.AreEqual(new Version(4, 4), CreateAndParse("4.4.12874 Compatibility Profile Context 14.100.0.0").Value);
		}

		[Test]
		//These are too scary to rely on, better to fail than to get the wrong version
		public void FailsOnStringsWeCantRelyOn()
		{
			Assert.IsFalse(CreateAndParse("Blah 2.1").HasValue);
			Assert.IsFalse(CreateAndParse("2.1lol").HasValue);
			Assert.IsFalse(CreateAndParse("1.2.3.4").HasValue);
		}

		static Optional<Version> CreateAndParse(string glVersion)
		{
			return new OpenGlVersion(glVersion, "", "").ToVersion();
		}

		[Test]
		public void IsSupported()
		{
			Assert.IsTrue(new Version(2, 1).IsSupported());
			Assert.IsTrue(new Version(3, 2).IsSupported());
			Assert.IsFalse(new Version(2, 0).IsSupported());
			Assert.IsFalse(new Version(1, 0).IsSupported());
			Assert.IsFalse(new Version(0, 0).IsSupported());
		}
	}
}
