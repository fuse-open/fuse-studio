using System;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Outracks.IO
{
	[TestFixture]
	public class JsonPathTests
	{
		[Test]
		public void JsonAbsoluteDirectoryPath()
		{
			var path = DirectoryPath.GetTempPath();
			var json = JToken.FromObject(path);
			var path2 = json.ToObject<AbsoluteDirectoryPath>();
			Assert.AreEqual(path, path2);
		}

		[Test]
		public void JsonAbsoluteFilePath()
		{
			var path = FilePath.CreateTempFile();
			var json = JToken.FromObject(path);
			var path2 = json.ToObject<AbsoluteFilePath>();
			Assert.AreEqual(path, path2);
		}

		[Test]
		public void JsonRelativeDirectoryPath()
		{
			var path = RelativeDirectoryPath.Empty / ".." / "Programs" / "Fuse" / "Data";
			var json = JToken.FromObject(path);
			var path2 = json.ToObject<RelativeDirectoryPath>();
			Assert.AreEqual(path, path2);
		}

		[Test]
		public void JsonRelativeFilePath()
		{
			var path = RelativeDirectoryPath.Empty / ".." / "Programs" / "Fuse" / new FileName("Data.dat");
			var json = JToken.FromObject(path);
			var path2 = json.ToObject<RelativeFilePath>();
			Assert.AreEqual(path, path2);
		}


		//Unused method, but maybe someone wants it for manual testing or something?
		#pragma warning disable 0219
		public void Execute()
		{
			var root = AbsoluteDirectoryPath.Parse("root");

			var file1 = root.Combine("templates").Combine("Projects").Combine(new FileName("example.exe"));

			var file2 = root / "templates" / "projects" / new FileName("example.exe");

			IAbsolutePath absFileOrDirectory = file2;

			absFileOrDirectory.Do(
				(AbsoluteFilePath file) => Console.Out.WriteLine("File " + file),
				(AbsoluteDirectoryPath dir) => Console.Out.WriteLine("Dir " + dir));

			IFilePath absOrRelFile = file2;

			absOrRelFile.Do(
				(AbsoluteFilePath abs) => Console.Out.WriteLine("Abs " + abs),
				(RelativeFilePath rel) => Console.Out.WriteLine("Rel " + rel));


			var relativePath = RelativeDirectoryPath.Empty / ".." / "Programs" / "Fuse" / "Data";

			var rebased = root / relativePath;

			var common = root / "a" / "b";
			var nop = common.RelativeTo(common);
			var downUp = (common / "left").RelativeTo(common / "right");
			var downDownUpUp = (common / "left" / "left2").RelativeTo(common / "right" / "right2");
			var downDownDownUpUpUp = (common / "left" / "left2" / "left3").RelativeTo(common / "right" / "right2" / "right3");



			var left = common / "lc" / "ld";
			var right = common / "rc" / "rd" / "re";

			var rightToLeft = left.RelativeTo(right);
			var leftToRight = right.RelativeTo(left);
			var leftToLeft = left.RelativeTo(left);

			var a = common / leftToLeft;
			var b = a == common;
		}
		#pragma warning restore 0219
	}
}