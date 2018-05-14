using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Outracks.Diagnostics;
using Outracks.IO;

namespace Outracks.Fuse.SystemTest
{
	public class SystemTest
	{
		public static int Main(string[] args)
		{
			var tests = new List<Test>
			{
				new Test("preview", PreviewTest.Run),
				new Test("auto-test-app", AutomaticTestApp.Run),
				new Test("create", FuseCreate.Run),
				new Test("build", FuseBuild.Run),
				new Test("install-android", FuseInstallAndroid.Run),
			};

			if (Platform.OperatingSystem == OS.Mac)
			{
				tests.Add(new Test("import", FuseImport.Run));
			}

			if (args.Contains("--help"))
			{
				Console.WriteLine("USAGE: SystemTest.exe --fuse=<path to fuse.exe> [--skip=<test1>,<test2>...]");
				Console.WriteLine();
				Console.WriteLine("Known tests:");
				foreach (var test in tests)
				{
					Console.WriteLine("  " + test.Name);
				}
				return 0;
			}
			var argList = args.ToList();
			var shell = new Shell();
			var fusePath = argList
				.TryParse("fuse")
				.SelectMany(p => ResolveAbsolutePath(shell, p));

			if (!fusePath.HasValue)
			{
				Console.WriteLine("Please specify '--fuse=<path to fuse.exe>'");
				return 1;
			}

			var skip = argList.TryParse("skip").SelectMany(s => s.Split(","));

			var fuseRunner = new FuseRunner(fusePath.Value);
			var root = AbsoluteDirectoryPath.Parse(Directory.GetCurrentDirectory());

			var results = tests.Select(
				t =>
				{
					Console.WriteLine();
					if (skip.Contains(t.Name))
					{
						Console.WriteLine("Skipping test:" + t.Name);
						return new TestResult(t, Result.Skipped);
					}
					else
					{
						Console.WriteLine("Running test: " + t.Name);
						return Run(t, root, fuseRunner);
					}
				}).ToImmutableList();

			Console.WriteLine();
			Console.WriteLine("Test results");
			var fmt = "{0,-" + tests.Select(t => t.Name.Length).Max() + "} : {1}";
			foreach (var testResult in results)
			{
				Console.WriteLine(fmt, testResult.Test.Name, testResult.Result);
			}
			Console.WriteLine();
			return results.Count(r => r.Result == Result.Failed) > 0 ? 1 : 0;
		}

		private static TestResult Run(Test t, AbsoluteDirectoryPath root, FuseRunner fuseRunner)
		{
			try
			{
				t.Run(root, fuseRunner);
				Console.WriteLine(t.Name + " Passed");
				return new TestResult(t, Result.Passed);
			}
			catch (Exception e)
			{
				Console.Error.WriteLine(e.Message);
				Console.WriteLine(t.Name + " Failed");
				return new TestResult(t, Result.Failed);
			}
		}


		static Optional<AbsoluteFilePath> ResolveAbsolutePath(Shell shell, string nativePath)
		{
			try
			{
				return Optional.Some((AbsoluteFilePath)shell.ResolveAbsolutePath(nativePath));
			}
			catch (Exception)
			{
				return Optional.None();
			}
		}

		class Test
		{
			public readonly string Name;
			public readonly Action<AbsoluteDirectoryPath, FuseRunner> Run;
			public Test(string name, Action<AbsoluteDirectoryPath, FuseRunner> run)
			{
				Name = name;
				Run = run;
			}
		}

		class TestResult
		{
			public readonly Test Test;
			public readonly Result Result;
			public TestResult(Test test, Result result)
			{
				Test = test;
				Result = result;
			}
		}
	}

	public class TestFailure : Exception
	{
		public TestFailure(string message ) : base(message)
		{
		}
	}

	public enum Result
	{
		Passed, Skipped, Failed	
	}
}
