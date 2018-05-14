using System;
using System.Collections.Generic;

namespace SketchConverter.SketchModel
{
	public class SketchLayer : SketchEntity
	{
		public SketchLayer Parent { get; set; } //Can't be readonly due to circular dependency :(
		public readonly SketchRect Frame;
		public readonly string Name;
		public readonly bool NameIsFixed;
		public readonly SketchAlignment Alignment;
		public readonly double Rotation;
		public readonly bool IsFlippedVertical;
		public readonly bool IsFlippedHorizontal;
		public readonly IReadOnlyList<SketchLayer> Layers;
		public readonly Optional<SketchStyle> Style;
		
		public SketchLayer(SketchLayer layer)
			: this(layer.Id,
				layer.Parent,
				layer.Frame,
				layer.Name,
				layer.NameIsFixed,
				layer.Alignment,
				layer.Rotation,
				layer.IsFlippedVertical,
				layer.IsFlippedHorizontal,
				layer.Style,
				layer.Layers)
		{
		}

		// possibly temp while refactoring
		public SketchLayer(SketchLayer layer, IReadOnlyList<SketchLayer> children)
			: this(layer.Id,
				layer.Parent,
				layer.Frame,
				layer.Name,
				layer.NameIsFixed,
				layer.Alignment,
				layer.Rotation,
				layer.IsFlippedVertical,
				layer.IsFlippedHorizontal,
				layer.Style,
				children)
		{
		}

		public SketchLayer(
			Guid id,
			SketchLayer parent,
			SketchRect frame,
			string name,
			bool nameIsFixed,
			SketchAlignment alignment,
			double rotation,
			bool flippedVertical,
			bool flippedHorizontal,
			Optional<SketchStyle> style,
			IReadOnlyList<SketchLayer> children) : base(id)
		{
			Parent = parent;
			Frame = frame;
			Name = name;
			NameIsFixed = nameIsFixed;
			Alignment = alignment;
			Rotation = rotation;
			IsFlippedVertical = flippedVertical;
			IsFlippedHorizontal = flippedHorizontal;
			Style = style;
			Layers = children;
		}
	}
}
