namespace Outracks.Fuse.Inspector.Sections
{
	using Fusion;
	
	class CircleSection
	{
		public static IControl Create(IElement element, IEditorFactory editors)
		{
			var startAngleRadians = element.GetAngle("StartAngle", 0.0);
			var startAngleDegrees = element.GetAngle("StartAngleDegrees", 0.0);

			var endAngleRadians = element.GetAngle("EndAngle", 0.0);
			var endAngleDegrees = element.GetAngle("EndAngleDegrees", 0.0);
			//var endAngle = endAngleRadians.Or(endAngleDegrees);

			var lengthAngleRadians = element.GetAngle("LengthAngle", 0.0);
			var lengthAngleDegrees = element.GetAngle("LengthAngleDegrees", 0.0);
			//var lengthAngle = lengthAngleRadians.Or(lengthAngleDegrees);

			//lengthAngleRadians = lengthAngleRadians.And(endAngle.IsNotSet);
			//lengthAngleDegrees = lengthAngleDegrees.And(endAngle.IsNotSet);
			//lengthAngle = lengthAngle.And(endAngle.IsNotSet);

			//endAngleRadians = endAngleRadians.And(lengthAngle.IsNotSet);
			//endAngleDegrees = endAngleDegrees.And(lengthAngle.IsNotSet);
			//endAngle = endAngle.And(lengthAngle.IsNotSet);

			return Layout.StackFromTop(
					Spacer.Small,

					Layout.Dock()
						.Right(editors.Label("Degrees", startAngleDegrees).WithWidth(CellLayout.HalfCellWidth))
						.Right(Spacer.Medium)
						.Right(editors.Label("Radians", startAngleRadians).WithWidth(CellLayout.HalfCellWidth))
						.Fill().WithInspectorPadding(),
					
					Spacer.Smaller,

					Layout.Dock()
						.Left(editors.Label("Start Angle", startAngleRadians))
						.Right(editors.Field(startAngleDegrees).WithWidth(CellLayout.HalfCellWidth))
						.Right(Spacer.Medium)
						.Right(editors.Field(startAngleRadians).WithWidth(CellLayout.HalfCellWidth))
						.Fill().WithInspectorPadding(),

					Spacer.Small,
				
					Layout.Dock()
						.Left(editors.Label("End Angle", endAngleRadians))
						.Right(editors.Field(endAngleDegrees).WithWidth(CellLayout.HalfCellWidth))
						.Right(Spacer.Medium)
						.Right(editors.Field(endAngleRadians).WithWidth(CellLayout.HalfCellWidth))
						.Fill().WithInspectorPadding(),
					
					Spacer.Small,
				
					Layout.Dock()
						.Left(editors.Label("Length Angle", lengthAngleRadians))
						.Right(editors.Field(lengthAngleDegrees).WithWidth(CellLayout.HalfCellWidth))
						.Right(Spacer.Medium)
						.Right(editors.Field(lengthAngleRadians).WithWidth(CellLayout.HalfCellWidth))
						.Fill().WithInspectorPadding(),
					
					Spacer.Medium)
				.MakeCollapsable(RectangleEdge.Bottom, element.Is("Fuse.Controls.Circle"));
		}
	}
}
