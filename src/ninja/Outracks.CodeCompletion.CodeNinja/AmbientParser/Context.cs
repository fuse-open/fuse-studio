using System.Collections.Generic;
using System.Linq;
using Uno;
using Uno.Compiler.API.Domain.Graphics;
using Uno.Compiler.API.Domain.IL;
using Uno.Compiler.API.Domain.IL.Members;
using Uno.Compiler.API.Domain.IL.Types;
using Uno.Compiler.Core;

namespace Outracks.UnoDevelop.CodeNinja.AmbientParser
{
    public class Context
    {
        public List<Node> NodePath { get; private set; }

        public Compiler Compiler;
        public Namespace Root;

        public bool expectsConstructor = false;



        public bool InTypeBody
        {
            get
            {
                if (NodePath.Count == 0) return false;

                var last = Enumerable.Last(NodePath);
                return
                    last.TypeOrNamespace != null && (
                    last.Type == NodeType.Class ||
                    last.Type == NodeType.Struct ||
                    last.Type == NodeType.Field);
            }
        }

        public Namescope TypeOrNamespace
        {
            get { return TypeOrNamespaces.FirstOrDefault(); }
        }

        public IEnumerable<Namescope> TypeOrNamespaces
        {
            get { return NodePath.Reverse<Node>().Select(n => n.TypeOrNamespace).NotNull(); }
        }

        public IEnumerable<BlockBase> BlockBasePath
        {
            get { return NodePath.Reverse<Node>().Select(n => n.BlockBase).NotNull(); }
        }

        public Node DrawStatementNode
        {
            get { return NodesOfType(NodeType.DrawStatement).FirstOrDefault(); }
        }

        public Node MetaPropertyNode
        {
            get { return NodesOfType(NodeType.MetaProperty).FirstOrDefault(); }
        }

        IEnumerable<Node> NodesOfType(NodeType type)
        {
            return NodePath.Reverse<Node>().Where(n => n.Type == type);
        }

        public Node InlineBlock
        {
            get
            {
                return NodePath.LastOrDefault(x => x.Type == NodeType.InlineBlock);
            }
        }

        public Node Block
        {
            get
            {
                return NodePath.LastOrDefault(x => x.Type == NodeType.Block);
            }
        }

        public BlockBase BlockBase
        {
            get
            {
                foreach (var n in NodePath.Reverse<Node>())
                {
                    if (n.BlockBase != null) return n.BlockBase;
                }
                return null;
            }
        }

        bool Visit(MetaProperty mp, Node n, int offset)
        {
            n.MetaProperty = mp;
            // Check if we are not inside the right node
            if (!(n.StartOffset < offset && n.EndOffset >= offset)) return false;

            NodePath.Add(n);
            if (n.Children != null)
            {
                foreach (var c in n.Children)
                {
                    if (Visit(mp, c, offset)) return false;
                }
            }

            return true;
        }

        bool Visit(BlockBase b, Node n, int offset)
        {
            n.BlockBase = b;
            // Check if we are not inside the right node
            if (!(n.StartOffset < offset && n.EndOffset >= offset)) return false;

            NodePath.Add(n);

            // Add metaproperties from lambda draw.
            var metaProperties = new List<MetaProperty>();
            foreach (var i in b.Members)
            {
                if (i.Type == BlockMemberType.Apply)
                {
                    var applyItem = i as Apply;
                    if (applyItem.Block.Parent == b)
                    {
                        foreach(var c in applyItem.Block.Members)
                        {
                            if(c.Type == BlockMemberType.MetaProperty)
                            {
                                metaProperties.Add(c as MetaProperty);
                            }
                        }
                    }
                }
            }

            if (n.Children != null)
                foreach (var c in n.Children)
                {
                    foreach (var db in b.EnumerateNestedScopes())
                    {
                        if (Visit(db, c, offset)) return true;
                    }

                    if (c.Type == NodeType.MetaProperty)
                    {
                        foreach (var m in metaProperties)
                        {
                            if(m.Name == c.Name)
                            {
                                if (Visit(m, c, offset)) return true;
                            }
                        }
                    }

                    foreach (var i in b.Members)
                    {
                        if (i.Type == BlockMemberType.Apply && c.Type == NodeType.Apply)
                        {
                            c.ApplyItem = i as Apply;
                            continue;
                        }

                        if (i.Type == BlockMemberType.MetaProperty && c.Type == NodeType.MetaProperty)
                        {
                            var mp = i as MetaProperty;
                            if (mp.Name == c.Name)
                            {
                                if (Visit(mp, c, offset)) return true;
                            }

                        }
                    }

                    if (Visit(b, c, offset)) return true;
                }

            return true;
        }

