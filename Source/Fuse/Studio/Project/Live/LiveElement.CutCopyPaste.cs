using System.Collections.Immutable;
using System.Text;
using System.Threading.Tasks;
using Uno.Collections;

namespace Outracks.Fuse.Live
{
	partial class LiveElement 
	{
		public async Task<SourceFragment> Cut()
		{
			var fragment = await Copy();

			if (!_parent.HasValue)
				throw new ElementIsRoot();

			UpdateXml(e => e.RemoveIndented());
			_parent.Value.UpdateChildren(c => c.Remove(this));

			return fragment;
		}

		public Task<SourceFragment> Copy()
		{
			var fragment = SourceFragment.FromXml(Element);
			string elementIndent;
			if (Element.TryGetElementIndent(out elementIndent))
			{
				fragment = RemoveIndentFromDescendantNodes(fragment, elementIndent);
			}
			return Task.FromResult(fragment);
		}

		static SourceFragment RemoveIndentFromDescendantNodes(SourceFragment fragment, string elementIndent)
		{
			// If all subsequent lines start with the indent specified, remove it
			StringBuilder stringBuilder = new StringBuilder();
			bool isFirst = true;
			bool indentFixSuccess = true;
			foreach (var line in fragment.ToString().Split('\n'))
			{
				if (isFirst)
				{
					isFirst = false;
					stringBuilder.Append(line);
				}
				else if (!line.StartsWith(elementIndent))
				{
					indentFixSuccess = false;
					break;
				}
				else
				{
					stringBuilder.Append('\n');
					stringBuilder.Append(line.Substring(elementIndent.Length));
				}
			}
			return indentFixSuccess ? SourceFragment.FromString(stringBuilder.ToString()) : fragment;
		}

		public IElement Paste(SourceFragment fragment)
		{
			var child = CreateChildElement(fragment);

			UpdateXml(e => e.AddIndented(child.Element));
			UpdateChildren(c => c.Add(child));

			return child;
		}

		public IElement PasteBefore(SourceFragment fragment)
		{
			if (!_parent.HasValue)
				throw new ElementIsRoot();

			var sibling  = _parent.Value.CreateChildElement(fragment);

			UpdateXml(e => e.AddBeforeSelfIndented(sibling.Element));		
			_parent.Value.UpdateChildren(c => c.Insert(c.IndexOf(this), sibling));

			return sibling;
		}

		public IElement PasteAfter(SourceFragment fragment)
		{
			if (!_parent.HasValue)
				throw new ElementIsRoot();
		
			var sibling = _parent.Value.CreateChildElement(fragment);

			UpdateXml(e => e.AddAfterSelfIndented(sibling.Element));
			_parent.Value.UpdateChildren(c => c.Insert(c.IndexOf(this) + 1, sibling));

			return sibling;
		}

		LiveElement CreateChildElement(SourceFragment sourceFragment)
		{
			var elm = new LiveElement(parent: this);
			elm.UpdateFrom(sourceFragment.ToXml());
			return elm;
		}
	}
}