using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using NUnit.Framework;

namespace Outracks.Fuse.Tests.Solution
{
    /// <summary>
    /// Abstract base class for Visual Studio solution sanity checks.
    /// </summary>
    public static class SolutionTestsHelper
    {
        public static IEnumerable<string> FindCSharpSourceFiles(string path)
        {
            return FindSourceFiles("*.cs", path);
        }

		public static string GetPhysicalLocation(this Assembly assembly)
		{
			if (assembly == null)
				throw new ArgumentNullException(nameof(assembly));

			var assemblyPath = assembly.Location;

			if (String.IsNullOrEmpty(assemblyPath))
			{
				var uri = new UriBuilder(assembly.CodeBase);
				var unescapeDataString = Uri.UnescapeDataString(uri.Path);
				assemblyPath = Path.GetFullPath(unescapeDataString);
			}

			return assemblyPath;
		}


		public static IEnumerable<T> Pad<T>(this IEnumerable<T> source, int count, T paddingValue)
		{
			foreach (var item in source)
			{
				yield return item;
				count--;
			}

			while (count > 0)
			{
				yield return paddingValue;
				count--;
			}
		}

		/// <summary>
		/// Finds the physical path of the Visual Studio Project File the assembly is compiled from.
		/// </summary>
		/// <param name="assembly">The assembly.</param>
		/// <returns>The physical path of the  Visual Studio Project File the assembly is compiled from.</returns>
		public static string FindProjectPathOf(Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            var assemblyPath = assembly.GetPhysicalLocation();

            if (String.IsNullOrEmpty(assemblyPath))
            {
                throw new FileNotFoundException($"Could not find a physical path for '{assembly}'.");
            }

            FileInfo assemblyFile = new FileInfo(assemblyPath);
            DirectoryInfo parentDirectory = assemblyFile.Directory;
            string fileNotFoundMessage = $"Couldn't find a Project file for '{assembly}'.";
            string projectFileName = Path.ChangeExtension(assemblyFile.Name, "csproj");

            try
            {
                while ((parentDirectory = parentDirectory.Parent) != null)
                {
                    FileInfo[] solutionFiles = parentDirectory.GetFiles(projectFileName, SearchOption.AllDirectories);

                    if (solutionFiles.Length > 0)
                        return solutionFiles[0].FullName;
                }
            }
            catch (Exception exception)
            {
                throw new FileNotFoundException(fileNotFoundMessage, exception);
            }

            throw new FileNotFoundException(fileNotFoundMessage);
        }


        /// <summary>
        /// Finds the physical path to the Visual Studio Project File <typeparamref name="T"/> is defined in.
        /// </summary>
        /// <typeparam name="T">The type whose  Visual Studio Project File we should find the physical path to.</typeparam>
        /// <returns>The physical path to the  Visual Studio Project File <typeparamref name="T"/> is defined in.</returns>
        public static string FindProjectPathOf<T>()
        {
            Assembly assembly = typeof(T).Assembly;
            return FindProjectPathOf(assembly);
        }


        /// <summary>
        /// Finds the physical path to the Visual Studio Solution File of the <see cref="Assembly"/> <typeparamref name="T"/> is defined in.
        /// </summary>
        /// <typeparam name="T">The type whose <see cref="Assembly"/> we should find the physical path to the Visual Studio Solution File of.</typeparam>
        /// <returns>
        /// The physical path to the Visual Studio Solution File of the <see cref="Assembly"/> <typeparamref name="T"/> is defined in.
        /// </returns>
        public static string FindSolutionPathOf<T>()
        {
            Assembly assembly = typeof(T).Assembly;
            return FindSolutionPathOf(assembly);
        }


