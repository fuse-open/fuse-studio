using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RegressionTests
{
	internal static class InteractiveDiffAccepter
	{
		/// <summary>
		/// Return `true` if all diffs were accepted
		/// </summary>
		public static bool Run(IReadOnlyCollection<TestResult> diffs)
		{
			Log.Info("");
			Log.Info("Running in interactive mode. Here are the changes:");
			var accepted = 0;
			using (var it = diffs.GetEnumerator())
			{
				var running = it.MoveNext();
				while (running)
				{
					var testResult = it.Current;
					Log.Info("Test : " + testResult.Test.Name);
					ColoredDiffPrinter.Print(testResult.Diff);
					var answer = Log.Prompt("Accept this change? [y,n,q,a,?]?");
					switch (answer)
					{
						case "y":
							accepted++;
							Accept(testResult);
							running = it.MoveNext();
							break;
						case "n":
							running = it.MoveNext();
							break;
						case "q":
							running = false;
							break;
						case "a":
							do
							{
								accepted++;
								Accept(it.Current);
							} while (it.MoveNext());
							running = false;
							break;
						default:
							Log.Info("y - accept this change");
							Log.Info("n - do not accept this change");
							Log.Info("q - quit; do not accept this change or any of the remaining ones");
							Log.Info("a - accept this change and all later changes in the entire test run");
							Log.Info("? - print help");
							break;
					}
				}
			}
			return accepted == diffs.Count();
		}

		private static void Accept(TestResult testResult)
		{
			foreach (var diff in testResult.Diff)
			{
				if (!Directory.Exists(diff.OldDir))
				{
					Directory.CreateDirectory(diff.OldDir);
				}
				File.Copy(diff.NewFile, diff.OldFile, true);
			}
		}
	}
}
