using System;
using Outracks.CodeCompletion;
using Uno.Compiler.API.Domain;
using Uno.Compiler.API.Domain.IL;
using Uno.Compiler.API.Domain.IL.Members;
using Uno.Compiler.API.Domain.IL.Types;
using Uno.Compiler.Frontend.Analysis;

namespace Outracks.UnoDevelop.CodeNinja.CodeCompleter
{
    static class CodeCompleterHelperMethods
    {
        // TODO: Move method out from here
        public static string ProtectionModifierKeywords(this Modifiers m)
        {
            if (m.HasFlag(Modifiers.Internal) && m.HasFlag(Modifiers.Protected)) return "protected internal ";
            else if (m.HasFlag(Modifiers.Private)) return "private";
            else if (m.HasFlag(Modifiers.Protected)) return "protected";
            else if (m.HasFlag(Modifiers.Public)) return "public";
            else throw new Exception("Invalid protection modifiers");
        }
    }

    public partial class CodeCompleter
    {
        string GetLineIndent()
        {
            int ofs = _reader.Offset;

            string indent = "";
            while (true)
            {
                var t = _reader.ReadTextReverse(1);

                if (t == null || t == "\n")
                {
                    _reader.Offset = ofs;
                    return indent;
                }


                if (t[0] == ' ') indent += " ";
				else if (t[0] == '\t') indent += "\t";
                else indent = "";
            }
        }

        void SuggestOverrides()
        {
            if (!(_context.TypeOrNamespace is DataType)) return;


            bool hasProtectionModifiers = false;
            int pp = _reader.Offset;
            while (true)
            {
                switch (_reader.ReadTokenReverse())
                {
                    case TokenType.Public:
                    case TokenType.Protected:
                    case TokenType.Private:
                    case TokenType.Internal:
                        hasProtectionModifiers = true;
                        break;

                    case TokenType.Whitespace: continue;
                }
                break;
            }
            _reader.Offset = pp;

            while (char.IsWhiteSpace(_reader.ReadText(1)[0]))
            {
            }

            var dt = _context.TypeOrNamespace as DataType;
            var bt = dt.Base;

            while (bt != null)
            {
                foreach (var kk in bt.EnumerateMembers())
                {
                    var k = kk;
                    if (k.IsVirtual)
                    {
                        bool found = false;
                        foreach (var u in dt.EnumerateMembers())
                        {
                            if (k.Name == u.Name && (k.GetType().Equals(u.GetType())))
                            {
                                found = true;
                            }
                        }

                        if (!found)
                        {
                            switch (k.MemberType)
                            {
                                case MemberType.Method:
                                    {
                                        var meth = k as Method;
                                        Suggest(
                                                SuggestItemType.Method,
                                                k,
                                                k.Name,
                                                () =>
                                                {
														string indent = "";//GetLineIndent();
                                                        string pretext = k.ReturnType.ToString() + " " + meth.Name + "(";
                                                        for (int i = 0; i < meth.Parameters.Length; i++)
                                                        {
                                                            if (i > 0) pretext += ", ";
                                                            pretext += meth.Parameters[i].Type.ToString() + " " + meth.Parameters[i].Name;
                                                        }
                                                        pretext += ")\n" + indent + "{\n" + indent + "\t";

                                                        if (!(meth.ReturnType is VoidType)) pretext += "return ";
                                                        pretext += "base." + meth.Name + "(";

                                                        for (int i = 0; i < meth.Parameters.Length; i++)
                                                        {
                                                            if (i > 0) pretext += ", ";
                                                            pretext += meth.Parameters[i].Name;
                                                        }

                                                        pretext += ");";
                                                        return pretext;
                                                    },
                                                () => "\n" + "}\n",
                                                () => "", //TODO: Build description here
                                                null, //TODO: Insert access modifiers here
                                                null, //TODO: Insert field modifiers here
                                                null, //TODO: Insert method arguments here
                                                (e) =>
                                                {
                                                    if (!hasProtectionModifiers)
                                                        e.InsertText(_reader.Offset-1, meth.Modifiers.ProtectionModifierKeywords() + " ");
                                                }
                                            );
                                    }
                                    break;
                                case MemberType.Property:
                                    {
                                        Suggest(SuggestItemType.Property, k, k.Name);
                                    }
                                    break;
                                case MemberType.Event:
                                    {
                                        Suggest(SuggestItemType.Property, k, k.Name);
                                    }
                                    break;
                            }

                        }
                    }
                }

                bt = bt.Base;
            }


        }
    }
}
