using System;
using System.Reactive.Linq;
using System.Reflection;
using Outracks.Fuse.Theming.Themes;
using Outracks.Fusion;
using Outracks.IO;

namespace Outracks.Fuse
{
	public static class Icons
	{
		public static Icon Folder { get; private set; }

		static Icons()
		{
			var asm = Assembly.GetExecutingAssembly();
			Folder = new Icon(() => asm.GetManifestResourceStream("Outracks.Fuse.Icons.Folder.png"));
		}

		public static Optional<Icon> GetFileIcon(AbsoluteFilePath f)
		{
			return Optional.None();
		}

		public static IControl MediumIcon(this IElement element, Brush borderBrush, Brush boxBrush)
		{
			return element.Name.Select(name => MediumIcon(name, borderBrush, boxBrush)).Switch();
		}

		public static IControl MediumIcon(string elementName, Brush borderBrush = default(Brush), Brush boxBrush = default(Brush))
		{
			var svgResourceName = string.Format("Outracks.Fuse.Icons.Primitives.{0}.Active.svg", elementName);
			if (Image.HasResource(svgResourceName, typeof(Icons).Assembly))
			{
				var colorMap = !borderBrush.IsDefault ? borderBrush.Select(color => new SingleColorMap(color)) : null;
				return FromResource(
						svgResourceName,
						colorMap: colorMap)
					.Center()
					.WithMediumSize();
			}

			// Just one SVG icon missing from set
			switch (elementName)
			{
				case "App": return App(borderBrush, boxBrush).WithMediumSize();
			}

			return PanelBorder(borderBrush, Control.Empty).WithMediumSize();
		}

		public static IControl ProjectIcon()
		{
			return Theme.CurrentTheme.Select(
					theme => FromResource(
						theme == Themes.OriginalDark
							? "Outracks.Fuse.Icons.Dashboard.ProjectIcon_dark@2x.png"
							: "Outracks.Fuse.Icons.Dashboard.ProjectIcon_light@2x.png"))
				.Switch();
		}

		public static IControl LoggedOut()
		{
			return  Theme.CurrentTheme.Select(
					theme => FromResource(
						theme == Themes.OriginalDark
							? "Outracks.Fuse.Icons.Dashboard.LoggedOut_dark@2x.png"
							: "Outracks.Fuse.Icons.Dashboard.LoggedOut_light@2x.png"))
				.Switch();
		}

		public static IControl AddViewport()
		{
			return  Theme.CurrentTheme.Select(
					theme => FromResource(
						theme == Themes.OriginalDark
							? "Outracks.Fuse.Icons.HeaderBar.AddViewport_dark.png"
							: "Outracks.Fuse.Icons.HeaderBar.AddViewport_light.png"))
				.Switch()
				.WithSize(new Size<Points>(16,37));
		}

		public static IControl DevicesIcon()
		{
			return  Theme.CurrentTheme.Select(
					theme => FromResource(
						theme == Themes.OriginalDark
							? "Outracks.Fuse.Icons.HeaderBar.Devices_dark.png"
							: "Outracks.Fuse.Icons.HeaderBar.Devices_light.png"))
				.Switch()
				.WithSize(new Size<Points>(16,16));
		}

		public static IControl Community()
		{
			return FromResource("Outracks.Fuse.Icons.Dashboard.Community@2x.png");
		}

		public static IControl ClassesSmall()
		{
			return FromResource("Outracks.Fuse.Icons.ClassesSmall.png")
				.WithSize(new Size<Points>(20,20));
		}

		public static IControl Docs()
		{
			return FromResource("Outracks.Fuse.Icons.Dashboard.Docs@2x.png");
		}

		public static IControl DragIconSmall()
		{
			return Theme.CurrentTheme.Select(
					theme => FromResource(
						theme == Themes.OriginalDark
							? "Outracks.Fuse.Icons.DragIconSmallDark.png"
							: "Outracks.Fuse.Icons.DragIconSmall.png"))
				.Switch()
				.WithSize(new Size<Points>(24,15));
		}

		public static IControl ArrowIcon()
		{
			return FromResource("Outracks.Fuse.Icons.Arrow@2x.png");
		}
		public static IControl Learn()
		{
			return FromResource("Outracks.Fuse.Icons.Dashboard.Learn@2x.png");
		}
		public static IControl FontSize()
		{
			return FromResource("Outracks.Fuse.Icons.FontSize.png");
		}
		public static IControl CharacterLimit()
		{
			return FromResource("Outracks.Fuse.Icons.CharacterLimit.png");
		}
		public static IControl LineSpacing()
		{
			return FromResource("Outracks.Fuse.Icons.LineSpacing.png");
		}

		public static IControl Confirm(Brush overlayColor = default(Brush))
		{
			return FromResource("Outracks.Fuse.Icons.Confirm.png", overlayColor);
		}

		public static IControl Cancel(Brush fg)
		{
			var size = Size.Create<Points>(14, 14);
			var stroke = Stroke.Create(1, fg);

			return Layout.Layer(
				Shapes.Line(
					start: Point.Create<Points>(0, 0),
					end: Point.Create(size.Width, size.Height),
					stroke: stroke),
				Shapes.Line(
					start: Point.Create(size.Width, 0),
					end: Point.Create(0, size.Height),
					stroke: stroke))
				.WithSize(size);
		}
		public static IControl Wrap(Brush overlayColor = default(Brush))
		{
			return FromResource("Outracks.Fuse.Icons.Wrap.Wrap.png", overlayColor);
		}
		public static IControl NoWrap(Brush overlayColor = default(Brush))
		{
			return FromResource("Outracks.Fuse.Icons.Wrap.NoWrap.png", overlayColor);
		}
		public static IControl Checkmark(Brush overlayColor = default(Brush))
		{
			return FromResource("Outracks.Fuse.Icons.Checkmark.png", overlayColor);
		}

