using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using System.Xml.Linq;
using Outracks.Simulator;
using Outracks.Simulator.Protocol;
using Outracks.Simulator.UXIL;

namespace Outracks.Fuse.Live
{
	partial class LiveElement
	{
		public async Task Replace(Func<SourceFragment, System.Threading.Tasks.Task<SourceFragment>> transform)
		{
			var newElement = (await transform(await Copy())).ToXml();
			if (Element.Parent != null)
			{
				Element.ReplaceWith(newElement);
			}
			_invalidated.OnNext(Unit.Default);
			UpdateFrom(newElement);
		}

		public void UpdateFrom(XElement newElement)
		{
			var oldElement = Element;
			UpdateFrom(oldElement, newElement);
			Element = newElement;
		}

		void UpdateFrom(XElement oldElement, XElement newElement)
		{
			Name.Write(newElement.Name.LocalName);
			_textPosition.OnNext(new SourceReference(_textPosition.Value.File, newElement.TryGetTextPosition()));
			UpdateAttributes(oldElement, newElement);
			UpdateValue(oldElement, newElement);
			UpdateChildren(newElement);
		}

		public int UpdateElementIds(string path, int startIndex, IDictionary<ObjectIdentifier, IElement> dict)
		{
			var id = new ObjectIdentifier(path, startIndex);
			dict[id] = this;
			_elementId.OnNextDistinct(id);

			startIndex++;

			foreach (var child in _children.Value)
			{
				startIndex = child.UpdateElementIds(path, startIndex, dict);
			}

			return startIndex;
		}

		void UpdateAttributes(XElement oldElement, XElement newElement)
		{
			var prev = oldElement.Attributes().ToArray();
			var next = newElement.Attributes().ToArray();

			var prevSet = oldElement.Attributes().ToDictionary(k => k.Name, k => k);
			var nextSet = newElement.Attributes().ToDictionary(k => k.Name, k => k);

			foreach (var prevAttr in prev)
			{
				var name = prevAttr.Name;
				XAttribute newAttr;
				if (!nextSet.TryGetValue(name, out newAttr))
					this[NameToKey(name)].Write(Optional.None());
				else if (newAttr.Value != prevAttr.Value)
					this[NameToKey(name)].Write(newAttr.Value);
			}

			foreach (var newAttr in next)
			{
				var name = newAttr.Name;
				if (!prevSet.ContainsKey(name))
					this[NameToKey(name)].Write(newAttr.Value);
			}
		}

		void UpdateValue(XElement oldElement, XElement newElement)
		{
			if (newElement.HasElements || string.IsNullOrWhiteSpace(newElement.Value))
			{
				if (oldElement.HasElements || oldElement.Value == newElement.Value)
					return;

				Content.Write(Optional.None());
			}
			else
			{
				if (!oldElement.HasElements && oldElement.Value == newElement.Value)
					return;

				Content.Write(newElement.Value.ToOptional());
			}
		}

		void UpdateChildren(XElement newElement)
		{
			var oldChildren = _children.Value;

			var newChildren = newElement.Elements().ToList();
			if (oldChildren.Count == 0 && newChildren.Count == 0)
				return;

			var builder = new LiveElement[newChildren.Count];

			// We'll be using newChildren as a worklist, so let's take a note of the position of all the new children
			var newChildrenLocation = newElement.Elements()
				.Select((e, i) => Tuple.Create(e, i))
				.ToDictionary(t => t.Item1, t => t.Item2);


			var needsReify = newChildren.Count != oldChildren.Count;

			// Incrementally (eagerly) find a new child with same element type
			// Performance should be "okay" since we're scanning both lists from the start and removing children from the worklist as we find them
			foreach (var oldChildImpl in oldChildren)
			{
				foreach (var newChild in newChildren)
				{
					// If one of the new XElement children we got has the same name as one of our IElements
					if (oldChildImpl.Element.Name.LocalName == newChild.Name.LocalName)
					{
						// Update the old IElement
						builder[newChildrenLocation[newChild]] = oldChildImpl;
						oldChildImpl.UpdateFrom(newChild);

						// Remove the new child from the worklist and stop iterating over it
						newChildren.Remove(newChild);
						// Breaking the loop here is important both for correctness and to avoid iterating over a changed collection
						break;
					}

					// elements will be removed
					needsReify = true;
				}
			}

			needsReify |= newChildren.Any();

			// So, we've reused all the elements we can reuse, but we still might have some left in our newChildren worklist
			foreach (var newChild in newChildren)
			{
				// Fortunately we know where they go
				var index = newChildrenLocation[newChild];

				// Since we're iterating through them from start to end the index should not be out of bounds
				var childElement = CreateChildElement(SourceFragment.FromString("<" + newChild.Name.LocalName + "/>"));

				childElement.UpdateFrom(newChild);

				builder[index] = childElement;
			}

			if (needsReify)
			{
				_children.OnNext(ImmutableList.Create(builder));
				_mutations.OnNext(new ReifyRequired());
			}
		}

		void UpdateXml(Action<XElement> action)
		{
			action(Element);
			_invalidated.OnNext(Unit.Default);
		}

		void UpdateChildren(Func<IImmutableList<LiveElement>, IImmutableList<LiveElement>> children)
		{
			_children.OnNext(children(_children.Value));
			_mutations.OnNext(new ReifyRequired());
			_invalidated.OnNext(Unit.Default);
		}
	}

}