        bool Visit(Member m, Node n, int offset)
        {
            n.Member = m;
            // Check if we are not inside the right node
            if (!(n.StartOffset < offset && n.EndOffset >= offset)) return false;

            NodePath.Add(n);

            if (n.Children != null)
                foreach (var c in n.Children)
                {
                    if (m is Method)
                    {
                        var method = m as Method;

                        foreach (var db in method.DrawBlocks)
                        {
                            if (Visit(db, c, offset)) return true;
                        }
                    }

                    if (Visit(m, c, offset)) return true;
                }

            return true;
        }


        bool Visit(Namescope scope, Node n, int offset)
        {
            n.TypeOrNamespace = scope;
            if (scope is ClassType)
            {
                n.BlockBase = (scope as ClassType).Block;
            }

            if (scope is Block) return Visit(scope as Block, n, offset);

            var name = n.Name;
            if (n.Name != "<root>" && name.Contains('<')) name = name.Substring(0, name.IndexOf('<'));

            // Check if we are not inside the right node
            if (!(n.StartOffset < offset && n.EndOffset >= offset)) return false;

            NodePath.Add(n);

            if (n.Children != null)
            {
                foreach (var c in n.Children)
                {
                    // If current scope is a data type, check if a type member has a match with the child node, if so let them have precedence
                    if (scope is DataType)
                    {
                        var dt = scope as DataType;

                        if (c.Type == NodeType.Constructor)
                        {
                            foreach (var m in dt.Constructors)
                            {
                                if (m.Source.Offset == c.StartOffset)
                                {
                                    if (Visit(m, c, offset)) return true;
                                }
                            }
                        }
                        else
                        {
                            foreach (var m in dt.EnumerateMembers())
                            {
                                if (m.Name == c.Name)
                                {
                                    if (Visit(m, c, offset)) return true;
                                }
                            }
                        }
                    }

                    // Check if a child scope has a match with the child node, if so let them have precedence
                    foreach (var cn in scope.EnumerateNestedScopes())
                    {
                        if (cn.Name == c.Name)
                        {
                            if (Visit(cn, c, offset)) return true;
                        }
                    }
                    if (Visit(scope, c, offset)) return true;
                }
            }

            return true;
        }

        public IEnumerable<string> Usings
        {
            get
            {
                foreach (var n in NodePath)
                {
                    if ((n.Type == NodeType.Namespace || n.Type == NodeType.Root) && n.Children != null)
                    {
                        foreach (var u in n.Children)
                        {
                            if (u.Type == NodeType.Using) yield return u.Name;
                        }
                    }
                }
            }
        }

        public IEnumerable<string> UsingStatics
        {
            get
            {
                foreach (var n in NodePath)
                {
                    if ((n.Type == NodeType.Namespace || n.Type == NodeType.Root) && n.Children != null)
                    {
                        foreach (var u in n.Children)
                        {
                            if (u.Type == NodeType.UsingStatic) yield return u.Name;
                        }
                    }
                }
            }
        }

        public Source Source;

        public Context(Compiler compiler, Source src, ParseResult parseResult, int offset, int length)
        {
            if (compiler == null)
                return;

            Source = src;

            Compiler = compiler;
            Root = compiler.Data.IL;
            NodePath = new List<Node>();

            offset -= length - parseResult.CodeLengthAtParseTime;

            Visit(Root, parseResult.Root, offset);
        }
    }
}
