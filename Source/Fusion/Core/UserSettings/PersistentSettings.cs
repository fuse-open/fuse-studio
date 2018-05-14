using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Outracks.IO;

namespace Outracks.Fusion
{
	public interface ISettings
	{
		IProperty<Optional<T>> Property<T>(string name);
	}

	public class EmptySettings : ISettings
	{
		public IProperty<Optional<T>> Property<T>(string name)
		{
			return Outracks.Property.Create(Optional.None<T>());
		}
	}

	public class PersistentSettings : ISettings
	{
		public static ISettings Load(
			AbsoluteFilePath usersettingsfile,
			IFileSystem filesystem = null,
			ISettings fallback = null,
			Action<Exception> onError = null)
		{
			return new PersistentSettings
			{
				_onError = onError ?? Console.Error.WriteLine,
				_properties = LoadUserSettings(usersettingsfile, onError ?? Console.Error.WriteLine),
				_settingsFile = usersettingsfile,
				_fileSystem = filesystem ?? new Shell(),
				_fallbackSettings = fallback ?? new EmptySettings(),
			};
		}

		static JObject LoadUserSettings(AbsoluteFilePath usersettingsfile, Action<Exception> onError)
		{
			if (File.Exists(usersettingsfile.NativePath))
			{
				try
				{ 
					// TODO Use FileSystem methods, make method non-static
					using (var loadfile = File.OpenRead(usersettingsfile.NativePath))
					using (var filereader = new StreamReader(loadfile))
					{
						var json = filereader.ReadToEnd();
						if (!json.IsEmpty())
							return JObject.Parse(json); // FuseSettingsSerializerSettings());
						// TODO try-catch in case deserialization doesn't work
					}
				}
				catch (Exception e)
				{
					onError(e);
				}
			}
			return new JObject();
		}

		readonly JsonSerializer _jsonSerializer = FusionJsonSerializer.CreateDefault();

		JObject _properties;
		AbsoluteFilePath _settingsFile;
		IFileSystem _fileSystem;
		ISettings _fallbackSettings;
		Action<Exception> _onError;
		readonly object _lock = new object();

		public IProperty<Optional<T>> Property<T>(string name)
		{
			var value = _properties.TryGetValue(name).SelectMany(TryParse<T>);
			var ownProperty = new PersistentProperty<T>(this, name, value);
			var fallbackProperty = _fallbackSettings.Property<T>(name);
			return ownProperty.With(value: ownProperty.Or(fallbackProperty));
		}

		public void Write<T>(string name, Optional<T> value)
		{
			lock (_lock)
			{
				if (!value.HasValue)
				{
					_properties.Remove(name);
				}
				else
				{
					_properties[name] = JToken.FromObject(value.Value, _jsonSerializer);
				}
			}
		}
		public void SaveSettings()
		{
			try
			{
				lock (_lock)
				{
					_fileSystem.ReplaceText(_settingsFile, _properties.ToString());
				}
			}
			catch (Exception e)
			{
				_onError(e);
			}
		}

		Optional<T> TryParse<T>(JToken token)
		{
			try
			{
				return token.ToObject<T>(_jsonSerializer);
			}
			catch (Exception e)
			{
				_onError(e);
				return Optional.None();
			}
		}
	}
}



