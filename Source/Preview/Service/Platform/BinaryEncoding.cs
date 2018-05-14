using System;
using System.Collections.Generic;
using System.IO;
using Outracks;
using Uno.Collections;

namespace Fuse.Preview
{
	static class BinaryEncoding
	{
		public static void WriteArray<T>(this BinaryWriter streamWriter, IEnumerable<T> list, Action<BinaryWriter, T> write)
		{
			var array = list.ToArray();
			streamWriter.Write(array.Length);
			foreach (var i in array)
			{
				write(streamWriter, i);
			}
		}

		public static void WriteTaggedValue(this BinaryWriter writer, object value)
		{
			WriteValue(writer, value, tagType: writer.Write);
		}

		public static void WriteValue(this BinaryWriter writer, object value, Action<string> tagType = null)
		{
			tagType = tagType ?? (typeTag => { });

			var valueType = value.GetType();
			if (valueType.IsArray)
			{
				tagType(valueType.GetElementType().FullName + "[]");

				var array = ((Array) value);
				writer.Write(array.Length);
				for (int i = 0; i < array.Length; i++)
					writer.WriteValue(array.GetValue(i));

				return;
			}

			tagType(valueType.FullName);

			if (value is int) writer.Write((int)value);
			else if (value is bool) writer.Write((bool)value);
			else if (value is string) writer.Write((string)value);
			else throw new NotSupportedException("Unsopported argument type: " + value.GetType());
		}

		public static object ReadTaggedValue(this BinaryReader reader)
		{
			return reader.ReadValue(typeTag: reader.ReadString());
		}

		public static object ReadValue(this BinaryReader reader, string typeTag)
		{
			if (typeTag == typeof(int).FullName) return reader.ReadInt32();
			if (typeTag == typeof(bool).FullName) return reader.ReadBoolean();
			if (typeTag == typeof(string).FullName) return reader.ReadString();
			if (typeTag.EndsWith("[]"))
			{
				var elementTypeTag = typeTag.BeforeLast("[]");
				var elementType = Type.GetType(elementTypeTag);
				var array = Array.CreateInstance(elementType, reader.ReadInt32());
				for (int i = 0; i < array.Length; i++)
					array.SetValue(ReadValue(reader, elementTypeTag), i);
				return array;
			}
			throw new NotSupportedException("Unsupported parameter type: " + typeTag);
		}
	}
}