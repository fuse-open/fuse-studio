using ICSharpCode.NRefactory.Xml;
using Outracks.IO;
using Uno.Compiler;
using Outracks.UnoDevelop.UXNinja;
using Uno;

namespace Outracks.CodeCompletionFactory.UXNinja
{
	public class Attribute : IAttributeContext
	{
		readonly SourceFile _file;
		readonly AXmlAttribute _attribute;

		public string Name 
		{
			get { return _attribute.LocalName; }			
		}

		public string NamespacePrefix
		{
			get
			{
				var name = _attribute.Name;
				var idxOfDelimeter = name.IndexOf(":", System.StringComparison.Ordinal);

				return idxOfDelimeter > 0 ? name.Substring(0, idxOfDelimeter) : "";
			}
		}

		public string Value
		{
			get { return _attribute.Value; }
		}

		public string FullName
		{
			get { return _attribute.Name; }
		}

		public Source Source
		{
			get { return _file.GetSourceFromOffsets(_attribute.StartOffset, _attribute.EndOffset); }
		}

		public bool IsNamespaceDeclaration
		{
			get { return _attribute.IsNamespaceDeclaration; }
		}

		public T GetBackingStore<T>() where T : class
		{
			return _attribute as T;
		}

		public Attribute(SourceFile file, AXmlAttribute attribute)
		{
			_file = file;
			_attribute = attribute;
		}
	}
}