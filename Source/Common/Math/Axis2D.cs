using System;
using Outracks.Diagnostics;

namespace Outracks
{
	public enum Axis2D
	{
		Horizontal, 
		Vertical
	}
	public enum Direction2D
	{
		LeftToRight = RectangleEdge.Left,
		TopToBottom = RectangleEdge.Top,
		RightToLeft = RectangleEdge.Right,
		BottomToTop = RectangleEdge.Bottom,
	}

	public enum RectangleEdge
	{
		Left = 0,
		Top = 1,
		Right = 2,
		Bottom = 3,
	}
	
	[Flags]
	public enum RectangleEdges
	{
		None = 0,
		Left = 1 << 0,
		Top = 1 << 1,
		Right = 1 << 2,
		Bottom = 1 << 3,
		All = Left | Top | Right | Bottom,
	}

	public static class Axis2DExtensions
	{
		public static bool ShouldFlip = Platform.OperatingSystem == OS.Mac;

		public static Thickness<T> FlipVerticallyOnMac<T>(this Thickness<T> thickness)
		{
			return Thickness.Create(edge => thickness[edge.FlipVerticallyOnMac()]);
		}

		public static RectangleEdge FlipVerticallyOnMac(this RectangleEdge edge)
		{
			if (edge == RectangleEdge.Top && ShouldFlip)
				return RectangleEdge.Bottom;

			if (edge == RectangleEdge.Bottom && ShouldFlip)
				return RectangleEdge.Top;

			return edge;
		}

		public static RectangleEdges ToFlags(this RectangleEdge edge)
		{
			switch (edge)
			{
				case RectangleEdge.Right:
					return RectangleEdges.Right;
				case RectangleEdge.Left:
					return RectangleEdges.Left;
				case RectangleEdge.Bottom:
					return RectangleEdges.Bottom;
				case RectangleEdge.Top:
					return RectangleEdges.Top;
			}
			return RectangleEdges.None;
		}

		public static Axis2D NormalAxis(this RectangleEdge _edge)
		{
			return _edge == RectangleEdge.Left || _edge == RectangleEdge.Right
				? Axis2D.Horizontal
				: Axis2D.Vertical;
		}

		public static bool IsMinimal(this RectangleEdge _edge)
		{
			return _edge == RectangleEdge.Left || _edge == RectangleEdge.Top;
		}

		public static double Sign(this RectangleEdge edge)
		{
			return edge.IsMinimal() ? 1.0 : -1.0;
		}
		public static Direction2D DirectionToEdge(this RectangleEdge edge)
		{
			switch (edge)
			{
				case RectangleEdge.Right:
					return Direction2D.LeftToRight;
				case RectangleEdge.Left:
					return Direction2D.RightToLeft;
				case RectangleEdge.Bottom:
					return Direction2D.TopToBottom;
				case RectangleEdge.Top:
					return Direction2D.BottomToTop;
			}
			throw new ArgumentException();
		}
		public static Direction2D DirectionToOpposite(this RectangleEdge edge)
		{
			switch (edge)
			{
				case RectangleEdge.Left:
					return Direction2D.LeftToRight;
				case RectangleEdge.Right:
					return Direction2D.RightToLeft;
				case RectangleEdge.Top:
					return Direction2D.TopToBottom;
				case RectangleEdge.Bottom:
					return Direction2D.BottomToTop;
			}
			throw new ArgumentException();
		}
		public static RectangleEdge Opposite(this RectangleEdge edge)
		{
			switch (edge)
			{
				case RectangleEdge.Left:
					return RectangleEdge.Right;
				case RectangleEdge.Right:
					return RectangleEdge.Left;
				case RectangleEdge.Top:
					return RectangleEdge.Bottom;
				case RectangleEdge.Bottom:
					return RectangleEdge.Top;
			}
			throw new ArgumentException();
		}

		public static Axis2D Opposite(this Axis2D axis)
		{
			return axis == Axis2D.Horizontal ? Axis2D.Vertical : Axis2D.Horizontal;
		}
	}


}