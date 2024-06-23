using System;
using System.Collections.Generic;
using Outracks.CodeCompletion;
using Outracks.UnoDevelop.CodeNinja;

namespace Outracks.UnoDevelop.UXNinja
{
	static class SuggestionHelper
	{
		public static SuggestItem Suggest(SuggestItemType type,
			object docObject,
			string name,
			Func<string> pretext = null,
			Func<string> posttext = null,
			Action<IAutoCompleteCodeEditor> commitAction = null,
			SuggestItemPriority priority = SuggestItemPriority.Normal)
		{
			return new SuggestItem(
				name,
				() => (DocumentationHinter.CreateHtmlHint(docObject, null, new List<string>()) ?? ""),
				type,
				pretext,
				posttext,
				null,
				null,
				null,
				null,
				commitAction,
				priority);
		}
	}
}
