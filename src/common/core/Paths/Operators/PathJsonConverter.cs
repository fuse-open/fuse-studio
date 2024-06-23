using System;
using Newtonsoft.Json;

namespace Outracks.IO
{
	public class PathJsonConverter : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			if (value is AbsoluteFilePath) writer.WriteValue(((AbsoluteFilePath) value).NativePath);
			else if (value is RelativeFilePath)  writer.WriteValue(((RelativeFilePath)value).NativeRelativePath);
			else if (value is AbsoluteDirectoryPath)  writer.WriteValue(((AbsoluteDirectoryPath)value).NativePath);
			else if (value is RelativeDirectoryPath) writer.WriteValue(((RelativeDirectoryPath)value).NativeRelativePath);
			else throw new ArgumentException("The value '" + value + "' was not recognized by this converter");
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			if (objectType == typeof(AbsoluteFilePath)) return AbsoluteFilePath.Parse((string)reader.Value);
			if (objectType == typeof(RelativeFilePath)) return RelativeFilePath.Parse((string)reader.Value);
			if (objectType == typeof (AbsoluteDirectoryPath)) return AbsoluteDirectoryPath.Parse((string)reader.Value);
			if (objectType == typeof(RelativeDirectoryPath)) return RelativeDirectoryPath.Parse((string)reader.Value);
			throw new ArgumentException("The type '" + objectType.FullName + "' was not recognized by this converter");
		}

		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof (AbsoluteFilePath)
				|| objectType == typeof (RelativeFilePath)
				|| objectType == typeof (AbsoluteDirectoryPath)
				|| objectType == typeof (RelativeDirectoryPath);
		}
	}
}