using System.Globalization;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SketchConverter.API;
using SketchConverter.SketchParser; // for ISketchArchive

namespace SketchConverter
{
	public static class CompatibilityChecker
	{
		// A sketch file archive (unzipped sketch file) contains a file called meta.json
		// which contains version information. An archive can have for example have version 97, but
		// compatibility version 93. This is the oldest version it is backwards compatible with.
		public static readonly int SketchCompatibilityVersion = 93;

		public static void Check(string sketchFilePath,
								 ISketchArchive sketchArchive,
								 ILogger log)
		{
			var meta = LoadMetaDataAsync(sketchArchive).Result;
			log.Info("You are running Sketch2Fuse compatible with Sketch format " +
					 "version " + SketchCompatibilityVersion);
			log.Info("Converting " + sketchFilePath + " created with Sketch "
					 + meta.AppVersion + " variant " + meta.Variant + " build" +
					 meta.Version);

			if (meta.CompatibilityVersion > SketchCompatibilityVersion)
			{
				log.Warning(
					"The sketch file you are converting to UX has been created with a newer version of Sketch than the converter supports.");
			}
			else if (meta.CompatibilityVersion < SketchCompatibilityVersion)
			{
				log.Warning(
					"The sketch file was created with an older version than Sketch2Fuse supports, please open the file in Sketch and save it as with the newer version if you experence any problems.");
			}
		}

		public struct SketchMeta
		{
			public double AppVersion;
			public int CompatibilityVersion;
			public int Version;
			public int Build;
			public string Variant;
		}

		private static async Task<SketchMeta> LoadMetaDataAsync(ISketchArchive sketchArchive)
		{
			using (var infoStream = sketchArchive.OpenFile("meta.json"))
			{
				var json = JObject.Parse(await infoStream.ReadToStringAsync().ConfigureAwait(false));
				var appversion = double.Parse((string) json["appVersion"], CultureInfo.InvariantCulture);
				// The first version of the open Sketch format doesn't have compatibilityVersion field.
				// ref TestProject.sketch. 2017-12-06 anette
				var compatibilityVersion = json["compatibilityVersion"] != null ? int.Parse((string) json["compatibilityVersion"]) : -1;
				var version = int.Parse((string) json["version"]);
				var build = (int) json["build"];
				var variant = (string) json["variant"];

				return new SketchMeta()
				{
					AppVersion = appversion,
					CompatibilityVersion =
						compatibilityVersion,
					Version = version,
					Build = build,
					Variant = variant
				};
			}
		}
	}
}
