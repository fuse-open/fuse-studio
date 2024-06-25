using System;

namespace RegressionTests
{
	static class TeamCityReporter
	{
		public static void TestSuiteStarted()
		{
			Console.WriteLine("##teamcity[testSuiteStarted name='RegressionTests']");
		}

		public static void TestSuiteFinished()
		{
			Console.WriteLine("##teamcity[testSuiteFinished name='RegressionTests']");
		}

		public static void TestStarted(Test test)
		{
			Console.WriteLine("##teamcity[testStarted name='" + test.Name + "' captureStandardOutput='true']");
		}

		public static void TestResult(TestResult test)
		{
			switch (test.Result)
			{
				case Result.Ignored:
					TestIgnored(test.Test);
					break;
				case Result.Failed:
					TestFailed(test.Test);
					break;
				 default:
					 break;
			}
			TestFinished(test.Test);
		}

		private static void TestFailed(Test test)
		{
			Console.WriteLine("##teamcity[testFailed name='" + test.Name + "']");
		}

		private static void TestFinished(Test test)
		{
			Console.WriteLine("##teamcity[testFinished name='" + test.Name + "']");
		}

		private static void TestIgnored(Test test)
		{
			Console.WriteLine("##teamcity[testIgnored name='" + test.Name + "']");
		}
	}
}
