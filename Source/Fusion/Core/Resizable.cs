using System.Reactive.Linq;

namespace Outracks.Fusion
{
	public static class Resizable
	{
		public static IControl MakeResizable(this IControl self, RectangleEdge edge, string name)
		{
			var property = Property.Create(new Points(280)); //.Settings.GetPointsProperty(name + ".Width");
			return self
				.WithDimension(edge.NormalAxis(), property)
				.MakeResizable(edge, property);
		}

		public static IControl MakeResizable(this IControl self, RectangleEdge edge, IProperty<Points> property, bool invert = false, Points minSize = default(Points))
		{
			var restrictedSize = property.Convert(v => v, v => v.Max(minSize));
			return self
				.WithOverlay(
					Control.Empty
						.WithDimension(edge.NormalAxis(), _ => Observable.Return<Points>(5))						
						.WhileDraggingScrub(restrictedSize, invert ? edge.DirectionToOpposite() : edge.DirectionToEdge())
						.SetCursor(edge.NormalAxis() == Axis2D.Horizontal ? Cursor.ResizeHorizontally : Cursor.ResizeVertically)
						.Dock(edge));
		}
	}
}