using System.Collections.Generic;
using System.Linq;
using ICSharpCode.NRefactory.Editor;
using ICSharpCode.NRefactory.Xml;
using Outracks.IO;
using Outracks.UnoDevelop.UXNinja;
using Uno;
using Uno.Compiler;

namespace Outracks.CodeCompletionFactory.UXNinja
{
	public class Context : IContext
	{
	    readonly SourceFile _file;
		readonly AXmlDocument _document;
		readonly int _caret;
		readonly IElementContext _root;
		readonly IDictionary<string, string> _namespaceDeclarations = new Dictionary<string, string>();
		readonly IElementContext _targetElement;

		public ContextType Type
		{
			get { return FindContextType(); }
		}

		ContextType FindContextType()
		{
			var lastElement = _targetElement;
			if (lastElement == null)
				return ContextType.None;

			var startTag = lastElement.GetBackingStore<AXmlElement>().StartTag;
			if (lastElement.Attributes.Any())
			{
				if (lastElement.Attributes.Any(attrib => attrib.GetBackingStore<AXmlAttribute>().ValueSegment.IsOffsetInsideSegment(_caret)))
				{
					return ContextType.StartTagAttributeValue;
				}
			}
			else
			{
				if (startTag.NameSegment.EndOffset >= _caret)
					return ContextType.StartTagName;
			}

			return ContextType.StartTagAttributeName;
		}

		public Source SourceLocation
		{
			get { return _file.GetSourceFromOffsets(_document.StartOffset, _document.EndOffset); }
		}

		public IDictionary<string, string> NamespaceDeclarations
		{
			get { return _namespaceDeclarations; }
		}

		public IElementContext TargetElement
		{
			get { return _targetElement; }
		}

		IElementContext FindTargetElement()
		{
			var elements = _root.TraverseSelfAndAllChildren();
			foreach (var elm in elements)
			{
				if (elm.StartTagSource != Source.Unknown
					&& elm.StartTagSource.Offset <= _caret
					&& elm.StartTagSource.EndOffset >= _caret)
				{
					return elm;
				}
			}

			return null;
		}

		Context(SourceFile file, AXmlDocument document, int caret)
		{
			_file = file;
			_document = document;
			_caret = caret;
			var root = document.Children.OfType<AXmlElement>().FirstOrDefault();
			_root = root == null ? null : new ElementContext(file, root, null);
			_targetElement = _root == null ? null : FindTargetElement();
			InitNamespaces();
		}

		public static Context CreateContext(AbsoluteFilePath filePath, string code, int offset, SourcePackage package)
		{
			var parser = new AXmlParser();
			var result = parser.Parse(new StringTextSource(code));
            var file = new SourceFile(package, filePath.NativePath, code);
			return new Context(file, result, offset);
		}

		void InitNamespaces()
		{
			if (_targetElement == null)
				return;

			var elements = _targetElement.TraverseAncestors();
			foreach (var element in elements)
			{
				var nsDeclarations = element.Attributes.Where(x => x.IsNamespaceDeclaration);
				nsDeclarations.Each(ns => _namespaceDeclarations.Add(ns.Value, ns.Name));
			}
		}

		public IElementContext Root
		{
			get { return _root; }
		}
	}
}