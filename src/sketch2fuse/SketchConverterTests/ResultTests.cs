using System;
using System.Collections.Generic;
using NUnit.Framework;
using SketchConverter;

namespace SketchConverterTests
{
	public class ResultTests
	{
		[Test]
		public void OkIsOk()
		{
			Assert.That(Result.Ok("yeah"), Is.InstanceOf(typeof(Ok<string>)));
			Assert.That(Result.Ok(42), Is.InstanceOf(typeof(Ok<int>)));
		}

		[Test]
		public void ErrIsErr()
		{
			Assert.That(Result.Err<double>("Couldn't parse that one!"), Is.InstanceOf(typeof(Err<double>)));
		}

		[Test]
		public void ExpectReturnsValueWhenOk()
		{
			Assert.That(
				Result.Ok(2).Expect(),
				Is.EqualTo(2));
		}

		[Test]
		public void ExpectThrowsWhenErr()
		{
			var e = Result.Err<int>("Failed!");

			Assert.That(
				() => e.Expect(),
				Throws.TypeOf<Exception>().With.Message.EqualTo("Failed!"));

			Assert.That(
				() => e.Expect("Expected a value"),
				Throws.TypeOf<Exception>().With.Message.EqualTo("Expected a value: Failed!"));
		}

		[Test]
		public void ConvenienceOperators()
		{
			var v = OneReasonResultOperatorIsUseful();
			Assert.That(v.Expect(), Is.EqualTo(42));

			var e = OneReasonErrorOperatorIsUseful();
			Assert.That(() => e.Expect(), Throws.TypeOf<Exception>().With.Message.EqualTo("nope"));

			//Another reason operators are nice:
			#pragma warning disable 0219
			var values = new List<Result<int>> {1, 2, 3, Result.Err("failed"), 5};
			#pragma warning restore 0219
		}

		private static Result<int> OneReasonResultOperatorIsUseful()
		{
			return 42; //Don't have to write "return Result.Ok(42)"
		}

		private static Result<int> OneReasonErrorOperatorIsUseful()
		{
			return Result.Err("nope"); //Don't have to specify type: "return Result.Err<int>("nope")"
		}

		[Test]
		public void SelectOkSelectsOk()
		{
			var values = new List<Result<int>>() {Result.Ok(1), Result.Err<int>("failed")};
			var ok = values.SelectOk();
			Assert.That(ok, Is.EqualTo(new List<int>(){1}));
		}
	}
}
