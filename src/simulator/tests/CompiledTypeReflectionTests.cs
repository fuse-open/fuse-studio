using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Outracks.Simulator.Client;

namespace Outracks.Simulator.Runtime.Tests
{
	public class MyType
	{
		public void M1(int i) { }
		public void M2(int i, int j = 0) { }

	}

	public class CompiledTypeReflectionTests
	{
		[Test]
		public void MathcesCorrectArguments()
		{
			Assert.True(ParamsFor("M1").ParametersMatch(new object[] { 1 }));
		}

		[Test]
		public void DoesntMatchTooFewArguments()
		{
			Assert.False(ParamsFor("M1").ParametersMatch(new object[] { }));
		}

		[Test]
		public void DoesntMatchTooManyArguments()
		{
			Assert.False(ParamsFor("M1").ParametersMatch(new object[] { 1, 2 }));
		}

		[Test]
		public void DoesntMatchWrongTypes()
		{
			Assert.False(ParamsFor("M1").ParametersMatch(new object[] { "foo" }));
		}

		[Test]
		public void MatchesGivenDefaultArgument()
		{
			Assert.True(ParamsFor("M2").ParametersMatch(new object[] { 1, 2 }));
		}

		[Test]
		public void MatchesNonGivenDefaultArgument()
		{
			Assert.True(ParamsFor("M2").ParametersMatch(new object[] { 1 }));
		}

		[Test]
		public void DoesntMatchGivenDefaultArgumentOfWrongType()
		{
			Assert.False(ParamsFor("M2").ParametersMatch(new object[] { 1, "foo" }));
		}

		static ParameterInfo[] ParamsFor(string method)
		{
			return typeof(MyType).GetMethods().First(m => m.Name == method).GetParameters();
		}
	}
}
