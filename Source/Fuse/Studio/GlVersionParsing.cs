using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text.RegularExpressions;

namespace Outracks.Fuse.Designer
{
	public static class GlVersionParsing
	{
		static readonly Version MinimumVersion = new Version(2, 1);

		public static IObservable<string> ToLogMessages(this IObservable<OpenGlVersion> version, IReport report)
		{
			return version.SelectMany(v => GetLogMessages(v, report));
		}

		public static IEnumerable<string> GetLogMessages(OpenGlVersion glVersion, IReport report)
		{
			yield return "OpenGL Version: " + glVersion.GlVersion + "\n";
			yield return "OpenGL Vendor: " + glVersion.GlVendor + "\n";
			yield return "OpenGL Renderer: " + glVersion.GlRenderer + "\n";
			var version = glVersion.ToVersion();
			if (version.HasValue)
			{
				if (!version.Value.IsSupported())
				{
					var message = "Error: The required OpenGL version is " + MinimumVersion
						+ ", your computer reports '" + glVersion.GlVersion + "'";
					report.Error(message, ReportTo.Log);
					yield return message;
				}
			}
			else
			{
				report.Warn("OpenGL parsing error: '" + glVersion.GlVersion + "'", ReportTo.Headquarters | ReportTo.Log);
				yield return "Warning: Failed to detect OpenGL version. The required version is "
				+ MinimumVersion + ", your computer reports '" + glVersion.GlVersion + "'";
			}
		}

		public static Optional<Version> ToVersion(this OpenGlVersion version)
		{
			try
			{
				const string bol = "^";
				const string optionalSpace = @"\s*";
				const string firstAndSecondComponent = @"([0-9])+\.([0-9])+";
				const string optionalThirdComponent = @"(\.([0-9])+)?";
				const string optionalStringIfStartingWithSpace = @"(\s.*)?";
				const string eol = "$";
				var pattern = bol
					+ optionalSpace
					+ firstAndSecondComponent
					+ optionalThirdComponent
					+ optionalStringIfStartingWithSpace
					+ eol;
				var regex = new Regex(pattern);
				var match = regex.Match(version.GlVersion);
				var major = int.Parse(match.Groups[1].Value);
				var minor = int.Parse(match.Groups[2].Value);
				return new Version(major, minor);
			}
			catch (Exception)
			{
				return Optional.None();
			}
		}

		public static bool IsSupported(this Version version)
		{
			return version >= MinimumVersion;
		}
	}
}
