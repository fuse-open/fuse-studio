using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Outracks.Fuse.Tests.Solution
{
	public class NugetPackageElement
	{
		private static readonly string[] knownFrameworkVersions = { "net451", "net45", "net40", "net35", "net20" };
		private static readonly string[] preferredFrameworkVersions = { "net45", "net40", "net35", "net20" };
		private readonly string projectFile;
		private readonly string solutionDirectoy;
		private string[] availableFrameworkVersions;
		private string fullPackagePath;
		private XDocument projectXmlDocument;


		public NugetPackageElement(
			string project,
			XElement packagesConfigElement,
			string solutionDirectoy,
			XDocument projectXmlDocument = null)
		{
			projectFile = project;
			this.solutionDirectoy = solutionDirectoy;
			this.projectXmlDocument = projectXmlDocument;
			Id = packagesConfigElement.Attribute("id").Value;
			Version = packagesConfigElement.Attribute("version").Value;
		}


		public string AssumedPackagePath => AssumedPackagePathStart + Version;

		public string AssumedPackagePathStart => Path.Combine(RelativePackagesPath, Id + ".");

		public string Id { get; }

		public string PreferredFrameworkVersion
		{
			get { return preferredFrameworkVersions.FirstOrDefault(x => AvailableFrameworkVersions.Contains(x)); }
		}

		public string ProjectDirectory => Path.GetDirectoryName(projectFile);

		public string ProjectName => Path.GetFileNameWithoutExtension(projectFile);

		public XDocument ProjectXmlDocument => projectXmlDocument ?? (projectXmlDocument = XDocument.Load(projectFile));

		public string RelativePackagesPath => GetRelativePath(
			Path.Combine(solutionDirectoy, "packages"),
			Path.GetDirectoryName(projectFile));

		public string Version { get; }

		private string[] AvailableFrameworkVersions
		{
			get
			{
				return availableFrameworkVersions
					?? (availableFrameworkVersions =
						Directory.GetDirectories(Path.Combine(FullPackagePath, "lib"))
							.Select(x => Path.GetFileName(x).ToLower())
							.Where(x => knownFrameworkVersions.Contains(x))
							.ToArray());
			}
		}

		private string FullPackagePath => fullPackagePath
			?? (fullPackagePath = Path.Combine(solutionDirectoy, "packages", Id + "." + Version));


		public static IEnumerable<NugetPackageElement> Load(string solutionDirectory)
		{
			return Load(
				Directory.EnumerateFiles(solutionDirectory, "*.csproj", SearchOption.AllDirectories),
				solutionDirectory);
		}


		public static IEnumerable<NugetPackageElement> Load(IEnumerable<string> projects, string solutionDirectory)
		{
			foreach (var projectFile in projects)
			{
				var projectLocal = projectFile;
				var packagesXml = Path.Combine(Path.GetDirectoryName(projectFile), "packages.config");
				if (!File.Exists(packagesXml))
				{
					Console.WriteLine("Project {0} has no packages.config; skipping.", projectFile);
					continue;
				}

				var projectXDoc = XDocument.Load(projectFile);
				foreach (
					var item in
					XDocument.Load(packagesXml).Descendants("package").Select(
						x => new NugetPackageElement(projectLocal, x, solutionDirectory, projectXDoc)))
					yield return item;
			}
		}


		public int ValidateHintPathReference(StringBuilder errorLog)
		{
			int errorCount = 0;
			if (!Directory.Exists(FullPackagePath))
				Console.WriteLine("Warning: Could not find package directory " + FullPackagePath);

			XNamespace ns = "http://schemas.microsoft.com/developer/msbuild/2003";
			var nsManager = new XmlNamespaceManager(new NameTable());
			nsManager.AddNamespace("x", ns.NamespaceName);
			var assumedPackagePathStart = AssumedPackagePathStart;
			var xpathPredicate =
				String.Format(
					"//x:Project/x:ItemGroup/x:Reference[starts-with(x:HintPath,'{0}') and string-length(x:HintPath) > ({1} + 1) and string(number(substring(x:HintPath,{1},1))) != 'NaN']",
					assumedPackagePathStart,
					assumedPackagePathStart.Length + 1);
			var referencesGroupedByVersion =
				ProjectXmlDocument.XPathSelectElements(xpathPredicate, nsManager).Select(
					x => new LibReference(assumedPackagePathStart, x, ns)).ToList().GroupBy(
					x => x.PathVersionPart).ToList();
			if (referencesGroupedByVersion.Count == 0)
			{
				if (Directory.Exists(fullPackagePath)
					&& Directory.EnumerateFiles(fullPackagePath, "*.dll", SearchOption.AllDirectories).Any())
				{
					errorLog.AppendFormat(
						"Warning: Could not find any reference to {0} in {1}.\r\n    Suggestion: Update-Package -reinstall {0} -ProjectName {1}\r\n",
						Id,
						ProjectName);
				}
			}

			if (referencesGroupedByVersion.Count > 1)
			{
				errorLog.AppendFormat(
					"There are dll-references from {0} to multiple versions of {1} ({2})\r\n",
					ProjectName,
					Id,
					String.Join(", ", referencesGroupedByVersion.Select(x => x.Key)));
				errorCount++;
			}

			foreach (var refVerGroup in referencesGroupedByVersion)
			{
				if (!DirVersionMatches(Version, refVerGroup.Key))
				{
					errorLog.AppendFormat(
						"Reference made from {0} to {1} got hintpath {2}, expected {3}\\???.\r\n    Suggestion: Update-Package –reinstall {1} -ProjectName {0}\r\n",
						ProjectName,
						Id,
						refVerGroup.First().HintPath,
						AssumedPackagePath);
					errorCount++;
				}

				foreach (var item in refVerGroup.Where(x => x.TargetFrameworkVersion != PreferredFrameworkVersion))
				{
					errorLog.AppendFormat(
						"Reference from {0} to {1} got hintpath {2} targeting wrong framework {3}, should be {4}.\r\n    Suggestion: Update-Package –reinstall {1} -ProjectName {0}\r\n",
						ProjectName,
						Id,
						item.HintPath,
						item.TargetFrameworkVersion,
						PreferredFrameworkVersion);
					errorCount++;
				}
			}

			return errorCount;
		}


		private static bool DirVersionMatches(string version, string dirVersion)
		{
			if (version == dirVersion)
				return true;

			// Directory 1.8 matches version 1.8.0, so if last part of version is .0 we can remove it.
			if (ParseVersionParts(version).SequenceEqual(ParseVersionParts(dirVersion)))
				return true;
			return false;
		}


		private string GetRelativePath(string filespec, string folder)
		{
			Uri pathUri = new Uri(filespec);
			// Folders must end in a slash
			if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
				folder += Path.DirectorySeparatorChar;
			Uri folderUri = new Uri(folder);
			return
				Uri.UnescapeDataString(
					folderUri.MakeRelativeUri(pathUri).ToString().Replace(
						'/',
						Path.DirectorySeparatorChar));
		}


		private static int[] ParseVersionParts(string version)
		{
			string _;
			return ParseVersionParts(version, out _);
		}


		private static int[] ParseVersionParts(string version, out string preReleasePart)
		{
			var mainAndPreleaseParts = version.Split('-');
			if (mainAndPreleaseParts.Length > 1)
				preReleasePart = mainAndPreleaseParts[1];
			else
				preReleasePart = null;
			return mainAndPreleaseParts[0].Split('.').Select(Int32.Parse).Pad(3, 0).ToArray();
		}

		public override string ToString()
		{
			return Id + "." + Version;
		}

		#region Nested type: LibReference

		private class LibReference
		{
			public LibReference(string hintPathBeforePrefix, XElement element, XNamespace ns)
			{
				var hintPathElement = element.Descendants(ns + "HintPath").First();
				HintPath = hintPathElement.Value;
				PathVersionPart = HintPath.Substring(
					hintPathBeforePrefix.Length,
					HintPath.IndexOfAny(
						new[] { '\\', '/' },
						hintPathBeforePrefix.Length)
					- hintPathBeforePrefix.Length);
				var packageRelativePath = HintPath.Substring(hintPathBeforePrefix.Length + PathVersionPart.Length + 1);

				var pathSegments = packageRelativePath.Split(Path.DirectorySeparatorChar);
				if (pathSegments[0] == "lib")
				{
					TargetFrameworkVersion =
						pathSegments
							.Skip(1)
							.Take(1)
							.Where(x => knownFrameworkVersions.Contains(x, StringComparer.OrdinalIgnoreCase))
							.Select(x => x.ToLower())
							.FirstOrDefault();
				}
			}


			public string HintPath { get; }

			public string PathVersionPart { get; }

			public string TargetFrameworkVersion { get; }
		}

		#endregion

		#region Nested type: VersionComparer

		internal class VersionComparer : StringComparer
		{
			public override int Compare(string x, string y)
			{
				string xPreRelease;
				var xParts = ParseVersionParts(x, out xPreRelease).Pad(4, 0).ToArray();
				string yPreRelease;
				var yParts = ParseVersionParts(y, out yPreRelease).Pad(4, 0).ToArray();
				if (xParts.SequenceEqual(yParts))
				{
					if (xPreRelease == null && yPreRelease == null)
						return 0;
					if (xPreRelease == null)
						return 1;
					if (yPreRelease == null)
						return -1;
					return InvariantCultureIgnoreCase.Compare(xPreRelease, yPreRelease);
				}

				return xParts.Zip(yParts, (xp, yp) => xp - yp).First(q => q != 0);
			}


			public override bool Equals(string x, string y)
			{
				return Compare(x, y) == 0;
			}


			public override int GetHashCode(string obj)
			{
				return obj.GetHashCode();
			}
		}

		#endregion
	}
}
