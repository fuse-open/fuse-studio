using System;
using System.Collections.Generic;

namespace Outracks.UnoDevelop.CodeNinja.AmbientParser
{
    public enum NodeType
    {
        Root,
        Using,
        UsingStatic,
        Namespace,
        Class,
        Block,
        Struct,
        Interface,
        Enum,
        Delegate,
        Field,
        Method,
        Apply,
        Constructor,
        Operator,
        Property,
        GetScope,
        SetScope,
        Indexer,
        MetaProperty,
        MetaPropertyDefinition,
        MetaPropertyDefinitionScope,
        DrawStatement,
        InlineBlock,
        Catch,
        Event,
        AddScope,
        RemoveScope,
    }

    public static class NodeTypeHelpers
    {
        public static bool IsMethodNode (this NodeType nt)
        {
            switch (nt)
            {
                case NodeType.Root: 
                case NodeType.Using:
                case NodeType.UsingStatic:
                case NodeType.Namespace:
                case NodeType.Class:
                case NodeType.Block:
                case NodeType.Struct:
                case NodeType.Enum:
                case NodeType.Delegate:
                case NodeType.Field:
                case NodeType.Apply:
                case NodeType.Indexer:
                case NodeType.Property:
                case NodeType.MetaProperty:
                case NodeType.MetaPropertyDefinition:
                case NodeType.DrawStatement:
                case NodeType.InlineBlock:
                case NodeType.Catch:
                case NodeType.Event:
                    return false;

                case NodeType.Method:
                case NodeType.Constructor:
                case NodeType.Operator:
                case NodeType.GetScope:
                case NodeType.SetScope:
                case NodeType.AddScope:
                case NodeType.RemoveScope:
                case NodeType.MetaPropertyDefinitionScope:
                    return true;
                    
                default:
                    throw new Exception("Unknown node type");
            }
        }
    }

    public class Node
    {
        public int StartOffset, EndOffset;
        public NodeType Type;

        public Uno.Compiler.API.Domain.IL.Namescope TypeOrNamespace;
        public Uno.Compiler.API.Domain.IL.Members.Member Member;
        public Uno.Compiler.API.Domain.Graphics.BlockBase BlockBase;
        public Uno.Compiler.API.Domain.Graphics.Apply ApplyItem;
        public Uno.Compiler.API.Domain.Graphics.MetaProperty MetaProperty;

        List<Node> _children;

        public void AddChild(Node n)
        {
            if (_children == null) _children = new List<Node>();
            _children.Add(n);
        }

        public IEnumerable<Node> Children
        {
            get
            {
                return _children;
            }
        }

        string _name;
        public string Name
        {
            get { return _name; }
        }
        public string BaseClassName;

        public string DrawStatementBlockList;

        public Node(int startOffset, int endOffset, NodeType type, string name)
        {
            StartOffset = startOffset;
            EndOffset = endOffset;
            Type = type;
            _name = name;
        }

        public override string ToString()
        {
            return Type.ToString().ToLower() + " " + Name;
        }
    }

}
