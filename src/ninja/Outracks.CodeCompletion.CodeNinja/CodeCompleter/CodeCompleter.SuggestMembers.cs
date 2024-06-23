using System.Collections.Generic;
using System.Linq;
using System.Text;
using Outracks.CodeCompletion;
using Uno;
using Uno.Compiler.API.Domain;
using Uno.Compiler.API.Domain.AST.Expressions;
using Uno.Compiler.API.Domain.Graphics;
using Uno.Compiler.API.Domain.IL;
using Uno.Compiler.API.Domain.IL.Members;
using Uno.Compiler.API.Domain.IL.Types;
using Uno.Compiler.Core.Syntax.Binding;
using Uno.Compiler.Core.Syntax.Compilers;
using Expression = Uno.Compiler.API.Domain.AST.Expressions.AstExpression;
using ExpressionType = Uno.Compiler.API.Domain.AST.Expressions.AstExpressionType;

namespace Outracks.UnoDevelop.CodeNinja.CodeCompleter
{
    public partial class CodeCompleter
    {
        List<Method> FindAllExtensionTypeMethods(FunctionCompiler fc, Source source)
        {
            var usings = fc.Compiler.NameResolver.TryGetUsings(fc.Namescope, source);

            var staticClasses = new HashSet<DataType>();
            for (var scope = fc.Namescope; scope != null; scope = scope.Parent)
            {
                var ns = scope as Namespace;

                if (ns != null)
                    foreach (var dt in ns.Types)
                        if (dt.IsStatic && !dt.IsGenericDefinition)
                            staticClasses.Add(dt);
            }

            if (usings != null)
            {
                foreach (var ns in usings.Namespaces)
                    foreach (var dt in ns.Types)
                        if (dt.IsStatic && !dt.IsGenericDefinition)
                            staticClasses.Add(dt);

                foreach (var dt in usings.Types)
                    if (dt.IsStatic && !dt.IsGenericDefinition)
                        staticClasses.Add(dt);
            }

            if (staticClasses.Count == 0)
                return null;

            var extensionMethods = new List<Method>();

            foreach (var dt in staticClasses)
                foreach (var m in dt.Methods)
                    if (m.IsStatic &&
                        m.Parameters.Length > 0 && m.Parameters[0].Modifier == ParameterModifier.This)
                        extensionMethods.Add(m);

            if (extensionMethods.Count == 0)
                return null;

            return extensionMethods;
        }

