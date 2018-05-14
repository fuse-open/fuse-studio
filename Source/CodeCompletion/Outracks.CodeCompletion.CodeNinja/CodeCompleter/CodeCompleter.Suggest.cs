using System;
using System.Collections.Generic;
using Outracks.CodeCompletion;
using Uno.Compiler.API.Domain;
using Uno.Compiler.Frontend.Analysis;
using Uno.Compiler.API.Domain.Graphics;
using Uno.Compiler.API.Domain.IL;
using Uno.Compiler.API.Domain.IL.Types;

namespace Outracks.UnoDevelop.CodeNinja.CodeCompleter
{
    enum SuggestionFilter
    {
        NoTypes = 0,
        Classes = 0x1,
        Structs = 0x2,
        Enums = 0x4,
        Delegates = 0x8,
        Interfaces = 0x10,
        TypeAliases = 0x20,

        AllTypes = Classes | Structs | Enums | Delegates | Interfaces | TypeAliases,

        UsingStaticMembers = 0x40,

        Blocks = 0x80,
    }

    public partial class CodeCompleter
    {
        List<SuggestItem> _suggestions = new List<SuggestItem>();
        HashSet<string> _suggestionNames = new HashSet<string>();

        void Suggest(
            SuggestItemType type, 
            object docObject, 
            string suggestionText, 
            Func<string> pretext = null, 
            Func<string> posttext = null, 
            Func<string> desctext = null, 
            string[] accessModifiers = null, 
            string[] fieldModifiers = null, 
            MethodArgument[] arguments = null, 
            Action<IAutoCompleteCodeEditor> commitAction = null, 
            SuggestItemPriority priority = SuggestItemPriority.Normal,
			bool omitDuplicates = true)
        {
			if (omitDuplicates && _suggestionNames.Contains(suggestionText)) 
				return;
            if (suggestionText.StartsWith(".")) 
				return;
            if (type == SuggestItemType.Class && _context.expectsConstructor)
            {
                type = SuggestItemType.Constructor;
                var dt = (DataType)docObject;
                foreach (var ctor in dt.Constructors)
                {
                    desctext = () => "";
                    var args = new List<MethodArgument>();
                    for (var i = 0; i < ctor.Parameters.Length; i++)
                    {
                        var arg = ctor.Parameters[i];
                        args.Add(new MethodArgument(arg.Name, arg.Type.Name, arg.Modifier == ParameterModifier.Out));
                    }
                    arguments = args.ToArray();
                    //This repetition is pretty ugly.. Oh well
                    var item = new SuggestItem(
                            suggestionText,
                            () => (DocumentationHinter.CreateHtmlHint(docObject, _context.Compiler, _context.Usings) ?? ""),
                            type,
                            pretext,
                            posttext,
                            desctext,
                            accessModifiers,
                            fieldModifiers,
                            arguments,
                            commitAction,
                            priority);
					_suggestions.Add(item);
                }
            } else {
				if (desctext == null) desctext = () => ""+type;
                _suggestions.Add(
                        new SuggestItem(
                            suggestionText, 
                            () => (DocumentationHinter.CreateHtmlHint(docObject, _context.Compiler, _context.Usings) ?? ""), 
                            type, 
                            pretext, 
                            posttext, 
                            desctext,
                            accessModifiers,
                            fieldModifiers,
                            arguments,
                            commitAction,
                            priority)
                        );
			}

			if(omitDuplicates) 
				_suggestionNames.Add(suggestionText);
        }

        void Suggest(IEnumerable<Block> e)
        {
            foreach (var b in e)
            {
                Suggest(SuggestItemType.Block, b, b.Name);
            }
        }

        void Suggest(IEnumerable<Namescope> e, string ignoredClassName = null, SuggestTypesMode mode = SuggestTypesMode.Everything)
        {
            foreach (var n in e) Suggest(n, ignoredClassName, mode);
        }

