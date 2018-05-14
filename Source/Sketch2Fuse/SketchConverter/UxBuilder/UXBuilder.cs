using System;
using System.Collections.Generic;
using System.Linq;
using SketchConverter.API;
using SketchConverter.SketchModel;
using SketchConverter.SketchParser;
using SketchImporter.UxGenerator;

namespace SketchConverter.UxBuilder
{
	public class UxBuilder
	{
		private readonly ILogger _log;
		private readonly SymbolClassNameBuilder _symbolClassNameBuilder;
		private readonly IAssetEmitter _assetEmitter;

		public UxBuilder(SymbolClassNameBuilder symbolClassNameBuilder, IAssetEmitter assetEmitter, ILogger log)
		{
			_symbolClassNameBuilder = symbolClassNameBuilder;
			_assetEmitter = assetEmitter;
			_log = log;
		}

		public UxNode BuildSymbolClass(SketchSymbolMaster symbol)
		{
			var symbolClassNode = new UxNode
			{
				ClassName = "Panel",
				SketchLayerName = symbol.Name,
				Attributes = new Dictionary<string, IUxSerializeable> {
					{"ux:Class", new UxString(_symbolClassNameBuilder.GetClassName(symbol))},
					{"Width", UxSize.Points((float) symbol.Frame.Width)},
					{"Height", UxSize.Points((float) symbol.Frame.Height)}
				}
			};
			AddChildrenLayers(symbolClassNode, symbol);
			return symbolClassNode;
		}

		public UxNode BuildPage(SketchLayer parent)
		{
			var pageNode = new UxNode
			{
				ClassName = "Panel",
				SketchLayerName = parent.Name,
				Attributes = new Dictionary<string, IUxSerializeable> {
					{"ClipToBounds", new UxString("true")}
				}
			};
			AddChildrenLayers(pageNode, parent);
			return pageNode;
		}

		//Public just for tests
		public UxNode BuildLayer(SketchLayer layer)
		{
			var node = BuildLayerInternal(layer);

			layer.Style.Do(style =>
				style.Opacity.Where(opacity => opacity < 1).Do(opacity =>
					node.Attributes["Opacity"] = new UxFloat((float) opacity)));

			if (layer.Layers.Any())
			{
				var hasMaskedChild = HasMaskedChild(layer);
				if (hasMaskedChild) AddUnsupportedMaskingWarning(node, layer.Name);

				// Skip processing children if we found a direct child that is a mask. 2017-12-12 anette
				// SketchShapeGroup is a SketchParentLayer but BuildShapeGroup handles it's own children.
				// This is inconsistent and should be cleaned up. 2017-12-06 anette
				if(!hasMaskedChild && !(layer is SketchShapeGroup))
				{
					var children = layer.Layers
						.Select(BuildLayer)
						.ToList();

					children.Reverse();

					node.Children.AddRange(children);
				}
			}

			// Flip and rotate the panel. Order of transformations matter,
			// we first flip (done by rotating 180 around the X/Y-axis),
			// then the actual rotation (around the implicit Z-axis)
			if (layer.IsFlippedVertical)
			{
				node.Children.Add(new UxRotation(180, UxRotation.RotationAxis.X));
			}
			if (layer.IsFlippedHorizontal)
			{
				node.Children.Add(new UxRotation(180, UxRotation.RotationAxis.Y));
			}
			if (!layer.Rotation.Equals(0))
			{
				node.Children.Add(new UxRotation(layer.Rotation));
			}


			node.Children.Insert(0, new UxComment(layer.Name));

			return node;
		}

		private void AddUnsupportedMaskingWarning(UxNode node, string layerName)
		{
			_log.Warning("Masked shapes are not supported " + layerName);
			node.Children.Add(new UxComment("Masked shape group is not supported in UX"));
		}

		private static bool HasMaskedChild(SketchLayer parent)
		{
			var hasMaskChild = parent.Layers
				.Select(l => l as SketchShapeGroup)
				.Where(l => l != null)
				.Any(sg => sg.HasClippingMask);

			return hasMaskChild;
		}

