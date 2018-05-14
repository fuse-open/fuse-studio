using Newtonsoft.Json;
using Outracks.IO;
using Uno.Configuration;

namespace Outracks.AndroidManager
{
	public class ConfigLoader
	{
		readonly IFileSystem _fs;
		readonly AbsoluteFilePath _configPath;

		public ConfigLoader(IFileSystem fs)
		{
			_fs = fs;
			_configPath = AbsoluteFilePath.Parse(UnoConfig.Current.GetFullPath("SdkConfig"));
		}

		public void Save(OptionalSdkConfigOptions options)
		{
			if(_fs.Exists(_configPath.ContainingDirectory) == false)
				_fs.Create(_configPath.ContainingDirectory);

			_fs.ReplaceText(_configPath, JsonConvert.SerializeObject(options.DeOptionalize(), 
				new JsonSerializerSettings
				{
					NullValueHandling = NullValueHandling.Ignore
				}));
		}

		public OptionalSdkConfigOptions GetCurrentConfig()
		{
			return GetCurrentConfigInternal()
				.Optionalize();
		}

		Optional<SdkConfigOptions> GetCurrentConfigInternal()
		{
			if (_fs.Exists(_configPath))
				return JsonConvert.DeserializeObject<SdkConfigOptions>(_fs.ReadAllText(_configPath, 3));
			else
				return Optional.None();
		}		
	}
}