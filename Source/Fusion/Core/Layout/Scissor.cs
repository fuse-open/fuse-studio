namespace Outracks.Fusion
{
	public static partial class Layout
	{
		public static IControl ScissorLeft(this IControl content, Points points)
		{
			return content.Scissor(RectangleEdge.Left, points);
		}
	
		public static IControl ScissorRight(this IControl content, Points points)
		{
			return content.Scissor(RectangleEdge.Right, points);
		}
	
		public static IControl ScissorTop(this IControl content, Points points)
		{
			return content.Scissor(RectangleEdge.Top, points);
		}

		public static IControl ScissorBottom(this IControl content, Points points)
		{
			return content.Scissor(RectangleEdge.Bottom, points);
		}

		/// <summary>
		/// Cuts away a part of the control
		/// </summary>
		/// <param name="content">The content to cut from</param>
		/// <param name="edge">From which side of the content to cut from</param>
		/// <param name="points">How much to cut, measured as the dimension of the cut away area orthogonal to edge</param>
		/// <returns>A visually clipped control (desiring points less size)</returns>
		public static IControl Scissor(this IControl content, RectangleEdge edge, Points points)
		{
			edge = edge.FlipVerticallyOnMac();

			return content
				.WithFrame(f => f.WithEdge(edge, f.GetEdge(edge).Add(edge.IsMinimal() ? new Points(-points) : points)))
				.Clip()
				.WithDimension(edge.NormalAxis(), content.DesiredSize[edge.NormalAxis()].Sub(points))
				;
		}
	}
}