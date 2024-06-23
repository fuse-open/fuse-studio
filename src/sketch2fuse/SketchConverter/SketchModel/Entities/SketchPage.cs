using System;
using System.Collections.Generic;

namespace SketchConverter.SketchModel
{
	public class SketchPage : SketchEntity
	{
		public readonly SketchRect Frame;
		public readonly string Name;
		public readonly IReadOnlyList<SketchLayer> Layers;

		public SketchPage(Guid id, SketchRect frame, string name, IReadOnlyList<SketchLayer> layers)
			: base(id)
		{
			Frame = frame;
			Name = name;
			Layers = layers;
		}
	}
}