		UxNode BuildLayerInternal(SketchLayer layer)
		{
			var group = layer as SketchGroup;
			if (group != null)
			{
				return BuildGroup(group);
			}

			var rectangle = layer as SketchRectangle;
			if (rectangle != null)
			{
				return BuildRectangle(rectangle);
			}

			var shapeGroup = layer as SketchShapeGroup;
			if (shapeGroup != null)
			{
				return BuildShapeGroup(shapeGroup);
			}

			var bitmap = layer as SketchBitmap;
			if (bitmap != null)
			{
				return BuildBitmap(bitmap);
			}

			var text = layer as SketchText;
			if (text != null)
			{
				return BuildText(text);
			}

			var symbolInstance = layer as SketchSymbolInstance;
			if (symbolInstance != null)
			{
				return BuildSymbolInstance(symbolInstance);
			}

			var shapePath = layer as SketchShapePath;
			if (shapePath != null)
			{
				return BuildShapePath(shapePath);
			}

			var warning = $"Unimplemented layer type: {layer.GetType().Name}";
			_log.Warning(warning);
			var groupNode = new UxNode
			{
				ClassName = "Panel",
				SketchLayerName = layer.Name
			};
			groupNode.Children.Add(new UxComment(warning));
			return groupNode;
		}

		private UxNode BuildShapePath(SketchShapePath shapePath)
		{
			var svgString = SketchCurvePointsToSvg.ToSvgString(shapePath.Path);
			var node = new UxNode
			{
				ClassName = "Path",
				SketchLayerName = shapePath.Name,
				Attributes =
				{
					["Data"] = new UxString(svgString),
					["StretchMode"] = new UxString("Fill")
				}
			};

			return BuildLayout(shapePath, node);
		}

		private UxNode BuildSymbolInstance(SketchSymbolInstance symbolInstance)
		{
			var className = _symbolClassNameBuilder.GetClassName(symbolInstance);
			var node = new UxNode {
				ClassName = className,
				SketchLayerName = symbolInstance.Name
			};

			return BuildLayout(symbolInstance, node);
		}

		UxNode BuildGroup(SketchGroup group)
		{
			if (group.Layers.Count == 0)
			{
				return new NullNode(new UxComment("Skipped empty group '" + group.Name + "'"));
			}
			var node = new UxNode
			{
				ClassName = "Panel",
				SketchLayerName = group.Name
			};
			return BuildLayout(group, node);
		}

		UxNode BuildRectangle(SketchRectangle rectangle)
		{
			// Is axis aligned rectangle
			if (rectangle.Path == null)
				throw new SketchParserException("Rectangle has no path");
			if (rectangle.Path.Points == null)
				throw new SketchParserException("Rectangle.Path has no points");
			if (Geometry.IsAxisAlignedRectangle(rectangle.Path))
			{
				// Create a Ux Rectangle if we have an axis aligned quadrilateral
				var node = new UxNode
				{
					ClassName = "Rectangle",
					SketchLayerName = rectangle.Name
				};
				if (rectangle.Path.Points.Any(p => p.CornerRadius > 0.0))
				{
					var cr = rectangle.Path.Points.Select(p => p.CornerRadius).ToList();
					node.Attributes["CornerRadius"]
						= new UxFloat4(
							(float) cr[0],
							(float) cr[1],
							(float) cr[2],
							(float) cr[3]
						);
				}
				return BuildLayout(rectangle, node);
			}
			// transformed rectangle, not axis aligned anymore or have less than or
			// more than four corners
			return BuildShapePath(rectangle);
		}



		UxNode BuildShapeGroup(SketchShapeGroup shapeGroupLayer)
		{
			var groupNode = new UxNode
			{
				ClassName = "Panel",
				SketchLayerName = shapeGroupLayer.Name
			};

			if (shapeGroupLayer.Layers.Count == 0)
			{
				_log.Warning($"Shape group {shapeGroupLayer.Name} has no layers");
				return groupNode;
			}

			// Warn if we have any combined shapes, as it is currently not supported
			var combinedShapes = shapeGroupLayer
			                     .Layers.Cast<SketchShapePath>()
			                     .Where(shape => shape.BooleanOperation !=
			                                     SketchBooleanOperation.NoOperation);
			if (combinedShapes.Any())
			{
				groupNode.Children.Add(new UxComment("Combined shapes are not supported in UX"));
				_log.Warning("Combined shapes are not supported " + shapeGroupLayer.Name + " with " + combinedShapes.Count() + " number of combined layers");
				return groupNode;
			}

			var shapeNodes = shapeGroupLayer
			                 .Layers
			                 .Cast<SketchShapePath>()
			                 .Where(shape => shape.BooleanOperation ==
			                                 SketchBooleanOperation.NoOperation)
			                 .Select(BuildLayer);
			foreach (var shapeNode in shapeNodes)
			{
				ApplyShapeStyle(shapeGroupLayer, shapeNode);
				groupNode.Children.Add(shapeNode);
			}

			return BuildLayout(shapeGroupLayer, groupNode);
		}

