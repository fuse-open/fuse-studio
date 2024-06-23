using SketchConverter.SketchModel;
using SketchImporter.UxGenerator;

namespace SketchConverter.UxBuilder
{
	public class LayoutBuilder
	{
		readonly SketchLayer _layer;

		SketchRect Frame => _layer.Frame;
		SketchAlignment Alignment => _layer.Alignment;

		public LayoutBuilder(SketchLayer layer)
		{
			_layer = layer;
		}

		enum UxAxisAlignment
		{
			Default,
			Center,
			Start,
			End
		}

		class UxInterval
		{
			public UxSize Start { get; set; }
			public UxSize End { get; set; }

			public static UxVector CombineAxes(UxInterval horizontal, UxInterval vertical)
			{
				return new UxVector(
					horizontal.Start,
					vertical.Start,
					horizontal.End,
					vertical.End
				);
			}
		}

		struct UxAxisLayout
		{
			public UxAxisAlignment Alignment { get; set; }
			public UxSize Offset { get; set; }
			public UxFloat2 Margin { get; set; }
			public UxSize Size { get; set; }
		}

		string BuildHorizontalAlignmentString(UxAxisAlignment aa)
		{
			switch (aa)
			{
				case UxAxisAlignment.Start: return "Left";
				case UxAxisAlignment.End: return "Right";
				case UxAxisAlignment.Center: return "Center";
				default: return "";
			}
		}

		string BuildVerticalAlignmentString(UxAxisAlignment aa)
		{
			switch (aa)
			{
				case UxAxisAlignment.Start: return "Top";
				case UxAxisAlignment.End: return "Bottom";
				case UxAxisAlignment.Center: return "Center";
				default: return "";
			}
		}

		string BuildUxAlignmentString(UxAxisAlignment horizontal, UxAxisAlignment vertical)
		{
			if (horizontal == UxAxisAlignment.Default
				&& vertical == UxAxisAlignment.Default)
			{
				return "Default";
			}

			if (horizontal == UxAxisAlignment.Center
				&& vertical == UxAxisAlignment.Center)
			{
				return "Center";
			}

			if (horizontal == UxAxisAlignment.Center
				&& vertical == UxAxisAlignment.Default)
			{
				return "HorizontalCenter";
			}

			if (horizontal == UxAxisAlignment.Default
				&& vertical == UxAxisAlignment.Center)
			{
				return "VerticalCenter";
			}

			return BuildVerticalAlignmentString(vertical) + BuildHorizontalAlignmentString(horizontal);
		}

		UxSize AnchorForAlignment(UxAxisAlignment alignment)
		{
			switch (alignment)
			{
				case UxAxisAlignment.Start: return UxSize.Percent(0);
				case UxAxisAlignment.End: return UxSize.Percent(100);
				default: return UxSize.Percent(50);
			}
		}

		UxAxisLayout BuildAxisLayout(SketchAxisAlignment sa, float start, float size, float parentSize)
		{
			var end = parentSize - (start + size);

			var layout = new UxAxisLayout()
			{
				Alignment = UxAxisAlignment.Start,
				Offset = UxSize.Points(0),
				Margin = new UxFloat2(0,0),
				Size = null
			};

			if (sa.FixSize)
			{
				layout.Size = UxSize.Points(size);

				if (sa.AlignStart)
				{
					layout.Margin.X = start;
					layout.Alignment = UxAxisAlignment.Start;
				}
				else if (sa.AlignEnd)
				{
					layout.Margin.Y = end;
					layout.Alignment = UxAxisAlignment.End;
				}
				else
				{
					layout.Alignment = UxAxisAlignment.Center;

					var offset = start + 0.5f * (size - parentSize);
					layout.Offset = UxSize.Percent(100 * offset / parentSize );
				}
			}
			else
			{
				if(sa.AlignStart && sa.AlignEnd)
				{
					layout.Alignment = UxAxisAlignment.Default;
					layout.Margin.X = start;
					layout.Margin.Y = end;
				}
				else
				{
					layout.Size = UxSize.Percent(100 * size / parentSize);

					if(sa.AlignStart)
					{
						layout.Alignment = UxAxisAlignment.Start;
						layout.Offset = UxSize.Points(start);
					}
					else if(sa.AlignEnd)
					{
						layout.Alignment = UxAxisAlignment.End;
						layout.Offset = UxSize.Points(-end);
					}
					else
					{
						layout.Alignment = UxAxisAlignment.Start;
						layout.Offset = UxSize.Percent(100 * start / parentSize);
					}
				}
			}

			return layout;
		}

		public UxNode Build(UxNode node)
		{
			var alignment = _layer.Alignment;

			var frame = _layer.Frame;
			var parentFrame = _layer.Parent.Frame;

			var attributes = node.Attributes;

			var horizontal = BuildAxisLayout(alignment.Horizontal,
				(float)frame.X,
				(float)frame.Width,
				(float?)parentFrame?.Width ?? 0);

			var vertical = BuildAxisLayout(alignment.Vertical,
				(float)frame.Y,
				(float)frame.Height,
				(float?)parentFrame?.Height ?? 0);

			if (horizontal.Size != null)
				attributes["Width"] = horizontal.Size;

			if (vertical.Size != null)
				attributes["Height"] = vertical.Size;

			var alignmentString = BuildUxAlignmentString(horizontal.Alignment, vertical.Alignment);
			if (alignmentString == "Default")
			{
				// If alignment is Default, we don't need to specify Width|Height="100%"
				if (horizontal.Size == UxSize.Percent(100)) attributes.Remove("Width");
				if (vertical.Size == UxSize.Percent(100)) attributes.Remove("Height");
			}
			else
			{
				attributes["Alignment"] = new UxString(alignmentString);
			}

			if (horizontal.Offset != null || vertical.Offset != null)
			{
				var horizontalOffset = horizontal.Offset ?? UxSize.Points(0);
				var verticalOffset = vertical.Offset ?? UxSize.Points(0);

				if (horizontalOffset.Value != 0 || verticalOffset.Value != 0)
				{
					attributes["Offset"] = new UxVector(horizontalOffset, verticalOffset);
				}
			}

			var hMargin = horizontal.Margin;
			var vMargin = vertical.Margin;

			attributes["Margin"] = new UxFloat4(
				hMargin.X,
				vMargin.X,
				hMargin.Y,
				vMargin.Y);

			return node;
		}
	}
}
