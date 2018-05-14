using System;
using System.Collections.Generic;
using System.Linq;
using Uno.Compiler.API.Domain;
using Uno.Compiler.API.Domain.Graphics;
using Uno.Compiler.API.Domain.IL;
using Uno.Compiler.API.Domain.IL.Members;
using Uno.Compiler.Frontend.Analysis;

namespace Outracks.UnoDevelop.CodeNinja
{
    public static partial class DocumentationHinter
    {
        public static string SafeXml(string s)
        {
            return System.Security.SecurityElement.Escape(s);
        }

        public static string IdentifierToHtmlString(string id)
        {
            id = SafeXml(id);
            if (Tokens.IsReserved(id)) return Color(id, "blue");
            return id;
        }

        public static string DataTypeToHtmlString(DataType dt, IEnumerable<string> usings)
        {
            if (dt == null) return "";

            string type;
            if (TypeAliases.TryGetTypeFromAlias(dt.FullName, out type)) return "<span style=\"color: blue;\">" + dt.FullName + "</span>";

            string name = SafeXml(dt.ToString());

            foreach (var u in usings.OrderByDescending((s) => s.Length))
            {
                if (name.StartsWith(u))
                {
                    name = name.Substring(Math.Min(name.Length,u.Length+1));
                    break;
                }
            }

            var p = name.LastIndexOf('.') + 1;
            if (p == 0)
            {
                return "<span style=\"color: green;\">" + name + "</span>";
            }
            else
            {
                var head = name.Substring(0, p);
                var tail = name.Substring(p);
                return head + "<span style=\"color: green;\">" + tail + "</span>";
            }
        }

        public static string CreateHtmlHint(MetaProperty mp, IEnumerable<string> usings, string fixedSourceString)
        {
            string defs = " :<br />";

            if (mp.Definitions != null && mp.Definitions.Length > 0)
            {
                foreach (var k in mp.Definitions)
                {
                    foreach (var r in k.Requirements)
                    {

                        defs += "&nbsp;&nbsp;&nbsp;&nbsp;" + SafeXml(r.ToString()).Replace("req(", "<span style=\"color: blue;\">req</span>(");
                        defs += "<br />";
                    }
                    defs += "&nbsp;&nbsp;&nbsp;&nbsp;" + SafeXml(k.Value.ToString());

                    if (k != mp.Definitions.Last())
                    {
                        defs += ",<br /><br />";
                    }
                    else defs += ";<br /><br />";
                }
            }
            else
            {
                defs = "<br /><em>undefined</em><br /><br />";
            }

            if (fixedSourceString == null && !mp.Source.FullPath.Contains('<'))
            {
                fixedSourceString = "Declared in <span style=\"color: gray;\">" + SafeXml(System.IO.Path.GetFileName(mp.Source.FullPath)) + "</span>";
            }

            return DataTypeToHtmlString(mp.ReturnType, usings) + " " + mp.Name + defs + fixedSourceString;
        }
    }
}
