using Outracks.Fusion;

namespace Outracks.Fuse.Inspector.Sections
{
	class ImageSection
	{
		public static IControl Create(IProject project, IElement element, IEditorFactory editors)
		{
			var file = element.GetString("File", "");
			var stretchMode = element.GetEnum("StretchMode", StretchMode.Uniform);

			var stretchDirection = element.GetEnum("StretchDirection", StretchDirection.Both);
			var stretchSizing = element.GetEnum("StretchSizing", StretchSizing.Zero);

			return Layout.StackFromTop(
					Spacer.Medium,

					editors.FilePath(
							attribute: file,
							projectRoot: project.RootDirectory,
							fileFilters: new[] { new FileFilter("Image Files", ".png", ".jpg", ".jpeg")  },
							placeholderText: "Path to image file")
						.WithInspectorPadding(),

					Spacer.Medium,

					editors.Dropdown(stretchMode)
						.WithLabel("Stretch Mode")
						.WithInspectorPadding(),

					Spacer.Medium,

					Layout.Dock()
						.Left(editors.Dropdown(stretchDirection).WithLabelAbove("Stretch Direction"))
						.Right(editors.Dropdown(stretchSizing).WithLabelAbove("Stretch Sizing"))
						.Fill().WithInspectorPadding(),

					Spacer.Medium)
				.MakeCollapsable(RectangleEdge.Bottom, element.Is("Fuse.Controls.Image"));
		}
	}

	enum StretchMode
	{
		PointPrecise,
		PixelPrecise,
		PointPrefer,
		Fill,
		Scale9,
		Uniform,
		UniformToFill,
	}

	enum StretchDirection
	{
		Both,
		UpOnly,
		DownOnly,
	}

	enum StretchSizing
	{
		Zero,
		Natural,
	}
}
