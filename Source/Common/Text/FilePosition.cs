using System;
using System.IO;
using Outracks.IO;

namespace Outracks
{
	public struct FilePosition : IEquatable<FilePosition>
	{
		public readonly AbsoluteFilePath File;
		public readonly TextPosition Position;

		public FilePosition(AbsoluteFilePath file, TextPosition position)
		{
			Position = position;
			File = file;
		}

		public override bool Equals(object obj)
		{
			return obj is FilePosition && Equals((FilePosition) obj);
		}

		public override int GetHashCode()
		{
			return (File.GetHashCode() * 397) ^ Position.GetHashCode();
		}

		public bool Equals(FilePosition other)
		{
			return File == other.File && Position == other.Position;
		}

		public static void Write(BinaryWriter writer, FilePosition position)
		{
			AbsoluteFilePath.Write(writer, position.File);
			TextPosition.Write(writer, position.Position);
		}

		public static FilePosition Read(BinaryReader reader)
		{
			return new FilePosition(
				AbsoluteFilePath.Read(reader),
				TextPosition.Read(reader));
		}
	}
}