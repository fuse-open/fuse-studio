using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Outracks.IO;

namespace Outracks.Fusion
{
	public static class FusionJsonSerializer
	{
		public static readonly JsonSerializerSettings Settings;

		static FusionJsonSerializer()
		{
			Settings = new JsonSerializerSettings();
			Settings.Converters.Add(new PointsConverter());
			Settings.Converters.Add(new SizeConverter<Points>());
			Settings.Converters.Add(new PointConverter<Points>());
			Settings.Converters.Add(new PathJsonConverter());
			Settings.Converters.Add(new StringEnumConverter());
		}

		public static JsonSerializer CreateDefault()
		{
			return JsonSerializer.Create(Settings);
		}
	}

	public class PointsConverter : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var points = (Points)value;
			writer.WriteValue(points.ToDouble());
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var token = JToken.Load(reader);
			var value = token.Value<double>();

			return new Points(value);
		}

		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(Points);
		}
	}

	public class SizeConverter<T> : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var size = (Size<T>)value;

			writer.WriteStartArray();
			JToken.FromObject(size.Width, serializer).WriteTo(writer);
			JToken.FromObject(size.Height, serializer).WriteTo(writer);
			writer.WriteEndArray();
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var children = JToken.Load(reader);
			var width = children[0].ToObject<T>(serializer);
			var height = children[1].ToObject<T>(serializer);
			return new Size<T>(width, height);
		}

		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(Size<T>);
		}
	}

	public class PointConverter<T> : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var point = (Point<T>)value;

			writer.WriteStartArray();
			JToken.FromObject(point.X, serializer).WriteTo(writer);
			JToken.FromObject(point.Y, serializer).WriteTo(writer);
			writer.WriteEndArray();
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var children = JToken.Load(reader);
			var x = children[0].ToObject<T>(serializer);
			var y = children[1].ToObject<T>(serializer);
			return new Point<T>(x, y);
		}

		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(Point<T>);
		}
	}
}