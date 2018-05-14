using System.Collections.Generic;

namespace SketchConverter.API
{
	public interface IConverter
	{
		void Convert(IEnumerable<string> sketchFilePath, string outputdirPath);
	}
}
