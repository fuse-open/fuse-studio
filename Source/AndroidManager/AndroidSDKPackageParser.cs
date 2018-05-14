using System;
using System.Collections.Generic;
using System.Linq;
using Outracks.IO;

namespace Outracks.AndroidManager
{
	class Package
	{
		public readonly string Name;
		public readonly Version SortableRevision;
		public readonly string OriginalRevision;

		public Package(string name, Version sortableRevision, string originalRevision)
		{
			Name = name;
			SortableRevision = sortableRevision;
			OriginalRevision = originalRevision;
		}
	}

	class AndroidSDKPackageParser
	{
		public static IEnumerable<Package> SearchForPackagesIn(IFileSystem fs, AbsoluteDirectoryPath searchPath)
		{
			var files = fs.GetFiles(searchPath);
			var sourcePropertyFile = files.FirstOrNone(f => f.Name == new FileName("source.properties"));
			if (sourcePropertyFile.HasValue)
			{
				var package = TryParse(fs.ReadAllText(sourcePropertyFile.Value, 5));
				if (package.HasValue)
					yield return package.Value;
			}

			foreach (var package in fs.GetDirectories(searchPath)
				.SelectMany(dir => SearchForPackagesIn(fs, dir))) 
			{
				yield return package;
			}
		}

		static Optional<Package> TryParse(string sourceProperties)
		{
			var lines = sourceProperties.NormalizeLineEndings().Split('\n');
			var properties = ParseProperties(lines);

			var sortableRevision = new Optional<Version>();
			var originalRevision = "";
			var name = "";

			foreach (var property in properties)
			{
				if (property.Name == "Pkg.Revision")
				{
					originalRevision = property.Value;
					Version version;
					if (Version.TryParse(property.Value.Contains(".") ? property.Value : property.Value + ".0", out version))
						sortableRevision = version;
				}

				if (property.Name == "Pkg.NameDisplay")
				{
					name = property.Value;
				}
			}

			return sortableRevision.HasValue
				? Optional.Some(new Package(name, sortableRevision.Value, originalRevision))
				: Optional.None();
		}

		static IEnumerable<Property> ParseProperties(IEnumerable<string> lines)
		{
			var properties = new List<Property>();
			foreach (var line in lines)
			{
				var property = ParseLine(line);
				property.Do(p => properties.Add(p));
			}
			return properties;
		}

		static Optional<Property> ParseLine(string line)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(line))
					return Optional.None();

				var equalSign = line.IndexOf('=');
				return new Property(line.Substring(0, equalSign), line.Substring(equalSign + 1, line.Length - equalSign - 1));
			}
			catch (Exception)
			{
				return Optional.None();
			}
		}
	}

	class Property
	{
		public readonly string Name;
		public readonly string Value;

		public Property(string name, string value)
		{
			Name = name;
			Value = value;
		}
	}
}