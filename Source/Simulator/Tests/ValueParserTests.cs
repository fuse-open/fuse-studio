using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Outracks.Simulator.Tests
{
	[TestFixture]
	class ValueParserTests
	{
		[Test]
		public void TestUXILCompilerConstructorInterface()
		{
			var ctor = ValueParser.GetUxilCompilerCtor();
			Assert.IsNotNull(ctor, "Failed to find an UXIL Constructor.");
			var parameters = ctor.GetParameters().Select(p => p.ParameterType.FullName);
			Assert.That(
				parameters,
				Is.EqualTo(
					new[]
					{
						"Uno.UX.Markup.Reflection.IDataTypeProvider",
						"System.String",
						"Uno.UX.Markup.AST.Element",
						"Uno.UX.Markup.Common.IMarkupErrorLog",
					}),
				"Constructor parameters doesn't match the expected parameters.");
		}
	}
}