		public static IControl ExtractClass(IObservable<bool> enabled)
		{
			return enabled
				.Select(
					x => x
						? FromResource("Outracks.Fuse.Icons.ExtractClassEnabled.png")
						: FromResource("Outracks.Fuse.Icons.ExtractClassDisabled.png"))
				.Switch();
		}

		public static IControl CannotEditPlaceholder()
		{
			return Theme.CurrentTheme.Select(
				theme => FromResource(
					theme == Themes.OriginalDark
						? "Outracks.Fuse.Icons.CannotEdit.Dark.png"
						: "Outracks.Fuse.Icons.CannotEdit.Light.png"))
				.Switch();
		}


		static IControl WithMediumSize(this IControl control)
		{
			return control.WithWidth(21).WithHeight(16);
		}

		static IControl App(Brush borderBrush, Brush brush)
		{
			return PrimitivesIcon("AppIcon", brush).Center();
		}

		static IControl Panel(Brush borderBrush, Brush boxBrush)
		{
			return PanelBorder(
				borderBrush,
				Box(boxBrush));
		}

		static IControl StackPanel(Brush borderBrush, Brush boxBrush)
		{
			return PanelBorder(
				borderBrush,
				Layout.SubdivideVertically(
					Box(boxBrush),
					Box(boxBrush)));
		}

		static IControl StackPanelHorizontal(Brush borderBrush, Brush boxBrush)
		{
			return PanelBorder(
				borderBrush,
				Layout.SubdivideHorizontally(
					Box(boxBrush),
					Box(boxBrush)));
		}

		static IControl Grid(Brush borderBrush, Brush boxBrush)
		{
			return PanelBorder(borderBrush,
				Layout.SubdivideVertically(
					Layout.SubdivideHorizontally(Box(boxBrush),Box(boxBrush),Box(boxBrush)),
					Layout.SubdivideHorizontally(Box(boxBrush),Box(boxBrush),Box(boxBrush))));
		}

		static IControl DockPanel(Brush borderBrush, Brush boxBrush)
		{
			return PanelBorder(borderBrush, Box2(boxBrush).CenterHorizontally().DockTop());
		}

		static IControl WrapPanel(Brush borderBrush, Brush boxBrush)
		{
			return PanelBorder(borderBrush,
				Layout.SubdivideVertically(
					Layout.SubdivideHorizontally(Box(boxBrush).Span(1), Box(boxBrush).Span(2)),
					Layout.SubdivideHorizontally(Box(boxBrush).Span(2), Control.Empty.Span(1))));
		}

		static IControl Each(Brush borderBrush, Brush brush)
		{
			return PanelBorder(borderBrush,
				PrimitivesIcon("EachArrows", brush).Center());
		}

		static IControl ScrollView(Brush borderBrush)
		{
			return PanelBorder(
				borderBrush,
				PrimitivesIcon("ScrollViewArrows", borderBrush).CenterVertically());
		}

		static IControl Rectangle(Brush fillBrush)
		{
			return PanelBorder(
				Brush.Transparent,
				Box(fillBrush).WithWidth(21).WithHeight(6).Center());
		}

		static IControl Circle(Brush fillBrush)
		{
			return PanelBorder(
				Brush.Transparent,
				Shapes.Circle(fill: fillBrush).WithWidth(11).WithHeight(11).Center());
		}

		static IControl PanelBorder(Brush brush, IControl control)
		{
			return control
				.WithPadding(new Thickness<Points>(1.5))
				.WithOverlay(Shapes.Rectangle(Stroke.Create(1, brush, new StrokeDashArray(2, 1))));
		}

		static IControl Box(Brush fill)
		{
			return Shapes
				.Rectangle(fill: fill, cornerRadius:Observable.Return(new CornerRadius(new Points(1))))
				.WithWidth(5).WithHeight(5)
				.WithPadding(new Thickness<Points>(0.5));
		}

		static IControl Box2(Brush fill)
		{
			return Shapes
				.Rectangle(fill: fill, cornerRadius: Observable.Return(new CornerRadius(new Points(1))))
				.WithWidth(11).WithHeight(5)
				.WithPadding(new Thickness<Points>(0.5));
		}

		static IControl PrimitivesIcon(string name, Brush overlayColor = default(Brush))
		{
			return FromResource("Outracks.Fuse.Icons.Primitives." + name + ".png", overlayColor);
		}

		static IControl FromResource(string resourceName, Brush overlayColor = default(Brush), IObservable<IColorMap> colorMap = null)
		{
			colorMap = colorMap ?? Theme.IconColorMap;
			return overlayColor.IsDefault
				? Image.FromResource(resourceName, typeof(Icons).Assembly, colorMap: colorMap)
				: Image.FromResource(resourceName, typeof(Icons).Assembly, overlayColor, colorMap);
		}

		class SingleColorMap : IColorMap
		{
			bool Equals(SingleColorMap other)
			{
				return _singleColor.Equals(other._singleColor);
			}

			public override bool Equals(object obj)
			{
				if (ReferenceEquals(null, obj)) return false;
				if (ReferenceEquals(this, obj)) return true;
				if (obj.GetType() != GetType()) return false;
				return Equals((SingleColorMap)obj);
			}

			public override int GetHashCode()
			{
				return _singleColor.GetHashCode();
			}

			readonly Color _singleColor;

			public SingleColorMap(Color singleColor)
			{
				_singleColor = singleColor;
			}

			public Color Map(Color color)
			{
				return _singleColor;
			}
		}
	}
}
