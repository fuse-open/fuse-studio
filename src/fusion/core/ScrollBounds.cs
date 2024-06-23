using System.Collections.Generic;

namespace Outracks.Fusion
{
	public class ScrollBounds
	{
		readonly Rectangle<Points> _visible;

		public Rectangle<Points> Visible
		{
			get { return _visible; }
		}

		readonly Rectangle<Points> _content;

		public Rectangle<Points> Content
		{
			get { return _content; }
		}

		public ScrollBounds(Rectangle<Points> visible, Rectangle<Points> content)
		{
			_visible = visible;
			_content = content;
		}

		public static ScrollBounds Zero
		{
			get { return new ScrollBounds(Rectangle.Zero<Points>(), Rectangle.Zero<Points>()); }
		}

		public override bool Equals(object obj)
		{
			var bounds = obj as ScrollBounds;
			return bounds != null &&
				   _visible.Equals(bounds._visible) &&
				   _content.Equals(bounds._content);
		}

		public override int GetHashCode()
		{
			var hashCode = -1109539441;
			hashCode = hashCode * -1521134295 + EqualityComparer<Rectangle<Points>>.Default.GetHashCode(_visible);
			hashCode = hashCode * -1521134295 + EqualityComparer<Rectangle<Points>>.Default.GetHashCode(_content);
			return hashCode;
		}
	}
}