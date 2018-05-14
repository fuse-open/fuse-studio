using System;
using System.Reactive.Linq;

namespace Outracks.Fusion
{
	public static class ToStrokeExtension
	{
		public static Stroke Switch(this IObservable<Stroke> s)
		{
			return Stroke.Create(
				brush: s.Select(x => x.Brush).Switch(),
				thickness: s.Select(x => x.Thickness).Switch().Replay(1).RefCount(),
				dashArray: s.Select(x => x.DashArray).Switch().Replay(1).RefCount());
		}
	}

	public class StrokeDashArray
	{
		public readonly static StrokeDashArray Solid = new StrokeDashArray();

		public readonly double[] Data;

		public StrokeDashArray(params double[] data)
		{
			Data = data;
		}
	}

	public class Stroke
	{
		public static readonly Stroke Empty = Create(0, Fusion.Brush.Transparent);

		public readonly Brush Brush;
		public readonly IObservable<double> Thickness;
		public readonly IObservable<StrokeDashArray> DashArray;

		Stroke(Brush brush, IObservable<double> thickness, IObservable<StrokeDashArray> dashArray)
		{
			Brush = brush;
			Thickness = thickness;
			DashArray = dashArray;
		}

		public static Stroke Create(double thickness, Brush brush)
		{
			return new Stroke(brush, Observable.Return(thickness), Observable.Return(StrokeDashArray.Solid));
		}

		public static Stroke Create(IObservable<double> thickness, Brush brush, IObservable<StrokeDashArray> dashArray)
		{
			return new Stroke(brush, thickness, dashArray);
		}

		public static Stroke Create(double thickness, Color color, StrokeDashArray dashArray)
		{
			return new Stroke(color, Observable.Return(thickness), Observable.Return(dashArray));
		}
		public static Stroke Create(double thickness, Brush color, StrokeDashArray dashArray)
		{
			return new Stroke(color, Observable.Return(thickness), Observable.Return(dashArray));
		}
	}
}