using System;
using System.IO;
using System.Linq;
using Mono.Options;

namespace RegressionTests
{
	internal class Program
	{
		public static void Main(string[] args)
		{
			var showHelp = false;
			var interactive = false;
			var enableTeamCityMessages = false;
			var options = new OptionSet
			{
				{"h|help", "Print help", v => showHelp = true},
				{"i|interactive", "Interactive mode to prompt for updating reference files", v => interactive = true},
				{"teamcity", "Enable TeamCity service messages", t => enableTeamCityMessages = true},
			};
			options.Parse(args);

			if (showHelp)
			{
				ShowHelp(options);
				return;
			}
			if (RunTests(interactive, enableTeamCityMessages))
			{
				Environment.Exit(1);
			}
		}

		private static void ShowHelp(OptionSet options)
		{
			var message = @"Usage: RegressionTests [<options>]

Runs the converter on all files in SKETCH_FILES_DIR.
Then compares the results to reference files.
Fails if any conversion fails, or a file differs from its reference.

Interactive mode:
Run with -i to get prompted whether to replace the reference files with
the new results

Ignoring tests:
A file in `files/Sketch43` can be ignored by adding a `.ignore` file next
to it, for instance:
```
files/Sketch43/ColoredArialText.sketch
files/Sketch43/ColoredArialText.sketch.ignore
```

Options:
";
			Console.WriteLine(message.Replace("SKETCH_FILES_DIR", TestFinder.SketchFilesDir));
			options.WriteOptionDescriptions(Console.Out);
		}

		private static bool RunTests(bool interactive, bool enableTeamCityMessages)
		{
			Log.Info("Starting test run. Build directory is " + OutputRootDir);
			if (enableTeamCityMessages) TeamCityReporter.TestSuiteStarted();
			var testResults = TestFinder.FindTests().Select(t => enableTeamCityMessages ? RunTestWithTCOutput(t) : RunTest(t)).ToList();
			if (enableTeamCityMessages) TeamCityReporter.TestSuiteFinished();
			Log.Info("");
			Log.Info("SUMMARY:");

			foreach (var testResult in testResults)
			{
				switch (testResult.Result)
				{
					case Result.Failed:
						Log.Error(testResult.ToString());
						break;
					case Result.Passed:
						Log.Success(testResult.ToString());
						break;
					case Result.Ignored:
						Log.Warn(testResult.ToString());
						break;
				}
			}
			var failures = testResults.Count(tr => tr.Result == Result.Failed);
			var passes = testResults.Count(tr => tr.Result == Result.Passed);
			var ignores = testResults.Count(tr => tr.Result == Result.Ignored);

			Log.Info("");
			Log.Info("STATISTICS:");

			Log.Success("Passed tests : " + passes);
			Log.Error("Failed tests : " + failures);
			Log.Warn("Ignored tests: " + ignores);

			if (interactive)
			{
				return !InteractiveDiffAccepter.Run(testResults.Where(t => t.Diff != null).ToList());
			}

			return failures != 0;
		}

		private static TestResult RunTest(Test test)
		{
			Log.Info("");
			if (File.Exists(test.Path + ".ignore"))
			{
				Log.Warn("Ignoring : " + test);
				return new TestResult(test, Result.Ignored);
			}

			Log.Info("Running : " + test);
			var outputDir = Path.Combine(OutputRootDir, test.Name);
			Directory.CreateDirectory(outputDir);
			var result = CommandRunner.Run("-i " + test.Path + " -o " + outputDir);
			if (result.ExitCode != 0)
			{
				Log.Error("Failed to convert: " + test);
				return new TestResult(test, Result.Failed);
			}

			File.WriteAllText(Path.Combine(outputDir, "stdout"), OutputCleaner.RemovePaths(result.StdOut));
			File.WriteAllText(Path.Combine(outputDir, "stderr"), OutputCleaner.RemovePaths(result.StdErr));

			var oldDir = test.Path.Replace(".sketch", ".reference");

			var diffs = DirectoryDiffer.Diff(oldDir, outputDir);

			if (diffs.Any())
			{
				Log.Error("Failed with diff : " + test);
				foreach (var diff in diffs)
				{
					Log.Info(diff.Summary);
					Log.Info(diff.Diff);
				}
				return new TestResult(test, Result.Failed, diffs);
			}

			Log.Success("Passed : " + test);
			return new TestResult(test, Result.Passed);
		}

		private static TestResult RunTestWithTCOutput(Test test)
		{
			TeamCityReporter.TestStarted(test);
			var result = RunTest(test);
			TeamCityReporter.TestResult(result);
			return result;
		}

		private static readonly string OutputRootDir = CreateOutputRootDir();

		private static string CreateOutputRootDir()
		{
			var dir = Path.Combine(Path.GetTempPath() + "SketchConverterRegressionTests");
			if (Directory.Exists(dir))
			{
				Directory.Delete(dir, true);
			}
			Directory.CreateDirectory(dir);
			return dir;
		}

	}
}
