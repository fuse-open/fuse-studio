
using Outracks.Diagnostics;
using Outracks.Fusion;

namespace Outracks.Fuse
{
	public static class Spacer
	{
		public static IControl Line { get { return Control.Empty.WithSize(new Size<Points>(1, 1)); } }
		public static IControl Smaller { get { return Control.Empty.WithSize(new Size<Points>(3, 3)); } }
		public static IControl Small { get { return Control.Empty.WithSize(new Size<Points>(7, 7)); } }
		public static IControl Small2 { get { return Control.Empty.WithSize(new Size<Points>(9, 9)); } }
		public static IControl Medim { get { return Control.Empty.WithSize(new Size<Points>(13, 13)); } }
		public static IControl Medium { get { return Control.Empty.WithSize(new Size<Points>(15, 15)); } }

		public static IControl WithMediumPadding(this IControl control)
		{
			return control.WithPadding(new Thickness<Points>(15));
		}

		public static IControl WithInspectorPadding(this IControl control)
		{
			return control.WithPadding(new Thickness<Points>(15, 0, 16, 0));
		}

		public static IControl WithMacWindowStyleCompensation(this IControl control)
		{
			return control.WithPadding(new Thickness<Points>(0, Platform.IsMac ? 37 : 0, 0, 0));
		}
	}
}
