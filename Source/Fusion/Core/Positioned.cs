namespace Outracks.Fusion
{
	public static class Positioned
	{
		public static Positioned<T> Create<T>(Point<Points> position, T value)
		{
			return new Positioned<T>(position, value);
		}
	}

	public class Positioned<T>
	{
		public Point<Points> Position { get; private set; }
		public T Value { get; private set; }

		public Positioned(Point<Points> position, T value)
		{
			Position = position;
			Value = value;
		}
	}

}