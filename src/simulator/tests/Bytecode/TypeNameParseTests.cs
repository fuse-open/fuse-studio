using NUnit.Framework;
using Outracks.Simulator.Bytecode;

namespace Outracks.Simulator.Protocol.Tests.Bytecode
{
	class TypeNameParserTests
	{
		[Test]
		public void ParsesSingleClass()
		{
			var clas = TypeNameParser.Parse("Class");
			Assert.AreEqual("Class", clas.Surname);
			Assert.False(clas.ContainingType.HasValue);
		}

		[Test]
		public void ParsesClassInNamespace()
		{
			var clas = TypeNameParser.Parse("Ns1.Ns2.Class");
			Assert.AreEqual("Class", clas.Surname);
			var ns2 = clas.ContainingType.Value;
			Assert.AreEqual("Ns2", ns2.Surname);
			var ns1 = ns2.ContainingType.Value;
			Assert.AreEqual("Ns1", ns1.Surname);
			Assert.False(ns1.ContainingType.HasValue);
		}

		[Test]
		public void ParsesSingleGenericClass()
		{
			var clas = TypeNameParser.Parse("Class<T>");
			Assert.AreEqual("Class", clas.Surname);
			Assert.AreEqual(1, clas.GenericArguments.Count);
			var t = clas.GenericArguments.Get(0);
			Assert.AreEqual("T", t.Surname);
			Assert.False(clas.ContainingType.HasValue);
			Assert.False(t.ContainingType.HasValue);
		}

		[Test]
		public void ParsesMultipleGenericClass()
		{
			var clas = TypeNameParser.Parse("Class<T1,T2>");
			Assert.AreEqual("Class", clas.Surname);
			Assert.AreEqual(2, clas.GenericArguments.Count);
			var t1 = clas.GenericArguments.Get(0);
			Assert.AreEqual("T1", t1.Surname);
			var t2 = clas.GenericArguments.Get(1);
			Assert.AreEqual("T2", t2.Surname);
			Assert.False(clas.ContainingType.HasValue);
			Assert.False(t1.ContainingType.HasValue);
			Assert.False(t2.ContainingType.HasValue);
		}

		[Test]
		public void ParsesGenericClassWithNamespacedParameter()
		{
			var clas = TypeNameParser.Parse("Class<T1.T2>");
			Assert.AreEqual("Class", clas.Surname);
			Assert.AreEqual(1, clas.GenericArguments.Count);
			var t2 = clas.GenericArguments.Get(0);
			Assert.AreEqual("T2", t2.Surname);
			var t1 = t2.ContainingType.Value;
			Assert.AreEqual("T1", t1.Surname);
			Assert.False(t1.ContainingType.HasValue);
			Assert.False(clas.ContainingType.HasValue);
		}

		[Test]
		public void ParsesComplicatedType()
		{
			var inner = TypeNameParser.Parse("Ns1.Class<T1,T2.T3>.Inner");
			Assert.AreEqual("Inner", inner.Surname);
			var clas = inner.ContainingType.Value;
			Assert.AreEqual(0, inner.GenericArguments.Count);
			Assert.AreEqual("Class", clas.Surname);
			var ns1 = clas.ContainingType.Value;
			Assert.AreEqual("Ns1", ns1.Surname);
			Assert.False(ns1.ContainingType.HasValue);
			var t1 = clas.GenericArguments.Get(0);
			Assert.AreEqual("T1", t1.Surname);
			var t3 = clas.GenericArguments.Get(1);
			Assert.AreEqual("T3", t3.Surname);
			Assert.AreEqual("T2", t3.ContainingType.Value.Surname);
		}
	}
}
