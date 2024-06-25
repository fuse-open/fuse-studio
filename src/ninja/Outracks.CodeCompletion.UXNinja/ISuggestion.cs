using System.Collections.Generic;
using Outracks.CodeCompletion;

namespace Outracks.UnoDevelop.UXNinja
{
	interface ISuggestion
	{
		IEnumerable<SuggestItem> Suggest();
	}
}
