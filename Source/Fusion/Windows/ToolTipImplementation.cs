using System.Windows;

namespace Outracks.Fusion.Windows
{
	static class ToolTipImplementation
	{
		public static void Initialize(Dispatcher dispatcher)
		{
			ToolTip.Implementation.Set = (self, toolTip) =>
			{
				self.BindNativeProperty(dispatcher, "tooltip", toolTip, (FrameworkElement element, string value) =>
					element.ToolTip = 
						string.IsNullOrEmpty(value) 
							? null 
							: new System.Windows.Controls.ToolTip { Content = value });

				return self;
			};
		}
	}
}
