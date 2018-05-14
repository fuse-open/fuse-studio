using System;
using System.Reactive.Linq;

namespace Outracks.Fusion
{
	public class CornerRadius : IEquatable<CornerRadius>
	{
		public static CornerRadius None = new CornerRadius(0, 0);
		public readonly Points RadiusX;
		public readonly Points RadiusY;

		public CornerRadius(Points radiusX, Points radiusY)
		{
			RadiusX = radiusX;
			RadiusY = radiusY;
		}

		public CornerRadius(Points uniformRadius)
		{
			RadiusX = uniformRadius;
			RadiusY = uniformRadius;
		}

		public bool Equals(CornerRadius other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return RadiusX.Equals(other.RadiusX) && RadiusY.Equals(other.RadiusY);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((CornerRadius)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (RadiusX.GetHashCode() * 397) ^ RadiusY.GetHashCode();
			}
		}

		public static bool operator ==(CornerRadius left, CornerRadius right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(CornerRadius left, CornerRadius right)
		{
			return !Equals(left, right);
		}
	}

	public static class CornerRadiusExtensions
	{
		public static IObservable<CornerRadius> ToCornerRadius(this IObservable<Points> radius)
		{
			return radius.Select(p => new CornerRadius(p));
		}
	}
}