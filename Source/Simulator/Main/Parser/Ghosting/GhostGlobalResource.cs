using Uno.UX.Markup.Reflection;

namespace Outracks.Simulator.Parser
{
	class GhostGlobalResource : IGlobalResource
	{
		public GhostGlobalResource(string fullPath, IDataType dataType, string globalSymbol)
		{
			GlobalSymbol = globalSymbol;
			DataType = dataType;
			FullPath = fullPath;
		}

		public string FullPath { get; private set; }
		public IDataType DataType { get; private set; }
		public string GlobalSymbol { get; private set; }
	}
}