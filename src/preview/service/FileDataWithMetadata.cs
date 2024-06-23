using System;
using System.Reactive.Linq;
using Outracks;
using Outracks.IO;

namespace Fuse.Preview
{
	public static class FileDataWithMetadata
	{
		public static FileDataWithMetadata<T> Create<T>(T metadata, byte[] data)
		{
			return new FileDataWithMetadata<T>(metadata, data);
		}

		public static IObservable<FileDataWithMetadata<AbsoluteFilePath>> DiffFileContent(this IObservable<AbsoluteFilePath> observable, IFileSystem fs)
		{
			return observable.DiffFileContent(fs, _ => _);
		}

		public static IObservable<FileDataWithMetadata<T>> DiffFileContent<T>(this IObservable<T> observable, IFileSystem fs, Func<T, AbsoluteFilePath> path)
		{
			return observable.Select(
				dep =>
				{
					using(var stream = fs.Read(path(dep), 10))
						return Create(dep, stream.ReadAllBytes());
				})
				.BufferPrevious()
				.Where(buffer =>
				{
					var prev = buffer.Previous;
					var cur = buffer.Current;

					// Just go on, if no previous version exist.
					if (prev.HasValue == false)
						return true;

					// Check if the byte sequences aren't equal
					// to filter out equal sequences.
					return EqualsFast(prev.Value.Data, cur.Data) == false;
				})
				.Select(b => b.Current);
		}

		// Apps made in Fuse may have pretty big assets as movies etc.
		// where the byte sequence equals (linq) method performs really bad
		// So this is just a faster solution
		static bool EqualsFast(byte []a, byte []b)
		{
			if (a.Length != b.Length)
				return false;

			for (int i = 0; i < a.Length; ++i)
			{
				if (a[i] != b[i])
					return false;
			}

			return true;
		}
	}

	public class FileDataWithMetadata<T>
	{
		public readonly T Metadata;
		public readonly byte[] Data;

		public FileDataWithMetadata(T metadata, byte[] data)
		{
			Metadata = metadata;
			Data = data;
		}

		public FileDataWithMetadata<TIn> WithMetadata<TIn>(TIn metadata)
		{
			return new FileDataWithMetadata<TIn>(metadata, Data);
		}
	}
}