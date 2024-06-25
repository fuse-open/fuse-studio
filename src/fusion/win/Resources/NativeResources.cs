using System.IO;

namespace Outracks.Fusion.Windows
{
	public static class NativeResources
	{
		public static Stream GetGrabCursor()
		{
			var assembly = typeof(NativeResources).Assembly;
			return assembly.GetManifestResourceStream("Outracks.Fusion.Windows.Resources.grab.cur");
		}

		public static Stream GetGrabbingCursor()
		{
			var assembly = typeof(NativeResources).Assembly;
			return assembly.GetManifestResourceStream("Outracks.Fusion.Windows.Resources.grabbing.cur");
		}

		public static System.Windows.Forms.Cursor GrabCursor { get; private set; }
		public static System.Windows.Forms.Cursor GrabbingCursor { get; private set; }

		public static void Load()
		{
			using (var stream = GetGrabCursor())
				GrabCursor = new System.Windows.Forms.Cursor(stream);

			using (var stream = GetGrabbingCursor())
				GrabbingCursor = new System.Windows.Forms.Cursor(stream);
		}
	}
}