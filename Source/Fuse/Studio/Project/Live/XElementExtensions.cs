using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Outracks.Fuse.Live
{
	internal static class XElementExtensions
	{
		public const string DefaultIndentStep = "\t";

		public static bool TryGetElementIndent(this XNode element, out string lineIndent)
		{
			if (element.Parent == null)
			{
				lineIndent = string.Empty;
				return true;
			}
			var prevNode = element.PreviousNode as XText;
			var nextNode = element.NextNode as XText;
			if (prevNode != null && nextNode != null)
			{
				var isLastElementOnLine = nextNode.Value.Contains("\n");
				var lastNewLineIndex = prevNode.Value.LastIndexOf('\n');
				if (isLastElementOnLine || lastNewLineIndex != -1)
				{
					var lineStartIndex = lastNewLineIndex + 1;
					lineIndent = prevNode.Value.Substring(lineStartIndex);
					return lineIndent.All(char.IsWhiteSpace);
				}
			}
			lineIndent = null;
			return false;
		}

		public static void AddBeforeSelfIndented(this XNode self, XElement addedElement)
		{
			string selfIndent;
			if (TryGetElementIndent(self, out selfIndent))
			{
				IndentDescendantNodes(addedElement, selfIndent);
				var newLineAndIndent = new XText("\n" + selfIndent);
				self.AddBeforeSelf(newLineAndIndent);
				newLineAndIndent.AddBeforeSelf(addedElement);
			}
			else
			{
				// Ignore indent
				self.AddBeforeSelf(addedElement);
			}
		}

		static void IndentDescendantNodes(XElement addedElement, string selfIndent)
		{
			foreach (var text in addedElement.DescendantNodes().OfType<XText>())
			{
				text.Value = text.Value.Replace("\n", "\n" + selfIndent);
			}
		}


		public static void AddAfterSelfIndented(this XNode self, XElement addedElement)
		{
			string selfIndent;
			if (TryGetElementIndent(self, out selfIndent))
			{
				IndentDescendantNodes(addedElement, selfIndent);
				var newLineAndIndent = new XText("\n" + selfIndent);
				self.AddAfterSelf(newLineAndIndent);
				newLineAndIndent.AddAfterSelf(addedElement);
			}
			else
			{
				// Ignore indent
				self.AddAfterSelf(addedElement);
			}
		}

		public static void AddIndented(this XElement self, XElement child)
		{
			var nodeBefore = self
				.Nodes()
				.LastOrDefault(n => !(n is XText && string.IsNullOrWhiteSpace(((XText)n).Value)));

			if (nodeBefore != null)
			{
				nodeBefore.AddAfterSelfIndented(child);
				return;
			}

			string selfIndent;

			if (TryGetElementIndent(self, out selfIndent) && self.Value.All(c => char.IsWhiteSpace(c) || c == '\n'))
			{
				// Remove all existing nodes, known to be whitespace (due to check above)
				// This is to make stuff less complicated, only have to worry about one case
				foreach (var whitespaceNode in self.Nodes().ToArray())
				{
					whitespaceNode.Remove();
				}
				string indentStep = DefaultIndentStep;
				if (self.Parent != null)
				{
					string parentIndent;
					if (self.Parent.TryGetElementIndent(out parentIndent) && selfIndent.StartsWith(parentIndent))
					{
						indentStep = selfIndent.Substring(parentIndent.Length);
					}
				}
				else if (selfIndent.Length > 0)
				{
					indentStep = selfIndent;
				}

				var childIndent = selfIndent + indentStep;
				IndentDescendantNodes(child, childIndent);

				self.Add(new XText("\n" + childIndent));
				self.Add(child);
				self.Add(new XText("\n" + selfIndent));

				return;
			}

			self.Add(child);
		}

		public static void RemoveIndented(this XElement self)
		{
			var prevNode = self.PreviousNode as XText;
			var nextNode = self.NextNode as XText;
			if (prevNode != null &&
				nextNode != null &&
				prevNode.Value.Contains('\n') &&
				nextNode.Value.Contains('\n'))
			{
				// If self is the sole element on a line with only whitespace, remove the whole line
				prevNode.Value = prevNode.Value.TrimEnd('\t', ' ');
				nextNode.Value = string.Join("\n", nextNode.Value.Split(new [] {'\n'}, StringSplitOptions.None).Skip(1));
				if (nextNode.Value == string.Empty)
					nextNode.Remove();
			}

			var parent = self.Parent;
			self.Remove();
			if (parent != null && parent.Nodes().All(n => n is XText) && parent.Value.All(c => "\n\t ".Contains(c)))
			{
				parent.RemoveNodes();
			}
		}
	}
}