        bool TrySuggestMembers(FunctionCompiler fc, Expression dte, bool includeBlocks)
        {
            var pe = fc.ResolveExpression(dte, null);
            fc.Compiler.AstProcessor.Process();

            if (pe != null)
            {
                var extensionTypeMethods = FindAllExtensionTypeMethods(fc, dte.Source);
                switch (pe.ExpressionType)
                {
                    case PartialExpressionType.Namespace: SuggestNamespaceMembers((pe as PartialNamespace).Namespace, includeBlocks); break;
                    case PartialExpressionType.Type: SuggestTypeMembers((pe as PartialType).Type, AccessorLevel.Unknown, true, includeBlocks, true, extensionTypeMethods); break;
                    case PartialExpressionType.Block: SuggestBlockItems((pe as PartialBlock).Block); break;
                    case PartialExpressionType.Variable: SuggestTypeMembers((pe as PartialVariable).Variable.ValueType, AccessorLevel.Unknown, false, false, true, extensionTypeMethods); break;
					case PartialExpressionType.Parameter: SuggestTypeMembers((pe as PartialParameter).Function.Match(f => f.Parameters, l => l.Parameters)[(pe as PartialParameter).Index].Type, AccessorLevel.Unknown, false, false, true, extensionTypeMethods); break;
                    case PartialExpressionType.Field: SuggestTypeMembers((pe as PartialField).Field.ReturnType, AccessorLevel.Unknown, false, false, true, extensionTypeMethods); break;
                    case PartialExpressionType.Event: break;
                    case PartialExpressionType.Property: SuggestTypeMembers((pe as PartialProperty).Property.ReturnType, AccessorLevel.Unknown, false, false, true, extensionTypeMethods); break;
                    case PartialExpressionType.Indexer: SuggestTypeMembers((pe as PartialIndexer).Indexer.ReturnType, AccessorLevel.Unknown, false, false, true, extensionTypeMethods); break;
                    case PartialExpressionType.ArrayElement: SuggestTypeMembers((pe as PartialArrayElement).ElementType, AccessorLevel.Unknown, false, false, true, extensionTypeMethods); break;
                    case PartialExpressionType.MethodGroup: break;

                    case PartialExpressionType.This:
                        {
                            if (fc.Function != null && fc.Function.DeclaringType != null)
                                SuggestTypeMembers(fc.Function.DeclaringType, AccessorLevel.Private, fc.Function.IsStatic, false, true, extensionTypeMethods);
                        }
                        break;

                    case PartialExpressionType.Value:
                        {
                            SuggestTypeMembers((pe as PartialValue).Value.ReturnType, AccessorLevel.Unknown, false, false, true, extensionTypeMethods);

                            if (dte.ExpressionType == ExpressionType.Identifier)
                            {
                                var lastNamescope = Enumerable.Last(_context.NodePath).TypeOrNamespace;
                                if (lastNamescope != null)
                                {
                                    var identifier = dte as AstIdentifier;
                                    var pt = _context.Compiler.NameResolver.TryResolveMemberRecursive(lastNamescope, identifier, null);
                                    if (pt == null) pt = _context.Compiler.NameResolver.TryResolveUsingNamespace(lastNamescope, identifier, null);
                                    if (pt is PartialType) SuggestTypeMembers((pt as PartialType).Type, AccessorLevel.Public, true, true, true, extensionTypeMethods);
                                }
                            }
                        }
                        break;

                    default: return false;
                }
                return true;
            }

            return false;
        }

        HashSet<object> _added = new HashSet<object>();

        void SuggestNamespaceMembers(Namespace n, bool includeBlocks)
        {
            HashSet<string> addedTypes = new HashSet<string>();

            if (includeBlocks)
            {
                foreach (var b in n.Blocks)
                    Suggest(SuggestItemType.Block, b, b.Name);
            }

            foreach (var t in n.Types)
            {
                if (addedTypes.Contains(t.Name)) continue;
                addedTypes.Add(t.Name);

                if (t is RefArrayType) continue;
                if (t is DelegateType)
                {
                    Suggest(SuggestItemType.Delegate, t, t.Name);
                }
                else if (t is ClassType)
                {
                    Suggest(SuggestItemType.Class, t, t.Name);
                }
                else if (t is StructType)
                {
                    Suggest(SuggestItemType.Struct, t, t.Name);
                }
                else if (t is InterfaceType)
                {
                    Suggest(SuggestItemType.Interface, t, t.Name);
                }
                else if (t is EnumType)
                {
                    Suggest(SuggestItemType.Enum, t, t.Name);
                }
            }

            foreach (var nk in n.Namespaces)
                Suggest(SuggestItemType.Namespace, nk, nk.Name);
        }


        enum AccessorLevel
        {
            Unknown,
            Private,
            Protected,
            Public
        }

        bool CompatibleAccessor(AccessorLevel level, Member m)
        {
            return CompatibleAccessor(level, m.Modifiers);
        }

        bool CompatibleAccessor(AccessorLevel level, Modifiers m)
        {
            switch (level)
            {
                case AccessorLevel.Public: return m.HasFlag(Modifiers.Public);
                case AccessorLevel.Protected: return m.HasFlag(Modifiers.Public) | m.HasFlag(Modifiers.Protected);
            }

            return true;
        }

