using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Outracks.Simulator.Bytecode;

namespace Outracks.Simulator.Protocol.Tests.Bytecode
{
	class TypeNameTests
	{
		[Test]
		public void ClassWithoutNamespace()
		{
			var type = TypeName.Parse("Class");
			AssertTypeName("Class", "Class", "Class", new List<string>(), new List<string>(), type);
		}

		[Test]
		public void ClassInNamespace()
		{
			var type = TypeName.Parse("Ns1.Ns2.Class");
			AssertTypeName("Class", "Ns1.Ns2.Class", "Ns1.Ns2.Class", new List<string>(), new List<string>(), type);
		}

		[Test]
		public void TypeWithGenericArgument()
		{
			var type = TypeName.Parse("Ns1.Class<T1>");
			AssertTypeName("Class", "Ns1.Class<T1>", "Ns1.Class`1", new List<string> { "T1" }, new List<string> {"T1"}, type);
		}

		[Test]
		public void TypeWithGenericArguments()
		{
			var type = TypeName.Parse("Ns1.Class<T1,T2>");
			AssertTypeName("Class", "Ns1.Class<T1,T2>", "Ns1.Class`2", new List<string> { "T1", "T2" }, new List<string>{"T1", "T2"}, type);
		}

		[Test]
		public void InnerTypeInGenericClass()
		{
			var type = TypeName.Parse("Ns1.Class<T1,T2>.Inner");
			AssertTypeName("Inner", "Ns1.Class<T1,T2>.Inner", "Ns1.Class`2.Inner", new List<string>(), new List<string> { "T1", "T2" }, type);
		}

		static void AssertTypeName(string classname, string fullName, string withGenericSuffix, List<string> genericArguments, List<string> genericArgumentsRecursively, TypeName typeName)
		{
			Assert.AreEqual(classname, typeName.Surname);
			if (genericArguments.Count > 0)
			{
				var fullNames = typeName.GenericArguments.Select(a => a.FullName);
				Assert.AreEqual(genericArguments, fullNames);
				Assert.AreEqual(classname + "<" + string.Join(",", fullNames) + ">", typeName.Name);
			}
			Assert.AreEqual(genericArgumentsRecursively, typeName.GenericArgumentsRecursively.Select(a => a.FullName));
			Assert.AreEqual(fullName, typeName.FullName);
			Assert.AreEqual(genericArgumentsRecursively.Count > 0, typeName.IsParameterizedGenericType);
			Assert.AreEqual(withGenericSuffix, typeName.WithGenericSuffix.FullName);
			Assert.AreEqual(fullName, typeName.ToString());
			AssertWriteRead(typeName);
		}

		static void AssertWriteRead(TypeName typeName)
		{
			using (var stream = new MemoryStream())
			using (var writer = new BinaryWriter(stream))
			{
				typeName._Write(writer);
				stream.Seek(0, SeekOrigin.Begin);
				using (var reader = new BinaryReader(stream))
				{
					var readTypeName = TypeName._Read(reader);
					Assert.AreEqual(typeName, readTypeName);
				}
			}
		}
	}
}
