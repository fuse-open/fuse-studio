using System;
using Uno.Collections;
using System.IO;
using Uno;

namespace Outracks.Simulator
{
	public class ImmutableList<T> : IEnumerable<T>
	{
		public static ImmutableList<T> Empty
		{
			get { return new ImmutableList<T>(new T[0]); }
		}


		readonly T[] _array;
		internal ImmutableList(T[] array)
		{
			_array = array;
		}

		public int Count
		{
			get { return _array.Length; }
		}

		public IEnumerator<T> GetEnumerator()
		{
			return ((IEnumerable<T>)_array).GetEnumerator();
		}

		public ImmutableList<T> Add(T element)
		{
			var copy = new T[Count + 1];
			Array.Copy(_array, copy, Count);
			copy[Count] = element;
			return new ImmutableList<T>(copy);
		}

		public ImmutableList<T> InsertRange(int index, IEnumerable<T> items)
		{
			var itemsArray = items.ToArray();
			var copy = new T[Count + itemsArray.Length];
			for (int i = 0; i < itemsArray.Length; i++)
				copy[i] = itemsArray[i];
			for (int i = 0; i < Count; i++)
				copy[itemsArray.Length + i] = _array[i];
			return new ImmutableList<T>(copy);
		}

		public ImmutableList<T> Replace(T element, T replacement)
		{
			var copy = new T[Count];
			for (int i = 0; i < Count; i++)
				copy[i] = _array[i].Equals(element)
					? replacement
					: _array[i];
			return new ImmutableList<T>(copy);
		}

		public T Get(int i)
		{
			return _array[i];
		}

		public T this[int i]
		{
			get { return _array[i]; }
		}

		public override string ToString()
		{
			return "[" + ((IEnumerable<T>)this).JoinToString(", ") + "]";
		}
	}


	public static class List
	{
		public static ImmutableList<T> ToList<T>(this Optional<T> element)
		{
			if (!element.HasValue)
				return ImmutableList<T>.Empty;

			var array = new T[1];
			array[0] = element.Value;
			return new ImmutableList<T>(array);
		}

		public static ImmutableList<T> Create<T>(params T[] elements)
		{
			return new ImmutableList<T>(elements); // TODO: consider if we should copy array
		}

		public static ImmutableList<T> ToImmutableList<T>(this IEnumerable<T> self)
		{
			return new ImmutableList<T>(self.ToArray());
		}

		public static void Write<T>(ImmutableList<T> array, BinaryWriter writer, Action<T, BinaryWriter> elementWriter)
		{
			Write(writer, array, elementWriter);
		}

		public static void Write<T>(this BinaryWriter writer, IEnumerable<T> array, Action<T, BinaryWriter> elementWriter)
		{
			writer.Write(array.Count());
			foreach (var elm in array)
				elementWriter(elm, writer);
		}

		public static void Write<T>(this BinaryWriter writer, ImmutableList<T> array, Action<T, BinaryWriter> elementWriter)
		{
			writer.Write(array.Count);
			foreach (var elm in array)
				elementWriter(elm, writer);
		}

		public static ImmutableList<T> Read<T>(BinaryReader reader, Func<BinaryReader, T> elementReader)
		{
			return ReadImmutableList(reader, elementReader);
		}

		public static void WriteImmutableList(BinaryWriter writer, IEnumerable<object> elements, Action<object> writeElement)
		{
			var elementsArray = elements.ToArray();
			writer.Write(elementsArray.Length);
			foreach (var element in elementsArray)
				writeElement(element);
		}

		public static ImmutableList<T> ReadImmutableList<T>(this BinaryReader reader, Func<BinaryReader, T> elementReader)
		{
			var array = new T[reader.ReadInt32()];
			for (int i = 0; i < array.Length; i++)
				array[i] = elementReader(reader);
			return new ImmutableList<T>(array);
		}
	}
}