        void SuggestTypeMembers(DataType dt, AccessorLevel accessorLevel, bool staticContext, bool metaContext, bool recurseToBase, List<Method> extensionTypeMethods)
        {
            // Suggest base type members
            if (recurseToBase && dt.Base != null)
                SuggestTypeMembers(dt.Base, accessorLevel == AccessorLevel.Public ? AccessorLevel.Public : AccessorLevel.Protected, staticContext, metaContext, recurseToBase, extensionTypeMethods);

            SuggestInterfaceType(dt, staticContext, metaContext, recurseToBase, extensionTypeMethods);

            SuggestEnumsInStaticContext(dt, staticContext);

            var ct = dt as ClassType;
            dt.PopulateMembers();

            if (metaContext && ct != null && ct.Block != null)
                SuggestBlockItems(ct.Block);

            SuggestSwizzlerType(dt);

            SuggestField(dt, accessorLevel, staticContext);

            SuggestEvents(dt, accessorLevel, staticContext);

            SuggestMethods(dt, accessorLevel, staticContext);

            SuggestProperties(dt, accessorLevel, staticContext);

            SuggestLiterals(dt, accessorLevel);

            SuggestExtensionTypeMethods(extensionTypeMethods, ct);

            SuggestStaticContext(dt, staticContext);

	        SuggestHasFlagForEnums(dt);
        }

	    void SuggestInterfaceType(
			DataType dt,
			bool staticContext,
			bool metaContext,
			bool recurseToBase,
			List<Method> extensionTypeMethods)
        {
            if (recurseToBase && dt is InterfaceType)
            {
                var it = dt as InterfaceType;
                foreach (var bit in it.Interfaces)
                    SuggestTypeMembers(
						bit,
						AccessorLevel.Public,
						staticContext,
						metaContext,
						recurseToBase,
						extensionTypeMethods);
            }
        }

        void SuggestEnumsInStaticContext(DataType dt, bool staticContext)
        {
            if (dt is EnumType && staticContext)
            {
                var et = dt as EnumType;
                foreach (var v in et.Literals) Suggest(SuggestItemType.EnumValue, v, v.Name);
            }
        }

        void SuggestStaticContext(DataType dt, bool staticContext)
        {
            if (!staticContext) return;

            foreach (var innertype in dt.NestedTypes)
            {
                if (innertype.Modifiers.HasFlag(Modifiers.Generated)) continue;
                if ((_context.TypeOrNamespace is DataType) &&
                    !IsMemberAccessible(innertype.Modifiers, dt, (_context.TypeOrNamespace as DataType))) continue;
                var it = SuggestItemType.Class;
                if (innertype is EnumType) it = SuggestItemType.Enum;
                if (innertype is DelegateType) it = SuggestItemType.Delegate;
                if (innertype is ClassType)
                {
                    switch (innertype.TypeType)
                    {
                        case TypeType.Class:
                            it = SuggestItemType.Class;
                            break;
                        case TypeType.Struct:
                            it = SuggestItemType.Struct;
                            break;
                        case TypeType.Interface:
                            it = SuggestItemType.Interface;
                            break;
                    }
                }
                Suggest(it, innertype, innertype.Name);
            }
        }

        void SuggestExtensionTypeMethods(IEnumerable<Method> extensionTypeMethods, ClassType ct)
        {
            if (extensionTypeMethods == null) return;
            foreach (var extTypeMethod in extensionTypeMethods.Where(
				extTypeMethod =>
					extTypeMethod.Parameters.Length > 0 &&
					extTypeMethod.Parameters[0].Type == ct))
            {
                Suggest(SuggestItemType.Method, extTypeMethod, extTypeMethod.Name);
            }
        }

        void SuggestLiterals(DataType dt, AccessorLevel accessorLevel)
        {
            foreach (var c in dt.Literals)
            {
                if (!CompatibleAccessor(accessorLevel, c.Modifiers)) continue;
                if ((_context.TypeOrNamespace is DataType) &&
                    !IsMemberAccessible(c, (_context.TypeOrNamespace as DataType))) continue;
                if (_added.Contains(c.Name)) continue;
                _added.Add(c.Name);
                Suggest(SuggestItemType.Constant, c, c.Name);
            }
        }

