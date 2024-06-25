using System;
using System.Collections.Generic;

namespace SketchConverter.SketchModel
{
	public class SketchDocument : SketchEntity
	{
		public readonly IReadOnlyList<SketchPage> Pages;

		public SketchDocument(Guid id, IReadOnlyList<SketchPage> pages) : base(id)
		{
			Pages = pages;
		}
	}
}