        /// <summary>
        /// Finds the physical path to the Visual Studio Solution File of the <see cref="Assembly"/> <paramref name="object"/> is defined in.
        /// </summary>
        /// <param name="object">The type whose <see cref="Assembly"/> we should find the physical path to the Visual Studio Solution File of.</param>
        /// <returns>
        /// The physical path to the Visual Studio Solution File of the <see cref="Assembly"/> <paramref name="object"/> is defined in.
        /// </returns>
        public static string FindSolutionPathOf(object @object)
        {
            if (@object == null)
                throw new ArgumentNullException(nameof(@object));

            return FindSolutionPathOf(@object.GetType());
        }


        /// <summary>
        /// Finds the physical path to the Visual Studio Solution File of the <see cref="Assembly"/> <paramref name="type"/> is defined in.
        /// </summary>
        /// <param name="type">The type whose <see cref="Assembly"/> we should find the physical path to the Visual Studio Solution File of.</param>
        /// <returns>
        /// The physical path to the Visual Studio Solution File of the <see cref="Assembly"/> <paramref name="type"/> is defined in.
        /// </returns>
        public static string FindSolutionPathOf(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return FindSolutionPathOf(type.Assembly);
        }


        /// <summary>
        /// Finds the physical path to the Visual Studio Solution File of the <paramref name="assembly"/>.
        /// </summary>
        /// <param name="assembly">The assembly of which to find the physical Visual Studio Solution File path to.</param>
        /// <returns>
        /// The physical path to the Visual Studio Solution File of the <paramref name="assembly"/>.
        /// </returns>
        public static string FindSolutionPathOf(Assembly assembly)
        {
			var assemblyPath = assembly.GetPhysicalLocation();

			if (String.IsNullOrEmpty(assemblyPath))
			{
				throw new FileNotFoundException($"Could not find a physical path for '{assembly}'.");
			}

			FileInfo assemblyFile = new FileInfo(assemblyPath);
			DirectoryInfo parentDirectory = assemblyFile.Directory;

            string fileNotFoundMessage = $"Couldn't find a Solution file for '{assembly}'.";

            try
            {
                while ((parentDirectory = parentDirectory.Parent) != null)
                {
                    FileInfo[] solutionFiles = parentDirectory.GetFiles("*.sln");

                    if (solutionFiles.Length > 0)
                        return solutionFiles[0].FullName;
                }
            }
            catch (Exception exception)
            {
                throw new FileNotFoundException(fileNotFoundMessage, exception);
            }

            throw new FileNotFoundException(fileNotFoundMessage);
        }


        public static IEnumerable<string> FindSourceFiles(string searchPattern, string path)
        {
            var sourceCodeFiles = Directory.EnumerateFiles(path,
                                                           searchPattern,
                                                           SearchOption.AllDirectories)
                                           .Where(x => !IsIgnoredPath(Path.GetDirectoryName(x)));
            return sourceCodeFiles;
        }



        /// <summary>
        /// Check all projects in solution at path for consistent nuget package references.
        /// </summary>
        /// <param name="solutionPath">Path to .sln file.</param>
        public static void VerifyNugetPackageReferences(string solutionPath, Func<NugetPackageElement, bool> filter)
        {
            filter = filter ?? (x => true);
            var solutionDir = Path.GetDirectoryName(solutionPath);
            //var solution = new ICSharpCode.NRefactory.ConsistencyCheck.Solution(solutionPath);
            var packages = NugetPackageElement.Load(solutionDir).Where(filter).GroupBy(x => x.Id).ToList();
            StringBuilder sb = new StringBuilder();
            int errorCount = 0;
            foreach (var package in packages)
            {
                var versions = package.GroupBy(x => x.Version).ToList();
                if (versions.Count > 1)
                {
                    sb.AppendFormat("Found multiple versions of package {0}:\r\n{1}",
                                    package.Key,
                                    String.Join("",
                                                versions.Select(
                                                    x =>
                                                        $"    {x.Key}\r\n{String.Join("", x.Select(y => $"        {y.ProjectName}\r\n"))}")));

                    errorCount++;

                    var suggestedVersion =
                        versions.Select(x => x.Key).OrderBy(x => x).Last();
                    var suggestedUpgrades =
                        versions.Where(x => x.Key != suggestedVersion).SelectMany(x => x);
                    sb.AppendFormat("    Suggested version is {0}, install using:\r\n{1}",
                                    suggestedVersion,
                                    String.Join("",
                                                suggestedUpgrades.Select(
                                                    x =>
                                                        $"        Update-Package -Id {x.Id} -ProjectName {x.ProjectName} -Version {suggestedVersion}\r\n")));
                }
            }
            foreach (var item in packages.SelectMany(x => x))
                errorCount += item.ValidateHintPathReference(sb);
            Assert.That(errorCount, Is.EqualTo(0), "Found package reference inconsitencies:\r\n" + sb);
        }


