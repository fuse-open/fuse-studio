using System.Collections.Generic;
using Uno;

namespace Outracks.UnoDevelop.UXNinja
{
	public interface IContext
	{
		ContextType Type { get; }

		Source SourceLocation { get; }

		IDictionary<string, string> NamespaceDeclarations { get; }

		IElementContext Root { get; }

		IElementContext TargetElement { get; }
	}
}
