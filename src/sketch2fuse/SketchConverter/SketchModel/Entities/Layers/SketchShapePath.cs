namespace SketchConverter.SketchModel
{
	public enum SketchBooleanOperation
	{
		NoOperation = -1,
		Union = 0,
		Subtraction = 1,
		Intersection = 2,
		Difference = 3
	}

	public class SketchShapePath : SketchLayer
	{
		public readonly SketchPath Path;
		public readonly SketchBooleanOperation BooleanOperation;

		public SketchShapePath(
			SketchLayer layer,
			SketchPath path,
			SketchBooleanOperation operation)
			: base(layer)
		{
			Path = path;
			BooleanOperation = operation;
		}
	}
}
