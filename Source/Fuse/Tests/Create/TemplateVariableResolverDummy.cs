using System.Collections.Generic;
using Outracks.Fuse.Templates;

namespace Outracks.Templates.Tests
{
	public class VariableResolverDummy : ITemplateVariableResolver
	{
		readonly Dictionary<string, string> _variableNames;

		public VariableResolverDummy(Dictionary<string, string> variableNames)
		{
			_variableNames = variableNames;
		}

		public string ResolveVariable(string name)
		{
			return _variableNames[name];
		}

	    public bool HasVariable(string name)
	    {
	        return _variableNames.ContainsKey(name);
	    }
	}
}