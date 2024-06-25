using System;
using System.Collections.Generic;

namespace Uno.Compiler
{
    public class DocumentationComment
    {
        public Dictionary<string, string> Params = new Dictionary<string, string>();

        public string ReturnValue;

        public string Summary;

        public string Body;

        public DocumentationComment(string comment)
        {
            try
            {
                comment = comment.Trim();

                var lines = comment.Split('\n');

                Body = "";

                foreach (var line in lines)
                {
                    var l = line.Trim('/', '*', ' ', '\t', '\r');

                    if (l.ToLower().StartsWith("@param"))
                    {
                        l = l.Substring("@param ".Length);
                        var x = l.IndexOf(' ');
                        if (x != -1)
                        {
                            var pname = l.Substring(0, x).Trim();
                            var pdoc = l.Substring(x, l.Length - x).Trim();
                            Params.Add(pname, pdoc);
                        }
                        continue;
                    }
                    else if (l.ToLower().StartsWith("@return"))
                    {
                        l = l.Substring("@return ".Length);
                        ReturnValue = l;
                        continue;
                    }
                    else
                    {
                        if (Summary == null && l.Length > 0) Summary = l;
                        else Body += l + " ";
                    }
                }
            }
            catch (Exception)
            {
            }
        }
    }
}