using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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

		static string ExpectedUnoVersion
		{
			get
			{
				var unoversion = File.ReadAllText(Path.Combine(SolutionDirectory, ".unoversion")).Trim(" \t\r\n".ToCharArray());
				Assert.That(
					unoversion,
					Does.Match(@"^(?<numparts>\d+)(?:\.(?<numparts>\d+))*(?<suffix>-[\w\-]+)?$"),
					".unoversion should contain a valid semantic version string");
				return unoversion;
			}
		}

		[Test]
		public void All_packages_in_solution_have_same_version_and_references_are_correct()
		{
			var excludedPackages = new[] { "NUnit", "NSubstitute", "Castle.Core" };
			SolutionTestsHelper.VerifyNugetPackageReferences(
				SolutionTestsHelper.FindSolutionPathOf(this),
				x => !excludedPackages.Contains(x.Id));
		}

		[Test]
		public void All_uno_nuget_package_references_matches_version_specified_in_unoversion_file()
		{
			var unoversion = File.ReadAllText(Path.Combine(SolutionDirectory, ".unoversion")).Trim(" \t\r\n".ToCharArray());
			Assert.That(
				unoversion,
				Does.Match(@"^(?<numparts>\d+)(?:\.(?<numparts>\d+))*(?<suffix>-[\w\-]+)?$"),
				".unoversion should contain a valid semantic version string");
			var wrongVersionSets = NugetPackageElement.Load(SolutionDirectory).Where(x => x.Id.StartsWith("FuseOpen.Uno."))
				.Where(x => x.Version != unoversion)
				.GroupBy(x => x.Version)
				.ToList();

			if (wrongVersionSets.Count > 0)
			{
				Console.WriteLine("Found one or more references to uno packages with wrong version:");
				foreach (var projGroup in wrongVersionSets.SelectMany(x => x).GroupBy(x => x.ProjectName))
				{
					Console.WriteLine("{0}:", projGroup.Key);
					foreach (var invalidRef in projGroup)
					{
						Console.WriteLine("  - {0}", invalidRef);
					}
				}
			}

			Assert.That(wrongVersionSets, Is.Empty, "One or more references to the the wrong package version of uno found");
		}

		[Test]
		public void Versions_specified_in_uno_packages_file_matches_version_specified_in_unoversion_file()
		{
			var unoversion = ExpectedUnoVersion;

			Console.WriteLine(unoversion);
			var regex = new Regex(@"^\s*[\w\.\-]+:\s*(?<ver>[^\s]+)\s*$", RegexOptions.Compiled | RegexOptions.Multiline);
			Assert.Multiple(
				() => {
					foreach (var m in regex.Matches(File.ReadAllText(Path.Combine(SolutionDirectory, "Stuff", "uno.packages")))
						.Cast<Match>())
					{
						Assert.That(
							m.Groups["ver"].Value,
							Is.EqualTo(unoversion),
							() => $"Incorrect uno version in uno.packages: {m.Value}");
					}
				});
		}

		[Test]
		public void Packaged_unoconfig_included_from_repo_root_unoconfig_is_from_version_specified_in_unoversion_file()
		{
			var repoUnoConfig = File.ReadAllText(Path.Combine(SolutionDirectory, ".unoconfig"));
			var includedUnoconfigVersion =
				Regex.Matches(
					repoUnoConfig,
					@"^\s*include\s+packages/FuseOpen\.Uno\.Tool\.(?<ver>[^/]+)/tools/\.unoconfig\s*$",
					RegexOptions.Multiline).Cast<Match>().Select(x => x.Groups["ver"].Value).SingleOrDefault();
			var suggestedLine = $"include packages/FuseOpen.Uno.Tool.{ExpectedUnoVersion}/tools/.unoconfig";
			Assert.That(
				includedUnoconfigVersion,
				Is.Not.Null,
				"Missing include in repo root .unoconfig, maybe add this line: " + suggestedLine);
			Assert.That(
				includedUnoconfigVersion,
				Is.EqualTo(ExpectedUnoVersion),
				"Version specified in repo root .unoconfig didn't match version in .unoversion. Maybe change to this: " +
				suggestedLine);
		}
	}
}