        void Suggest(Namescope ns, string ignoredClassName = null, SuggestTypesMode mode = SuggestTypesMode.Everything)
        {
            if (ns is Namespace)
                Suggest(SuggestItemType.Namespace, ns, ns.Name);
            if (ns is ClassType)
            {
                if (ignoredClassName == null || ignoredClassName != ns.Name)
                {
                    if (_exceptionTypesOnly)
                    {
                        var ct = ns as ClassType;
                        if (!ct.Equals(_context.Compiler.ILFactory.Essentials.Exception) && !ct.IsSubclassOf(_context.Compiler.ILFactory.Essentials.Exception)) return;
                    }

                    string n = ns.Name;

                    var sit = SuggestItemType.Class;

                    if (mode == SuggestTypesMode.Blocks)
                    {
                        if (n.EndsWith("BlockFactory"))
                        {
                            n = n.Substring(0, n.Length - "BlockFactory".Length);
                            sit = SuggestItemType.BlockFactory;
                        }
                        else return;
                    }
                    else if (mode == SuggestTypesMode.Importers)
                    {
                        if (n.EndsWith("Importer")) 
                        {
                            n = n.Substring(0, n.Length - "Importer".Length);
                            sit = SuggestItemType.Importer;
                        }
                        else return;
                    }
                    else if (mode == SuggestTypesMode.Attributes)
                    {
                        if (n.EndsWith("Attribute"))
                        {
                            n = n.Substring(0, n.Length - "Attribute".Length);
                            sit = SuggestItemType.Class;
                        }
                        else return;
                    }

                    Suggest(sit, ns, n); 
                }
            }


            if (mode == SuggestTypesMode.Everything)
            {
                if (!_exceptionTypesOnly && ns is InterfaceType)
                    Suggest(SuggestItemType.Interface, ns, ns.Name);

                if (!_exceptionTypesOnly && ns is StructType)
                    Suggest(SuggestItemType.Struct, ns, ns.Name);

                if (!_exceptionTypesOnly && ns is DelegateType)
                    Suggest(SuggestItemType.Delegate, ns, ns.Name);

                if (!_exceptionTypesOnly && ns is EnumType)
                    Suggest(SuggestItemType.Enum, ns, ns.Name);

                if (ns is GenericParameterType)
                {
                    Suggest(SuggestItemType.GenericParameterType, ns, ns.Name);
                }
            }
        }

        void SuggestKeywords(params string[] keywords)
        {
            foreach (var k in keywords)
            {
                Suggest(SuggestItemType.Keyword, k, k);
            }
        }

        bool SuggestAttributesIfInsideAttributeDeclaration()
        {
            // Check if we are inside an attribute declaration
            var offs = _reader.Offset;
            while (true)
            {
                var t = _reader.ReadTokenReverse();

                switch (t)
                {
                    case TokenType.Identifier:
                    case TokenType.Whitespace:
                    case TokenType.Period:
                        continue;

                    case TokenType.LeftSquareBrace:
                        {
                            _reader.Offset = offs;
                            var memberExp = FindMemberExpression(out t, false);

                            SuggestTypes(memberExp, false, false, SuggestTypesMode.Attributes);
                            return true;
                        }
                }
                break;
            }

            _reader.Offset = offs;
            return false;
        }

        void SuggestExtensionMethod()
        {
            var offs = _reader.Offset;
            var foundThis = false;

            while (true)
            {
                var t = _reader.ReadTokenReverse();
                switch (t)
                {
                    case TokenType.Whitespace:               
                        continue;
                    case TokenType.This:                        
                        foundThis = true;
                        break;
                }
                break;
            }

            _reader.Offset = offs;
            if(!foundThis)
                SuggestKeywords("this");
        }

        void SuggestRootItems()
        {
            if (SuggestAttributesIfInsideAttributeDeclaration()) return;

            SuggestKeywords("using", "static", "namespace", "class", "struct", "block", "enum", "delegate", "public", "partial", "intrinsic", "drawable", "private");
        }
    }
}
