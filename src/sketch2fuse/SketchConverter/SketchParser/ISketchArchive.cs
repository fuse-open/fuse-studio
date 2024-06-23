using System;
using System.IO;

namespace SketchConverter.SketchParser
{
	public interface ISketchArchive : IDisposable
	{
		Stream OpenFile(string path);
	}
}
