using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Outracks.IO;

namespace Outracks.Fuse.Testing
{
	public class RandomUxMutator
	{
		private readonly Random _rng;
		private readonly int _maxElements = 200;

		private readonly string[] _colors =
			{ "#c66029", "#e27043", "#ed830c", "#eba41e", "#eede83", "#91e11c", "#53bfdc", "#3963d3", "#e76b8f" };

		public RandomUxMutator()
		{
			_rng = new Random();
		}

		public static int Depth(XElement element)
		{
			return element.Parent == null ? 0 : Depth(element.Parent) + 1;
		}

		public void Mutate(AbsoluteFilePath file, int iterations)
		{
			var element = SourceFragment.FromString(File.ReadAllText(file.NativePath)).ToXml();
			for (int i = 0; i < iterations; i++)
			{
				Mutate(element);
			}
			File.WriteAllText(file.NativePath, SourceFragment.FromXml(element).ToString());
		}

		public void Mutate(XElement element)
		{
			var descendants = element.Descendants().ToList();
			if ((descendants.Count < _maxElements / 2 || _rng.NextDouble() > descendants.Count / (double) _maxElements) &&
				descendants.Count < _maxElements)
			{
				var containers = descendants.Count > 0 ? descendants.Where(x => x.Name.LocalName == "Grid") : new[] { element };
				var container = RandomItem(containers);
				var existingChildCount = container.Elements().Count();
				var insertPoint = _rng.Next(0, existingChildCount + 1);

				var insertElement = SourceFragment.FromString(
						String.Format(
							"<Grid ChildOrder=\"{0}\" Background=\"{1}\" />",
							Depth(container) % 2 == 0 ? "RowMajor" : "ColumnMajor",
							RandomItem(_colors)))
					.ToXml();

				if (insertPoint == existingChildCount)
				{
					container.Add(insertElement);
				}
				else
				{
					container.Elements().ElementAt(insertPoint).AddBeforeSelf(insertElement);
				}
			}
			else
			{
				RandomItem(descendants).Remove();
			}
		}

		private T RandomItem<T>(IEnumerable<T> items)
		{
			var list = items as IList<T> ?? items.ToArray();
			if (list.Count == 0)
				throw new InvalidOperationException("Need at least one item");

			return list[_rng.Next(0, list.Count)];
		}
	}
}