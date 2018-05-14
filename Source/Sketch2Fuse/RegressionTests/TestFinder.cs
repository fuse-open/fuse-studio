using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RegressionTests
{
	internal static class TestFinder
	{
		public static readonly string SketchFilesDir = Path.Combine("files", "Sketch43");
		private static readonly string TestDirectory = FindTestDirectory();

		public static IEnumerable<Test> FindTests()
		{
			return Directory.GetFiles(TestDirectory, "*.sketch")
				.Select(path => new Test(Path.GetFileNameWithoutExtension(path), path));
		}

		private static string FindTestDirectory()
		{
			var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
			while (directory.Parent != null)
			{
				var maybe = Path.Combine(directory.FullName, SketchFilesDir);
				if (Directory.Exists(maybe))
				{
					return maybe;
				}
				directory = directory.Parent;
			}
			throw new Exception("Didn't find test directory");
		}
	}
}
