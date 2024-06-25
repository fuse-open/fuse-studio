using System.Threading.Tasks;
using SketchConverter.API;
using SketchConverter.SketchModel;

namespace SketchConverter.SketchParser
{
	public interface ISketchParser
	{
		SketchDocument Parse(ISketchArchive stream);
	}

	public class SketchParser : ISketchParser
	{
		private readonly ILogger _log;

		public SketchParser(ILogger log)
		{
			_log = log;
		}

		public SketchDocument Parse(ISketchArchive stream)
		{
			return ParseAsync(stream).Result;
		}

		private async Task<SketchDocument> ParseAsync(ISketchArchive stream)
		{
			return await new SketchParserInternal(stream, _log).ParseDocumentAsync();
		}
	}
}