        public static void VerifyProjectNoOrphanSourceCodeFiles(string projectPath)
        {
            XDocument project;
            using (var f = File.OpenRead(projectPath))
            {
                project = XDocument.Load(f);
            }
            var projectDir = Path.GetDirectoryName(projectPath);
            var xmlNamespaceManager = new XmlNamespaceManager(new NameTable());
            xmlNamespaceManager.AddNamespace("pj", project.Root.Name.NamespaceName);
            var filesInProject =
                ((IEnumerable)project.XPathEvaluate("/pj:Project/pj:ItemGroup/pj:Compile/@Include", xmlNamespaceManager))
                    .Cast<XAttribute>()
                    .Select(x => x.Value)
                    .Where(x => Path.GetExtension(x) == ".cs")
                    .Select(x => MakeRelative(Path.GetFullPath(Path.Combine(projectDir, x)), projectDir)).ToList();
            var filesInDirectory =
                FindCSharpSourceFiles(projectDir)
                    .Select(x => MakeRelative(x, projectDir)).ToList();

            var errorLog = new StringBuilder();
            var filesNotIncludedInProject = filesInDirectory.Except(filesInProject, StringComparer.OrdinalIgnoreCase);
            foreach (var orphanFile in filesNotIncludedInProject)
                errorLog.AppendFormat("File \"{0}\" in project folder of {1} is not included.", orphanFile, projectPath);

            if (errorLog.Length > 0)
                Assert.Fail(errorLog.ToString());
        }


        private static IEnumerable<string> GetSourceCodeFiles(string solutionDirectory,
                                                              Func<string, bool> excludeFilter = null,
                                                              string[] fileSearchPatterns = null)
        {
            fileSearchPatterns = fileSearchPatterns ?? new[] { "*.cs", "*.csproj", "*.config", "*.xml" };
            excludeFilter = excludeFilter ?? IsIgnoredPath;
            return
                fileSearchPatterns.SelectMany(x => Directory.EnumerateFiles(solutionDirectory, x, SearchOption.AllDirectories)).Where(
                    x => !excludeFilter(x));
        }


        private static IEnumerable<string> GetSourceCodeFilesThatShouldHaveUtf8Encoding(string solutionDirectory,
                                                                                        Func<string, bool> excludeFilter = null,
                                                                                        string[] fileSearchPatterns = null)
        {
            // Keeping this method in case we don't want certain files to be UTF-8
            return GetSourceCodeFiles(solutionDirectory, excludeFilter, fileSearchPatterns);
        }


        private static bool IsIgnoredPath(string directoryName)
        {
            var slash = Path.DirectorySeparatorChar;
            return directoryName.Contains(slash + "obj" + slash)
                || directoryName.Contains(slash + "bin" + slash);
        }


        private static string MakeRelative(string filePath, string referencePath)
        {
            var fileUri = new Uri(filePath);
            var referenceUri = new Uri(referencePath);
            return referenceUri.MakeRelativeUri(fileUri).ToString().Replace("/", Path.DirectorySeparatorChar.ToString());
        }

        #region Nested type: NugetPackageElement

		#endregion
    }
}
