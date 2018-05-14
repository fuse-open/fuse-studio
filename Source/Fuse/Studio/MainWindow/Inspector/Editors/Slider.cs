namespace Outracks.Fuse.Inspector.Editors
{
	using Fusion;
	
	static class SliderEditor
	{
		public static IControl Create(IAttribute<double> value, double min, double max)
		{
			return Slider.Create(value, min, max)
				.WithHeight(CellLayout.DefaultCellHeight);
		}
	}
}