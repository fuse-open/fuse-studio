using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;
using Outracks.IO;

namespace Fuse.Preview.Tests
{
	public class CreateOrReuseOutputDir
	{
		static OutputDirWithLock TestOutputDirGen(RelativeDirectoryPath basePath)
		{
			var shell = new Shell();
			var generator = new OutputDirGenerator(shell);
			var baseP = Assembly.GetExecutingAssembly().GetCodeBaseFilePath().ContainingDirectory / basePath;
			return generator.CreateOrReuseOutputDir(baseP);
		}

		[Test]
		public void TestDirInUse()
		{
			var name0 = "TestDirInUse";
			var name1 = "TestDirInUse1";

			var result0 = TestOutputDirGen(RelativeDirectoryPath.Parse(name0));
			var result1 = TestOutputDirGen(RelativeDirectoryPath.Parse(name0));

			// Expects the base path to
			Assert.True(result0.OutputDir.Name == new DirectoryName(name0));
			Assert.True(result1.OutputDir.Name == new DirectoryName(name1));
		}

		[Test]
		public void TestDirInUseSimultaneously()
		{
			var name = "TestDirInUseSim";

			var listOfPossibilities = new List<DirectoryName>
			{
				new DirectoryName("TestDirInUseSim"),
				new DirectoryName("TestDirInUseSim1"),
				new DirectoryName("TestDirInUseSim2"),
				new DirectoryName("TestDirInUseSim3"),
				new DirectoryName("TestDirInUseSim4")
			};

			var result0 = Task.Run(() => TestOutputDirGen(RelativeDirectoryPath.Parse(name)));
			var result1 = Task.Run(() => TestOutputDirGen(RelativeDirectoryPath.Parse(name)));
			var result2 = Task.Run(() => TestOutputDirGen(RelativeDirectoryPath.Parse(name)));
			var result3 = Task.Run(() => TestOutputDirGen(RelativeDirectoryPath.Parse(name)));
			var result4 = Task.Run(() => TestOutputDirGen(RelativeDirectoryPath.Parse(name)));

			// Expects the base path to
			Assert.True(listOfPossibilities.Remove(result0.Result.OutputDir.Name));
			Assert.True(listOfPossibilities.Remove(result1.Result.OutputDir.Name));
			Assert.True(listOfPossibilities.Remove(result2.Result.OutputDir.Name));
			Assert.True(listOfPossibilities.Remove(result3.Result.OutputDir.Name));
			Assert.True(listOfPossibilities.Remove(result4.Result.OutputDir.Name));

			foreach (var r in new[] { result0, result1, result2, result3, result4 })
			{
				try
				{
					r.Result.LockFile.Dispose();
				}
				catch (Exception e)
				{
					Console.WriteLine("Got error while trying to dispose lock in {0}: {1}", r.Result.OutputDir, e);
					throw;
				}
			}
		}

		[Test]
		public void TestDirReuse()
		{
			var name0 = "TestDirReuse";

			var result0 = TestOutputDirGen(RelativeDirectoryPath.Parse(name0));
			result0.LockFile.Dispose();
			var result1 = TestOutputDirGen(RelativeDirectoryPath.Parse(name0));

			// Expects the base path to
			Assert.True(result0.OutputDir.Name == new DirectoryName(name0));
			Assert.True(result1.OutputDir.Name == new DirectoryName(name0));
		}
	}
}