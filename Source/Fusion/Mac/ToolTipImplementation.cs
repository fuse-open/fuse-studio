using System.Reactive.Concurrency;
using AppKit;

namespace Outracks.Fusion.OSX
{
	static class ToolTipImplementation
	{
		public static void Initialize(IScheduler dispatcher)
		{
			ToolTip.Implementation.Set = (self, toolTip) =>
			{
				self.BindNativeProperty(
					dispatcher,
					"tooltip", toolTip,
					(NSView view, string value) =>
					{
						if (value == null)
						{
							view.RemoveAllToolTips();
						}
						else
						{
							view.ToolTip = value;
						}
					});
				return self;
			};
		}
	}
}
