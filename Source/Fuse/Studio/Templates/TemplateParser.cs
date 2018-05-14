using System;
using System.Collections.Generic;
using System.Text;

namespace Outracks.Fuse.Templates
{
	public class TemplateParser
    {
	    readonly ITemplateVariableResolver _variableResolver;

	    const string StartRegion = "_$$";
        const string EndRegion = "$$_";

		const string StartTag = "_$";
		const string EndTag = "$_";

	    public TemplateParser(ITemplateVariableResolver variableResolver)
	    {
		    _variableResolver = variableResolver;
	    }

		/// <exception cref="TemplateParseError" />
		public string ReplaceVariables(string template)
		{
			var sb = new StringBuilder();
			foreach (var token in Parse(template))
			{
				sb.Append(token.MatchWith(
					variable => _variableResolver.ResolveVariable(variable.Name),
					text => text.Text));
			}
			return sb.ToString();
	    }

	    public string PreprocessIncludeRegions(string template)
	    {
            //normalize line endings
            template = template.Split("\r").Join("\n").Split("\n\n").Join("\n"); 
		
		    var lines = template.Split("\n");
		    var source = "";
		    var level = 0;
		    foreach (var l in lines) 
		    {
			    if (l.IndexOf(StartRegion, StringComparison.Ordinal) > -1) 
			    {
				    var name = l.Substring(StartRegion.Length);
				    if (!_variableResolver.HasVariable(name)) 
					    level++;
			    }
			    else if (l.IndexOf(EndRegion, StringComparison.Ordinal) > -1) 
			    {
				    if (level > 0)
					    level--;
			    }
			    else 
			    {
			        if (level > 0)
			        {
			            continue;
			        }
			        source += l + "\n";
			    }
		    }
	        return source;
	    }

		static IEnumerable<TemplateToken> Parse(string text)
		{
			var scanner = new TextScanner(text);

			while (true)
			{
				// Find the next start tag
				var endOfLastToken = scanner.Position;
				var startTag = scanner.ScanTo(StartTag);
				if (!startTag.HasValue) 
				{
					// No more start tags found, emit remaining text token
					yield return new TemplateText(text, endOfLastToken, scanner.End);
					yield break;
				}

				// Otherwise emit text between end of last token and the start tag
				yield return new TemplateText(text, endOfLastToken, startTag.Value);

				// Find the next end tag
				var afterStartTag = scanner.Position;
				var endTag = scanner.ScanTo(EndTag);
				if (!endTag.HasValue)
					throw new TemplateParseError("Couldn't find closing '" + EndTag + "'", startTag.Value.ToPosition(text));

				// emit the text between the start and end tag 
				yield return new TemplateVariable(text.Substring(afterStartTag, endTag.Value - afterStartTag));
			}
		}
    }


}
