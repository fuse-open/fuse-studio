using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;

namespace EndToEndTests
{
	public class EndToEnd
	{
		private static readonly string Command;
		private static readonly string AssemblyDirectory;
		private static readonly string FilesDirectory;
		private static readonly string TempDirectory;

		static EndToEnd()
		{
			var codeBase = Assembly.GetExecutingAssembly().CodeBase;
			var uri = new UriBuilder(codeBase);
			var path = Uri.UnescapeDataString(uri.Path);
			AssemblyDirectory = Path.GetDirectoryName(path);
			FilesDirectory = Path.Combine(AssemblyDirectory, "..", "..", "..", "files", "Sketch43");
			Assert.That(FilesDirectory, Does.Exist);
			Command = Path.Combine(AssemblyDirectory, "Command.exe");
			Assert.That(Command, Does.Exist);
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

		[Test]
		[Timeout(10000)]
		public void ByDefaultPrintsUsageAndReturnsNonZero()
		{
			var result = RunToCompletion("");
			Assert.AreEqual(1, result.ExitCode);
			Assert.That(result.StdOut, Does.Contain("Usage"));
		}

		[Test]
		[Timeout(10000)]
		public void PrintsHelpAndReturnsZero()
		{
			var result = RunToCompletion("-h");
			Assert.AreEqual(0, result.ExitCode);
			Assert.That(result.StdOut, Does.Contain("Usage"));
		}

		[Test]
		[Timeout(10000)]
		public void UnknownParameterPrintsErrorAndUsageAndReturnsNonZero()
		{
			var result = RunToCompletion("--use-emacs");
			Assert.AreEqual(1, result.ExitCode);
			Assert.That(result.StdOut, Does.Contain("Unknown argument: '--use-emacs'"));
			Assert.That(result.StdOut, Does.Contain("Usage"));
		}

		[Test]
		[Timeout(10000)]
		public void ConvertsSketchToUx()
		{
			var arguments = "-i=" + Path.Combine(FilesDirectory, "Rectangle.Sketch") + " -o=" + TempDirectory;
			var result = RunToCompletion(arguments);
			Assert.AreEqual(0, result.ExitCode);
			CheckGeneratedUxClass("Sketch.Rectangle");
		}

		[Test]
		[Timeout(10000)]
		public void ConvertsTwoSketchFilesToUx()
		{
			var arguments = "-i=" + Path.Combine(FilesDirectory, "Rectangle.sketch") +
							" -i=" + Path.Combine(FilesDirectory, "Star.sketch") +
							" -o=" + TempDirectory;
			var result = RunToCompletion(arguments);
			Assert.AreEqual(0, result.ExitCode);
			CheckGeneratedUxClass("Sketch.Rectangle");
			CheckGeneratedUxClass("Sketch.Star");
		}

		[Test]
		[Timeout(10000)]
		public void ConvertValidSketchFileToUx()
		{
			var arguments = "-i=" + Path.Combine(FilesDirectory, "Rectangle.sketch") +
							" -i=" + Path.Combine(FilesDirectory, "NoSuchFile.sketch") +
							" -o=" + TempDirectory;
			var result = RunToCompletion(arguments);
			Assert.AreEqual(0, result.ExitCode);
			CheckGeneratedUxClass("Sketch.Rectangle");
			Assert.That(result.StdOut, Does.Contain("Can't convert non-existing Sketch-file"));
		}

		private static void CheckGeneratedUxClass(string classname)
		{
			var uxfile = Path.Combine(TempDirectory, classname + ".ux");
			Assert.That(uxfile, Does.Exist);
			var content = File.ReadAllLines(uxfile);
			Assert.That(content[0], Does.StartWith("<Panel ux:Class=\"" + classname + "\""));
			Assert.That(content.Last(), Is.EqualTo("</Panel>"));
		}

		private static ProcessResult RunToCompletion(string arguments)
		{
			var stdout = new StringBuilder();
			var stderr = new StringBuilder();
			var p = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = Command,
					Arguments = arguments,
					UseShellExecute = false,
					RedirectStandardError = true,
					RedirectStandardOutput = true
				}
			};
			p.OutputDataReceived += (sender, args) => stdout.Append(args.Data ?? "");
			p.ErrorDataReceived += (sender, args) => stderr.Append(args.Data ?? "");
			p.Start();
			p.BeginOutputReadLine();
			p.BeginErrorReadLine();
			p.WaitForExit();
			var result = new ProcessResult(stdout.ToString(), stderr.ToString(), p.ExitCode);
			return result;
		}
	}

	public class ProcessResult
	{
		public readonly string StdOut;
		public readonly string StdErr;
		public readonly int ExitCode;

		public ProcessResult(string stdOut, string stdErr, int exitCode)
		{
			StdOut = stdOut;
			StdErr = stdErr;
			ExitCode = exitCode;
		}
	}
}
