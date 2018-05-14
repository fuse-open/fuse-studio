using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Outracks.Fusion
{
	public static class LayoutTracker
	{
		public static IControl Create(Func<ILayoutTracker, IControl> content)
		{
			return Implementation(content);
		}

		public static Func<Func<ILayoutTracker,IControl>,IControl> Implementation;
	}

	public interface ILayoutTracker
	{
		/// <summary>
		/// Wraps the content parameter to notify the frame parameter of where 
		/// it is on the screen, relative to this LayoutTracker
		/// </summary>
		IControl TrackVisualBounds(Action<Rectangle<Points>> frame, IControl content);
	}
}