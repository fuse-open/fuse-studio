using NUnit.Framework;

namespace Outracks.Fuse.Tests
{
	[TestFixture]
	class BuildArgsTests
	{
		static string[] _argStrings = new[]
		{
			"",
			"-DLOL1 -DLOL2",
			"  -DLOL1 -DLOL2  ",
			" -DLOL1  \"quoted  string with spaces \" -DLOL2  ",
		};

		static string[][] _argLists = new[]
		{
			new string[0],
			new[] { "-DLOL1", "-DLOL2" },
			new[] { "-DLOL1", "-DLOL2" },
			new[] { "-DLOL1", "quoted  string with spaces ", "-DLOL2" },
		};

		[Test]
		public void GetArgumentList()
		{
			for (var i = 0; i < _argStrings.Length; ++i)
				CollectionAssert.AreEqual(_argLists[i], BuildArgs.GetArgumentList(_argStrings[i]));
		}

		[Test]
		public void ArgumentRoundTrip1()
		{
			foreach (var argList in _argLists)
			{
				var argString = BuildArgs.GetArgumentString(argList);
				CollectionAssert.AreEqual(argList, BuildArgs.GetArgumentList(argString));
			}
		}

		[Test]
		public void ArgumentRoundTrip2()
		{
			foreach (var argString in _argStrings)
			{
				var argList = BuildArgs.GetArgumentList(argString);
				var argString2 = BuildArgs.GetArgumentString(argList);
				CollectionAssert.AreEqual(argList, BuildArgs.GetArgumentList(argString2));
			}
		}
	}
}