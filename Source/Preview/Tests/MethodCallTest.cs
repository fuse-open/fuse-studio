using System.IO;
using NUnit.Framework;

namespace Fuse.Preview.Tests
{
	[TestFixture]
	class MethodCallTest
	{
		const string Arg1 = "one";
		const int Arg2 = 7;
		const bool Arg3 = true;
		static readonly string[] Arg4 = { ".is", "91=", "1" };
			
		[Test]
		public void Parsing()
		{
			var firstCall = MethodCall.FromExpression((TestClass test) => test.FirstMethod(Arg1, Arg2, Arg3, Arg4));
			Assert.That(firstCall.Method, Is.EqualTo("FirstMethod"));
			Assert.That(firstCall.Arguments, Is.EquivalentTo(new object[] { Arg1, Arg2, Arg3, Arg4 }));

			var secondCall = MethodCall.FromExpression((TestClass test) => test.SecondMethod(Arg1, Arg2, Arg3, Arg4));
			Assert.That(secondCall.Method, Is.EqualTo("SecondMethod"));
			Assert.That(secondCall.Arguments, Is.EquivalentTo(new object[] { Arg1, Arg2, Arg3, Arg4 }));
		}

		[Test]
		public void Encoding()
		{
			var call = MethodCall.FromExpression((TestClass test) => test.FirstMethod(Arg1, Arg2, Arg3, Arg4));

			using (var stream = new MemoryStream())
			using (var reader = new BinaryReader(stream))
			using (var writer = new BinaryWriter(stream))
			{
				call.WriteTo(writer);
				var length = stream.Position;
				stream.Seek(0, SeekOrigin.Begin);
				var readCall = MethodCall.ReadFrom(reader);
				Assert.That(readCall.Method, Is.EqualTo(call.Method));
				Assert.That(readCall.Arguments, Is.EquivalentTo(call.Arguments));
				Assert.That(stream.Position, Is.EqualTo(length));
			}
		}

		[Test]
		public void Invoking()
		{
			var firstCall = MethodCall.FromExpression((TestClass test) => test.FirstMethod(Arg1, Arg2, Arg3, Arg4));
			var firstTest = new TestClass();
			firstCall.InvokeOn(firstTest);
			Assert.That(firstTest.Called, Is.EqualTo("FirstMethod"));
			Assert.That(firstTest.Arg1, Is.EqualTo(Arg1));
			Assert.That(firstTest.Arg2, Is.EqualTo(Arg2));
			Assert.That(firstTest.Arg3, Is.EqualTo(Arg3));
			Assert.That(firstTest.Arg4, Is.EqualTo(Arg4));

			var secondCall = MethodCall.FromExpression((TestClass test) => test.SecondMethod(Arg1, Arg2, Arg3, Arg4));
			var secondTest = new TestClass();
			secondCall.InvokeOn(secondTest);
			Assert.That(secondTest.Called, Is.EqualTo("SecondMethod"));
			Assert.That(secondTest.Arg1, Is.EqualTo(Arg1));
			Assert.That(secondTest.Arg2, Is.EqualTo(Arg2));
			Assert.That(secondTest.Arg3, Is.EqualTo(Arg3));
			Assert.That(secondTest.Arg4, Is.EqualTo(Arg4));
		}
	}

	class TestClass
	{
		public string Called = "";
		public string Arg1 = null;
		public int Arg2 = -1;
		public bool Arg3 = false;
		public string[] Arg4 = null;

		public void FirstMethod(string arg1, int arg2, bool arg3, string[] arg4)
		{
			Called = "FirstMethod";
			Arg1 = arg1;
			Arg2 = arg2;
			Arg3 = arg3;
			Arg4 = arg4;
		}

		public void SecondMethod(string arg1, int arg2, bool arg3, string[] arg4)
		{
			Called = "SecondMethod";
			Arg1 = arg1;
			Arg2 = arg2;
			Arg3 = arg3;
			Arg4 = arg4;
		}
	}
}
