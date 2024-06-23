using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Outracks.IO
{
	public static class Reading
	{
		/// <exception cref="AggregateException"></exception>
		public static TextDocumentContent ReadTextDocument(this IFileSystem fileSystem, AbsoluteFilePath path, int maxAttempts)
		{
			return new TextDocumentContent(path, fileSystem.ReadAllText(path, maxAttempts));
		}

		/// <exception cref="AggregateException"></exception>
		public static string ReadAllText(this IFileSystem fileSystem, AbsoluteFilePath path, int maxAttempts)
		{
			return RetryLoop.Try(
				maxAttempts,
				() =>
				{
					using (var stream = fileSystem.OpenRead(path))
						return stream.ReadToEnd();
				});
		}

		/// <exception cref="IOException"></exception>
		public static string ReadToEnd(this Stream stream)
		{
			return new StreamReader(stream).ReadToEnd();
		}

		/// <exception cref="AggregateException"></exception>
		public static byte[] ReadAllBytes(this IFileSystem fileSystem, AbsoluteFilePath path, int maxAttempts)
		{
			return RetryLoop.Try(
				maxAttempts,
				() =>
				{
					using (var stream = fileSystem.OpenRead(path))
						return stream.ReadAllBytes();
				});
		}

		public static Stream Read(this IFileSystem fileSystem, AbsoluteFilePath path, int maxAttempts)
		{
			return RetryLoop.Try(
				maxAttempts,
				() => fileSystem.OpenRead(path));
		}

		/// <exception cref="IOException"></exception>
		public static byte[] ReadAllBytes(this Stream stream)
		{
			return new BinaryReader(stream).ReadAllBytes();
		}

		/// <exception cref="IOException"></exception>
		public static byte[] ReadAllBytes(this BinaryReader reader)
		{
			const int bufferSize = 4096;

			using (var ms = new MemoryStream())
			{
				byte[] buffer;
				do
				{
					buffer = reader.ReadBytes(bufferSize);
					ms.Write(buffer, 0, buffer.Length);
				}
				while (buffer.Length == bufferSize);

				return ms.ToArray();
			}
		}


		/// <exception cref="IOException"></exception>
		public static async Task<byte[]> ReadAllBytesAsync(this Stream stream)
		{
			using (var ms = new MemoryStream())
			{
				await stream.CopyToAsync(ms);
				return ms.ToArray();
			}
		}

		public static Task WriteAllBytesAsync(this Stream stream, byte[] bytes)
		{
			return stream.WriteAsync(bytes, 0, bytes.Length, CancellationToken.None);
		}
	}

}