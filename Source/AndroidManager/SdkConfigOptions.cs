using Newtonsoft.Json;
using Outracks.IO;

namespace Outracks.AndroidManager
{
	class SdkConfigOptions
	{
		[JsonProperty("Android.SDK.Directory")]
		public string AndroidSdkDirectory = null;

		[JsonProperty("Android.NDK.Directory")]
		public string AndroidNdkDirectory = null;

		[JsonProperty("Java.JDK.Directory")]
		public string JavaJdkDirectory = null;

		[JsonProperty("Android.SDK.BuildToolsVersion")]
		public string AndroidSdkBuildToolsVersion = null;

		[JsonProperty("HaveAllSDKPackages")]
		public bool HaveAllSdkPackages = false;
	}

	public class OptionalSdkConfigOptions
	{
		public Optional<AbsoluteDirectoryPath> AndroidSdkDirectory;
		public Optional<AbsoluteDirectoryPath> AndroidNdkDirectory;
		public Optional<AbsoluteDirectoryPath> JavaJdkDirectory;
		public Optional<string> AndroidSdkBuildToolsVersion;
		public bool HaveAllSdkPackages;

		public OptionalSdkConfigOptions(
			Optional<AbsoluteDirectoryPath> androidSdkDirectory, 
			Optional<AbsoluteDirectoryPath> androidNdkDirectory, 
			Optional<AbsoluteDirectoryPath> javaJdkDirectory, 
			Optional<string> androidSdkBuildToolsVersion, 
			bool haveAllSdkPackages)
		{
			AndroidSdkDirectory = androidSdkDirectory;
			AndroidNdkDirectory = androidNdkDirectory;
			JavaJdkDirectory = javaJdkDirectory;
			AndroidSdkBuildToolsVersion = androidSdkBuildToolsVersion;
			HaveAllSdkPackages = haveAllSdkPackages;
		}
	}

	static class OptionalExtensions
	{
		public static OptionalSdkConfigOptions Optionalize(this Optional<SdkConfigOptions> sdkConfigOptions)
		{
			if (sdkConfigOptions.HasValue)
			{
				var someConfig = sdkConfigOptions.Value;
				return new OptionalSdkConfigOptions(
					androidSdkDirectory: AbsoluteDirectoryPath.TryParse(someConfig.AndroidSdkDirectory),
					androidNdkDirectory: AbsoluteDirectoryPath.TryParse(someConfig.AndroidNdkDirectory),
					javaJdkDirectory: AbsoluteDirectoryPath.TryParse(someConfig.JavaJdkDirectory),
					androidSdkBuildToolsVersion: someConfig.AndroidSdkBuildToolsVersion.ToOptional(),
					haveAllSdkPackages: someConfig.HaveAllSdkPackages
				);
			}
			else
			{
				return new OptionalSdkConfigOptions(
					Optional.None(),
					Optional.None(),
					Optional.None(),
					Optional.None(),
					false);
			}
		}

		public static SdkConfigOptions DeOptionalize(this OptionalSdkConfigOptions sdkConfigOptions)
		{
			return new SdkConfigOptions()
			{
				AndroidSdkDirectory = sdkConfigOptions.AndroidSdkDirectory.Select(dir => dir.NativePath).Or((string) null),
				AndroidNdkDirectory = sdkConfigOptions.AndroidNdkDirectory.Select(dir => dir.NativePath).Or((string) null),
				JavaJdkDirectory = sdkConfigOptions.JavaJdkDirectory.Select(dir => dir.NativePath).Or((string) null),
				AndroidSdkBuildToolsVersion = sdkConfigOptions.AndroidSdkBuildToolsVersion.Or((string) null),
				HaveAllSdkPackages = sdkConfigOptions.HaveAllSdkPackages
			};
		}
	}
}