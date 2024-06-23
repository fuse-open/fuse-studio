using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Outracks.Fuse.Tests.Solution
{
	[TestFixture]
	public class SolutionVersioningTests
	{
		static string SolutionDirectory
		{
			get { return Path.GetDirectoryName(SolutionTestsHelper.FindSolutionPathOf<SolutionVersioningTests>()); }
		}

		[Test]
		public void All_packages_in_solution_have_same_version_and_references_are_correct()
		{
			var excludedPackages = new[] { "NUnit", "NSubstitute", "Castle.Core" };
			SolutionTestsHelper.VerifyNugetPackageReferences(
				SolutionTestsHelper.FindSolutionPathOf(this),
				x => !excludedPackages.Contains(x.Id));
		}
	}
}
