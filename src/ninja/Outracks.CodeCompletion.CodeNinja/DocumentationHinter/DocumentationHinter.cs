using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Outracks.UnoDevelop.CodeNinja.AmbientParser;
using Uno;
using Uno.Compiler.API;
using Uno.Compiler.API.Domain;
using Uno.Compiler.API.Domain.Graphics;
using Uno.Compiler.API.Domain.IL;
using Uno.Compiler.API.Domain.IL.Expressions;
using Uno.Compiler.API.Domain.IL.Members;
using Uno.Compiler.API.Domain.IL.Types;

namespace Outracks.UnoDevelop.CodeNinja
{
    public class FileEntity { public string Path; public FileEntity(string path) { Path = path; } }
    public class DirectoryEntity { public string Path; public DirectoryEntity(string path) { Path = path; } }

    public static partial class DocumentationHinter
    {
        static string Keyword(string text) { return "<span style=\"color: blue;\">" + text + "</span>"; }
        static string Color(string text, string color) { return "<span style=\"color: " + color + ";\">" + text + "</span>"; }

        static string TrimSafe(string namePath, Context context)
        {
            foreach (var u in context.Usings.OrderByDescending((s) => s.Length))
            {
                if (namePath.StartsWith(u)) return SafeXml(namePath.Substring(u.Length, namePath.Length - u.Length));
            }
            return SafeXml(namePath);
        }

        static string Trim(string namePath, Context context)
        {
            foreach (var u in context.Usings.OrderByDescending((s) => s.Length))
            {
                if (namePath.StartsWith(u)) return SafeXml(namePath.Substring(u.Length, namePath.Length - u.Length));
            }
            return namePath;
        }

        static string StripHTML(string inputString)
        {
            return Regex.Replace(inputString, "<.*?>", string.Empty);
        }


        public static Entity LastHintedEntity { get; set; }

