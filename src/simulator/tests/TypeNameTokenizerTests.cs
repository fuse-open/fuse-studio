using System.Collections.Generic;
using NUnit.Framework;
using Outracks.Simulator.Bytecode;

namespace Outracks.Simulator.Protocol.Tests
{
	class TypeNameTokenizerTests
	{
		[Test]
		public void TokenizesSingleClass()
		{
			Assert.AreEqual(new List<string> { "Class" }, TypeNameTokenizer.Tokenize("Class"));
		}

		[Test]
		public void TokenizesClassInNamespace()
		{
			Assert.AreEqual(new List<string> { "Ns", ".", "Class" }, TypeNameTokenizer.Tokenize("Ns.Class"));
		}

		[Test]
		public void TokenizesGenericClass()
		{
			Assert.AreEqual(new List<string> { "Class", "" + "<", "T", ">" }, TypeNameTokenizer.Tokenize("Class<T>"));
		}

		[Test]
		public void TokenizesGenericClassWithMultipleTypes()
		{
			Assert.AreEqual(
				new List<string> { "Class", "<", "T1", ",", "T2", ">" },
				TypeNameTokenizer.Tokenize("Class<T1,T2>"));
		}

		[Test]
		public void TokenizesComplicatedType()
		{
			Assert.AreEqual(
				new List<string> { "Ns", ".", "Class", "<", "T1", ",", "T2", ".", "T3", ">", ".", "Delegate" },
				TypeNameTokenizer.Tokenize("Ns.Class<T1,T2.T3>.Delegate"));
		}
	}
}
