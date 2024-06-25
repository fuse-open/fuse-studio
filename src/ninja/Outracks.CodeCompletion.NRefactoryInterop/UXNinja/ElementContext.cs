using System.Collections.Generic;
using System.Linq;
using ICSharpCode.NRefactory.Xml;
using Outracks.UnoDevelop.UXNinja;
using Uno;
using Uno.Compiler;

namespace Outracks.CodeCompletionFactory.UXNinja
{
	public class ElementContext : IElementContext
	{
		readonly SourceFile _file;
		readonly AXmlElement _element;
		readonly ElementContext _parent;
		readonly ElementContext []_children;
		readonly IAttributeContext[] _attributes;

		public string NamespacePrefix
		{
			get { return _element.Prefix; }
		}

		public string Name
		{
			get { return _element.LocalName; }
		}

		public IEnumerable<IAttributeContext> Attributes
		{
			get
			{
				return _attributes;
			}
		}

		public Source StartTagSource
		{
			get { return _element.StartTag == null ? Source.Unknown : _file.GetSourceFromOffsets(_element.StartTag.StartOffset, _element.StartTag.EndOffset); }
		}

		public Source EndTagSource
		{
			get { return _element.EndTag == null ? Source.Unknown : _file.GetSourceFromOffsets(_element.EndTag.StartOffset, _element.EndTag.EndOffset); }
		}

		public ElementContext(SourceFile file, AXmlElement element, ElementContext parent)
		{
			_file = file;
			_element = element;
			_parent = parent;
			_children = _element.Children
				.OfType<AXmlElement>()
				.Select(elm => new ElementContext(_file, elm, this)).ToArray();

			_attributes = _element.Attributes.Select(a => new Attribute(_file, a)).ToArray();
		}

		public IElementContext Parent
		{
			get { return _parent; }
		}

		public IEnumerable<IElementContext> Children
		{
			get { return _children; }
		}

		public T GetBackingStore<T>() where T : class
		{
			return _element as T;
		}
	}
}