        public static string CreateHtmlHint(object obj, ICompiler compiler, IEnumerable<string> usings)
        {
            var documentation = "";

            if (obj is Entity)
            {
                var doc = DocumentationCache.GetEntry(obj as Entity);

                if (doc != null && doc.Documentation != null && doc.Documentation.Length > 0)
                {
                    var fullDoc = doc.Documentation.Replace("@", "");

                    documentation = "<br />" + StripHTML(fullDoc) + "<br /><br /><em>Press F1 for more documentation</em>";
                }

                LastHintedEntity = obj as Entity;
            }

            if (obj is GetMetaProperty)
            {
                var rmp = obj as GetMetaProperty;
                return DataTypeToHtmlString(rmp.ReturnType, usings) + " " + rmp.Name + " " + Color("<em>(meta property)</em>", "gray") + documentation;
            }
            else if (obj is InvalidExpression)
            {
                return null;
            }
            else if (obj is ClassType)
            {
                var dt = obj as ClassType;
                return Keyword("class") + " " + SafeXml(dt.QualifiedName) + documentation;
            }
            else if (obj is Namespace)
            {
                var dt = obj as Namespace;
                return Keyword("namespace ") + " " + SafeXml(dt.QualifiedName) + documentation;
            }
            else if (obj is string)
            {
                string type;
                if (TypeAliases.TryGetTypeFromAlias(obj as string, out type))
                {
                    var s = compiler.ILFactory.GetEntity(Source.Unknown, type);
                    return (s != null ? (CreateHtmlHint(s, compiler, usings) ?? "") : "") + "<br /><br /><em><strong>" + obj as string + "</strong> is a type alias for " + SafeXml(type) + "</em>";
                }

                return Keyword(SafeXml(obj as string)) + " (keyword)";
            }
            else if (obj is StructType)
            {
                var dt = obj as StructType;
                return Keyword("struct") + " " + SafeXml(dt.QualifiedName) + documentation;
            }
            else if (obj is InterfaceType)
            {
                var dt = obj as InterfaceType;
                return Keyword("interface") + " " + SafeXml(dt.QualifiedName) + documentation;
            }
            else if (obj is DelegateType)
            {
                var dt = obj as DelegateType;
                return Keyword("delegate") + " " + SafeXml(dt.ToString()) + documentation;
            }
            else if (obj is EnumType)
            {
                var dt = obj as EnumType;
                return Keyword("enum") + " " + SafeXml(dt.QualifiedName) + documentation;
            }
            else if (obj is Variable)
            {
                var v = obj as Variable;
                return DataTypeToHtmlString(v.ValueType, usings) + " " + SafeXml(v.Name) + " (local variable)";
            }
            else if (obj is Block)
            {
                var b = obj as Block;
                return SafeXml(b.Name) + " (meta property block)" + documentation;
            }
            else if (obj is Parameter)
            {
                var p = obj as Parameter;
                return DataTypeToHtmlString(p.Type, usings) + " " + SafeXml(p.Name) + " (method argument)";
            }
            else if (obj is GenericParameterType)
            {
                var gpt = obj as GenericParameterType;

                if (gpt.IsGenericMethodParameter)
                    return SafeXml(gpt.ToString()) + " : generic parameter type of method " + SafeXml(gpt.GenericMethodParent.ToString());
                else
                    return SafeXml(gpt.ToString()) + " : generic parameter type of type " + SafeXml(gpt.GenericTypeParent.ToString());
            }
            else
            {
                var usingList = usings as IList<string> ?? usings.ToList();
                if (obj is Property)
                {
                    var p = obj as Property;

                    return
                        DataTypeToHtmlString(p.ReturnType, usingList) + " " + DataTypeToHtmlString(p.DeclaringType, usingList) + "." + p.Name + " { " +
						(p.GetMethod != null && p.GetMethod.Body != null ? "get; " : "") +
						(p.SetMethod != null && p.SetMethod.Body != null ? "set; " : "") +
                        "} " + documentation;
                }
                else if (obj is Field)
                {
                    var f = obj as Field;

                    return
                        DataTypeToHtmlString(f.ReturnType, usingList) + " " + DataTypeToHtmlString(f.DeclaringType, usingList) + "." + f.Name + documentation;
                }
                else if (obj is Constructor)
                {
                    var c = obj as Constructor;

                    string s = "(";
                    foreach (var arg in c.Parameters)
                    {
                        if (arg != c.Parameters[0]) s += ", ";
                        s += DataTypeToHtmlString(arg.Type, usingList) + " " + arg.Name;
                        if (arg.OptionalDefault != null) s += " = " + SafeXml(arg.OptionalDefault.ToString());
                    }
                    s += ")";

                    var overloads = c.DeclaringType.Constructors.Count;
                    if (overloads > 1)
                        s += Color(" <em>(+" + (overloads - 1) + " overloads)</em>", "gray");

                    return
                        DataTypeToHtmlString(c.ReturnType, usingList) + " " + DataTypeToHtmlString(c.DeclaringType, usingList) + s + documentation;
                }
                else if (obj is Method)
                {
                    var m = obj as Method;

                    string s = "(";
                    foreach (var arg in m.Parameters)
                    {
                        if (arg != (m as Method).Parameters[0]) s += ", ";
                        s += DataTypeToHtmlString(arg.Type, usingList) + " " + arg.Name;
                        if (arg.OptionalDefault != null) s += " = " + SafeXml(arg.OptionalDefault.ToString());
                    }
                    s += ")";

                    var overloads = 0;
                    var bt = m.DeclaringType;

                    while (bt != null)
                    {
                        foreach (var om in bt.Methods)
                            if (om.Name == m.Name)
                                overloads++;

                        bt = bt.Base;
                    }

                    if (overloads > 1)
                        s += Color(" <em>(+" + (overloads - 1) + " overloads)</em>", "gray");

                    return
                        DataTypeToHtmlString(m.ReturnType, usingList) + " " + DataTypeToHtmlString(m.DeclaringType, usingList) + "." + SafeXml(m.Name) + s + documentation;
                }
                else if (obj is MetaProperty)
                {
                    return CreateHtmlHint(obj as MetaProperty, usingList, null);
                }
                else if (obj is FileEntity)
                {
                    return SafeXml((obj as FileEntity).Path) + " <em>(external file)</em>";
                }
                else if (obj is DirectoryEntity)
                {
                    return SafeXml((obj as FileEntity).Path) + " <em>(external directory)</em>";
                }
            }

            if (obj == null) return null;

            return documentation;
        }
    }
}