		UxNode BuildText(SketchText text)
		{
			var uxNode = new UxNode {
				ClassName="Text",
				SketchLayerName = text.Name
			};

			if (text.AttributedString.Attributes.Count > 1)
			{
				_log.Warning($"UX Builder: Multiple text styles on the same text element not supported in UX. Found {text.AttributedString.Attributes.Count} text styles on {text.Name}, using just one of them.");
			}
			var stringAttributes = text.AttributedString.Attributes.First();

			uxNode.Attributes["Color"] = SketchColorToFloat4(stringAttributes.Color);
			uxNode.Attributes["FontSize"] = new UxFloat((float)stringAttributes.FontSize);
			uxNode.Attributes["Value"] = new UxString(text.AttributedString.Contents);
			uxNode.Attributes["TextTruncation"] = new UxString("None");
			uxNode.Attributes["TextWrapping"] = new UxString("Wrap");

			if (stringAttributes.Alignment != SketchTextAlignment.Left)
			{
				uxNode.Attributes["TextAlignment"] = new UxString(BuildTextAlignment(stringAttributes.Alignment));
			}

			uxNode.Children.AddRange(BuildShadows(text.Name, text.Style));
			
			if (text.Style.Borders.Count > 0)
			{
				_log.Warning($"UX Builder: Borders on Sketch texts not supported in UX (Sketch object: {text.Name}).");
			}

			if (text.Style.Fills.Count > 0)
			{
				_log.Warning($"UX Builder: Fills on Sketch texts not supported in UX (Sketch object: {text.Name}). Using text style color instead.");
			}

			return BuildLayout(text, uxNode);
		}

		private IEnumerable<IUxSerializeable> BuildShadows(string layerName, SketchStyle style)
		{
			return style.Shadows
				.Where(x => x.IsEnabled)
				.Select(x => BuildShadow(x, true))
				.Concat(InnerShadowWarning(layerName, style.InnerShadows.Count));
		}

		private IEnumerable<IUxSerializeable> InnerShadowWarning(string layerName, int numInnerShadows)
		{
			if (numInnerShadows > 0)
			{
				_log.Warning(
					$"UX Builder: Inner shadows not supported. {layerName} has {numInnerShadows} shadow" +
					(numInnerShadows > 1 ? "s." : "."));
				yield return new UxComment("Inner shadow not supported in UX");
			}
		}

		string BuildTextAlignment(SketchTextAlignment alignment)
		{
			switch (alignment)
			{
				case SketchTextAlignment.Center: return "Center";
				case SketchTextAlignment.Right: return "Right";
				default: return "Left";
			}
		}

		private void AddChildrenLayers(UxNode node, SketchLayer parent)
		{
			var hasMaskedChildLayer = HasMaskedChild(parent);
			if (hasMaskedChildLayer)
			{
				_log.Warning("Masked shapes are not supported " + parent.Name);
				node.Children.Add(new UxComment("Masked shape group is not supported in UX"));
			}
			else
			{
				node.Children = parent.Layers
					.AsEnumerable()
					.Reverse()
					.Select(BuildLayer)
					.Cast<IUxSerializeable>()
					.ToList();
			}
		}

		UxNode BuildLayout(SketchLayer layer, UxNode targetNode)
		{
			return new LayoutBuilder(layer).Build(targetNode);
		}

		readonly HashSet<string> _exportedAssets = new HashSet<string>();

		void ExportImage(SketchImage image)
		{
			if (_exportedAssets.Contains(image.Path))
				return;

			_exportedAssets.Add(image.Path);
			_assetEmitter.Write(image);
		}

		UxNode BuildBitmap(SketchBitmap bitmap)
		{
			var imagePath = bitmap.Image.Path;
			ExportImage(bitmap.Image);

			var imageNode = new UxNode
			{
				ClassName = "Image",
				SketchLayerName = bitmap.Name,
				Attributes = new Dictionary<string, IUxSerializeable>
				{
					{"File", new UxString(imagePath)}
				}
			};

			imageNode.Children.AddRange(BuildShadows(bitmap.Name, bitmap.Style));

			return BuildLayout(bitmap, imageNode);
		}

