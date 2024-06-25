using System.Xml.Linq;

namespace Outracks.Simulator.Parser
{
	class ParsedDocument
	{
		public readonly XElement RootElement;
		public readonly string Path;

		public ParsedDocument(XElement rootElement, string path)
		{
			RootElement = rootElement;
			Path = path;
		}
	}
}