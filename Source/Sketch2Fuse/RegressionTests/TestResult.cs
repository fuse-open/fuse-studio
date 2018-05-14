using System.Collections.Generic;

namespace RegressionTests
{
	internal class TestResult
	{
		public readonly Test Test;
		public readonly Result Result;
		public readonly List<FileDiff> Diff;

		public TestResult(Test test, Result result, List<FileDiff> diff = null )
		{
			Test = test;
			Result = result;
			Diff = diff;
		}

		public override string ToString()
		{
			return string.Format("{0,-7} : {1}", Result, Test);
		}
	}
}
