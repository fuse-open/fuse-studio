namespace SketchConverter.SketchModel
{
	public struct SketchAxisAlignment
	{
		// TODO: Some of these options are mutually exclusive, that is,
		// only *one* of AlignStart and AlignEnd can be active if FixSize == true.
		// Should somehow make this state uninstantiable.

		public readonly bool AlignStart;
		public readonly bool AlignEnd;
		public readonly bool FixSize;

		public SketchAxisAlignment(bool alignStart, bool alignEnd, bool fixSize)
		{
			AlignStart = alignStart;
			AlignEnd = alignEnd;
			FixSize = fixSize;
		}
	}

	public struct SketchAlignment
	{
		public readonly SketchAxisAlignment Horizontal;
		public readonly SketchAxisAlignment Vertical;

		public SketchAlignment(SketchAxisAlignment horizontal, SketchAxisAlignment vertical)
		{
			Horizontal = horizontal;
			Vertical = vertical;
		}
	}
}