		UxNode BuildSolidColorBrush(SketchSolidColorBrush brush)
		{
			return new UxNode
			{
				ClassName = "SolidColor",
				Attributes = new Dictionary<string, IUxSerializeable>()
				{
					{ "Color", SketchColorToFloat4(brush.Color) }
				}
			};
		}

		UxNode BuildLinearGradientBrush(SketchLinearGradientBrush brush)
		{
			return new UxNode
			{
				ClassName = "LinearGradient",
				Attributes = new Dictionary<string, IUxSerializeable>
				{
					{"StartPoint", SketchPointToFloat2(brush.From)},
					{"EndPoint", SketchPointToFloat2(brush.To)}
				},
				Children = brush.Stops.Select(stop => new UxNode
				{
					ClassName = "GradientStop",
					Attributes = new Dictionary<string, IUxSerializeable>
					{
						{"Color", SketchColorToFloat4(stop.Color)},
						{"Offset", new UxFloat((float)stop.Position)}
					}
				}).Cast<IUxSerializeable>().ToList()
			};
		}

		UxNode BuildBrush(ISketchBrush brush)
		{
			var solidColorBrush = brush as SketchSolidColorBrush;
			if (solidColorBrush != null)
			{
				return BuildSolidColorBrush(solidColorBrush);
			}

			var linearGradientBrush = brush as SketchLinearGradientBrush;
			if (linearGradientBrush != null)
			{
				return BuildLinearGradientBrush(linearGradientBrush);
			}

			return null;
		}

		UxNode BuildShadow(SketchShadow shadow, bool perPixel)
		{
			var dx = shadow.Offset.X;
			var dy = shadow.Offset.Y;
			// atan2 will give us the angle in the correct quadrant with cartesian coordinates.
			// Sketch uses screen coordinates which resulted in the shadow being flipped in the X-direction 
			// We therefore call atan2 with -dx. This gives us the right result. 2018-02-05 anette
			var angle = (180 / Math.PI) * Math.Atan2(dy, -dx);
			var distance = Math.Sqrt(dx*dx + dy*dy);

			var node = new UxNode
			{
				ClassName = "Shadow",
				Attributes = new Dictionary<string, IUxSerializeable>
				{
					{"Angle", new UxFloat((float) angle)},
					{"Distance", new UxFloat((float) distance)},
					{"Color", SketchColorToFloat4(shadow.Color)},
					{"Size", new UxFloat((float) shadow.BlurRadius)}
				}
			};

			if(perPixel)
			{
				node.Attributes["Mode"] = new UxString("PerPixel");
			}

			return node;
		}

		void ApplyShapeStyle(SketchShapeGroup layer, UxNode targetNode)
		{
			var style = layer.Style;

			if (style.Blur.HasValue)
			{
				_log.Warning($"Skipping {style.Blur.Value.BlurType} blur on {layer.Name}. Not supported in UX");
			}

			targetNode.Children.AddRange(BuildShadows(layer.Name, layer.Style));
			
			var fillNodes = style.Fills
				.Where(fill => fill.IsEnabled)
				.Select(x => BuildBrush(x.Brush));
			targetNode.Children.AddRange(fillNodes);

			var strokeNodes = style.Borders
				.Where(border => border.IsEnabled)
				.Select(border => new UxNode
				{
					ClassName = "Stroke",
					Attributes = new Dictionary<string, IUxSerializeable>
					{
						{"Width", new UxFloat((float) border.Thickness)},
						{"Alignment", new UxString(border.Position.ToString())}
					},
					Children = new List<IUxSerializeable> {BuildBrush(border.Brush)}
				});
			targetNode.Children.AddRange(strokeNodes);
		}

		UxFloat4 SketchColorToFloat4(SketchColor color)
		{
			return new UxFloat4(
				(float)color.Red,
				(float)color.Green,
				(float)color.Blue,
				(float)color.Alpha
			);
		}

		UxFloat2 SketchPointToFloat2(SketchPoint point)
		{
			return new UxFloat2(
				(float)point.X,
				(float)point.Y
			);
		}
	}
}