        void SuggestProperties(DataType dt, AccessorLevel accessorLevel, bool staticContext)
        {
            foreach (var member in dt.Properties)
            {
                if (!CompatibleAccessor(accessorLevel, member)) continue;
                if (member.IsStatic != staticContext) continue;
                if (!(dt is ArrayType) && member.Modifiers.HasFlag(Modifiers.Generated)) continue;
                if ((_context.TypeOrNamespace is DataType) &&
                    !IsMemberAccessible(member, (_context.TypeOrNamespace as DataType))) continue;
                if (_added.Contains(member)) continue;
                _added.Add(member.OverriddenProperty);

                var mods = new List<string>();
                if (member.SetMethod == null) mods.Add("readonly"); //Hackish but want to display readonly for properties too

                Suggest(SuggestItemType.Property, member, member.Name, null, null, () => member.ReturnType.ToString(), mods.ToArray(), null, null, null, SuggestItemPriority.Normal);
            }
        }

		string HashMethod(string name, MethodArgument[] args){
			var build = new StringBuilder ();
			build.Append (name);
			args.Each (arg => {
				build.Append (arg.IsOut.ToString());
				build.Append (arg.Name);
				build.Append (arg.ArgType);
			});
			return build.ToString ();
		}

		MethodArgument[] GetMethodArgsFromMethod(Method m)
		{
			return m.Parameters.Select (arg =>
				new MethodArgument (arg.Name, arg.Type.ToString (), arg.Modifier == ParameterModifier.Out)
			).ToArray();
		}

        void SuggestMethods(DataType dt, AccessorLevel accessorLevel, bool staticContext)
        {
            foreach (var member in dt.Methods)
            {
                if (!CompatibleAccessor(accessorLevel, member)) continue;
                if (member.IsStatic != staticContext) continue;
                if (member.Modifiers.HasFlag(Modifiers.Generated)) continue;
                if ((_context.TypeOrNamespace is DataType) &&
					!IsMemberAccessible(member, (_context.TypeOrNamespace as DataType))) continue;
				var args = GetMethodArgsFromMethod (member);
				var hash = HashMethod (member.Name, args);
                if (_added.Contains(hash)) continue;
				if (member.OverriddenMethod != null) {
					var om = member.OverriddenMethod;
					_added.Add (HashMethod (om.Name, GetMethodArgsFromMethod (om)));
				}

                var priority = SuggestItemPriority.Normal;

                var accessMods = new List<string>();
                foreach (string mod in member.Modifiers.ToString().ToLower().Split(", "))
                {
                    accessMods.Add(mod);
                }

                Suggest(SuggestItemType.Method, member, member.Name, null, null, () => member.ReturnType.ToString(), accessMods.ToArray(), null, args, null, priority, false);
				_added.Add (hash);
            }
        }

        void SuggestEvents(DataType dt, AccessorLevel accessorLevel, bool staticContext)
        {
            foreach (var member in dt.Events)
            {
                if (!CompatibleAccessor(accessorLevel, member)) continue;
                if (member.IsStatic != staticContext) continue;
                if (member.Modifiers.HasFlag(Modifiers.Generated)) continue;
                if ((_context.TypeOrNamespace is DataType) &&
                    !IsMemberAccessible(member, (_context.TypeOrNamespace as DataType))) continue;
                var it = SuggestItemType.Field;
                it = SuggestItemType.Event;
                Suggest(it, member, member.Name);
            }
        }

        void SuggestField(DataType dt, AccessorLevel accessorLevel, bool staticContext)
        {
            foreach (var member in dt.Fields)
            {
                if (!CompatibleAccessor(accessorLevel, member)) continue;
                if (member.IsStatic != staticContext) continue;
                if (member.Modifiers.HasFlag(Modifiers.Generated)) continue;
                if ((_context.TypeOrNamespace is DataType) &&
                    !IsMemberAccessible(member, (_context.TypeOrNamespace as DataType))) continue;
                var it = SuggestItemType.Field;
                var accessMods = new List<string>();
                var fieldMods = new List<string>();
                foreach (var s in member.Modifiers.ToString().ToLower().Split(", ")) accessMods.Add(s);
                if (member.FieldModifiers.HasFlag(FieldModifiers.ReadOnly)) fieldMods.Add("readonly");
                if (member.FieldModifiers.HasFlag(FieldModifiers.Const)) fieldMods.Add("const");
                var priority = SuggestItemPriority.Normal;
                Suggest(it, member, member.Name, null, null, () => member.ReturnType.ToString(), accessMods.ToArray(), null, null, null, priority);
            }
        }

