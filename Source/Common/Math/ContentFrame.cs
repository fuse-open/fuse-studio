namespace Outracks
{
	public struct ContentFrame<T> where T : INumeric<T>
	{
		public readonly Rectangle<T> FrameBuonds;
		public readonly Rectangle<T> ContentBounds;

		public Rectangle<T> VisibleBounds
		{
			get { return FrameBuonds.Intersect(ContentBounds); }
		}

		public ContentFrame(Rectangle<T> frameBuonds, Rectangle<T> contentBounds)
		{
			FrameBuonds = frameBuonds;
			ContentBounds = contentBounds;
		}

		public override string ToString()
		{
			return "{Frame = "+FrameBuonds+", Content = "+ContentBounds+"}";
		}
	}
}