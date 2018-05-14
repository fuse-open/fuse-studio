using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;

namespace SketchConverter.SketchParser
{
	public class SketchArchive : ISketchArchive
	{
		ZipFile _zip;

		public SketchArchive(Stream sketchFileStream)
		{
			_zip = new ZipFile(sketchFileStream);
		}

		public SketchArchive()
		{
		}

		public void Dispose()
		{
			_zip?.Close();
		}

		public Stream OpenFile(string path)
		{
			var entryIndex = _zip.FindEntry(path, true);
			if (entryIndex < 0)
			{
				return null;
			}
			return _zip.GetInputStream(entryIndex);
		}

		public void Load(FileStream stream)
		{
			try
			{
				_zip = new ZipFile(stream);
			}
			catch (ZipException zipException)
			{
				throw new SketchParserException("Can't open sketch file. This file might be on an old format. <ADD SOME HELPFUL HINT?>: " + zipException.Message, zipException);
			}
			catch (Exception e)
			{
				throw new SketchParserException("Can't open sketch file: " + e.Message, e);
			}
		}
	}
}