        void SuggestSwizzlerType(DataType dt)
        {
            if (dt.Fields.Count <= 4 && (
				IsOfSwizzlerType("Float", dt.QualifiedName) ||
                IsOfSwizzlerType("Int", dt.QualifiedName) ||
				IsOfSwizzlerType("UInt", dt.QualifiedName) ||
                IsOfSwizzlerType("Short", dt.QualifiedName) ||
                IsOfSwizzlerType("UShort", dt.QualifiedName) ||
                IsOfSwizzlerType("Byte", dt.QualifiedName) ||
                IsOfSwizzlerType("SByte", dt.QualifiedName)))
            {
                var nameTemplate = dt.Name.Substring(0, dt.Name.Length - 1);
                for (var i = 2; i <= dt.Fields.Count; ++i)
                {
                    var dummyDt = new StructType(dt.Source, dt.Parent, null, dt.Modifiers, nameTemplate + i);
                    SuggestSwizzlerTypes(dt, dummyDt, "", 0, i, SuggestItemPriority.Normal);
                }
            }
        }

        void SuggestTerminals()
        {
            SuggestBlockItems(_compiler.BlockBuilder.TerminalProperties, "(terminal property)");
        }

        void SuggestBlockItems(Block b, string fixedSourceString = null)
        {
            foreach (var k in b.NestedBlocks)
            {
                Suggest(SuggestItemType.Block, k, k.Name);
            }

            for (int i = b.Members.Count-1; i >= 0; i--)
            {
                var bi = b.Members[i];
                switch (bi.Type)
                {
                    case BlockMemberType.Node: break;
                    case BlockMemberType.Apply: SuggestBlockItems((bi as Apply).Block, fixedSourceString); break;
                    case BlockMemberType.MetaProperty:
                        {
                            var mp = (bi as MetaProperty);
                            if (_added.Contains(mp.Name)) continue;

                            foreach (var s in _added)
                            {
                                if (s is Property && (s as Property).Name == mp.Name) { _added.Add(mp.Name); }
                                if (s is Field && (s as Field).Name == mp.Name) { _added.Add(mp.Name); }
                                if (s is Literal && (s as Literal).Name == mp.Name) { _added.Add(mp.Name); }
                            }
                            if (_added.Contains(mp)) continue;
                            _added.Add(mp.Name);

                            Suggest(SuggestItemType.MetaProperty, mp, mp.Name);
                        }
                        break;
                }
            }
        }

        bool IsOfSwizzlerType(string type, string qualifiedName)
        {
            string template = "Uno." + type;
            if(qualifiedName.Substring(0, qualifiedName.Length - 1) != template)
                return false;

            for(int i = 2;i <= 4;++i)
            {
                if(qualifiedName == template + i.ToString())
                {
                    return true;
                }
            }

            return false;
        }

        void SuggestSwizzlerTypes(DataType dt, DataType dummyDt, string value, int depth, int maxDepth, SuggestItemPriority priority)
        {
            if(depth == maxDepth)
            {
                Suggest(SuggestItemType.Field, dummyDt, value, null,null, () => dt.ToString(), null, null, null, null, priority);
                return;
            }

            foreach(var c in dt.Fields)
            {
                SuggestSwizzlerTypes(dt, dummyDt, value + c.Name, depth + 1, maxDepth, priority);
            }
        }

	    void SuggestHasFlagForEnums(DataType dt)
	    {
		    if (dt.TypeType != TypeType.Enum)
			    return;

			Suggest(SuggestItemType.Method, dt, "HasFlag");
	    }
    }
}
