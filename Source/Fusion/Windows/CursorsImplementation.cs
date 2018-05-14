using System.Windows;

namespace Outracks.Fusion.Windows
{
	static class CursorsImplementation
	{
		static readonly System.Windows.Input.Cursor GrabCursor = null;
		static readonly System.Windows.Input.Cursor GrabbingCursor = null;

		static CursorsImplementation()
		{
			using (var stream = NativeResources.GetGrabCursor())
				GrabCursor = new System.Windows.Input.Cursor(stream);
			using (var stream = NativeResources.GetGrabbingCursor())
				GrabbingCursor = new System.Windows.Input.Cursor(stream);
		}

		public static void Initialize(Dispatcher dispatcher)
		{
			Cursors.Implementation.Set = (self, cursor) =>
			{
				self.BindNativeProperty(dispatcher, "cursor", cursor, (FrameworkElement e, Cursor t) => e.Cursor = t.ToWpf());
				return self;
			};
		}

		public static System.Windows.Input.Cursor ToWpf(this Cursor cursor)
		{
			switch (cursor)
			{
				case Cursor.Grab:
					return GrabCursor;
				case Cursor.Grabbing:
					return GrabbingCursor;
				case Cursor.Pointing:
					return System.Windows.Input.Cursors.Hand;
				case Cursor.ResizeHorizontally:
					return System.Windows.Input.Cursors.SizeWE;
				case Cursor.ResizeVertically:
					return System.Windows.Input.Cursors.SizeNS;
				case Cursor.Text:
					return System.Windows.Input.Cursors.IBeam;
				case Cursor.Normal:
				default:
					return System.Windows.Input.Cursors.Arrow;
			}
		}


	}
}