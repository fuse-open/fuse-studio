using System.Text.RegularExpressions;

namespace RegressionTests
{
	public static class OutputCleaner
	{
		//Just removes paths typically found in our output. Will probably need updating now and again
		//Not the most robus code, but it's only for internal use anyway
		public static string RemovePaths(string str)
		{
			var preceding = @"(\s|^|'|\()";
			var fileName = @"([\S\.]+.(ux|sketch|png))";
			var windowsRegex = new Regex(preceding + @"\w:[\S\\]*\\" + fileName);
			var windowsReplaced = windowsRegex.Replace(str, "$1<path removed>$2");

			var macRegex = new Regex(preceding + @"/[\S/]+/" + fileName);
			return macRegex.Replace(windowsReplaced, "$1<path removed>$2");
		}
	}
}
