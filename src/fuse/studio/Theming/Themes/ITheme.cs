using System;
using Outracks.Fusion;

namespace Outracks.Fuse.Theming.Themes
{
	internal interface ITheme
	{
		Color LineBrush { get; }
		Brush WeakLineBrush { get; }
		Brush Background { get; }
		Brush PanelBackground { get; }
		Brush WorskpaceBackground { get; }
		Brush TopBarBackground { get; }
		Brush DefaultText { get; }
		Brush DisabledText { get; }
		Brush DescriptorText { get; }
		Color Active { get; }
		Brush ActiveHover { get; }
		Brush Purple { get; }
		Brush Link { get; }
		Brush Margin { get; }
		Brush Padding { get; }
		Brush FieldBackground { get; }
		Stroke FieldStroke { get; }
		Stroke FieldFocusStroke { get; }
		Stroke FieldErrorStroke { get; }
		Brush Shadow { get; }
		Font DefaultFont { get; }
		Font DefaultFontBold { get; }
		Font DescriptorFont { get; }
		Font HeaderFont { get; }
		Brush SplashTextColor { get; }
		Brush SplashSelectColor { get; }
		Color IconPrimary { get; }
		Color IconSecondary { get; }
		Brush OutlineVerticalLineBackground { get; }
		Brush SplashBackground { get; }
		Brush SplashPanelBackground { get; }
		Brush SplashLink { get; }
		Brush SplashActive { get; }
		Brush SwitchInactiveBackground { get; }
		Brush SwitchInactiveStroke { get; }
		Brush SwitchActiveBackground { get; }
		Brush SwitchActiveStroke { get; }
		Brush SwitchThumbFill { get; }
		Brush SwitchThumbStroke { get; }
		Brush OutlineIcon { get; }
		Brush Cancel { get; }
		Brush FaintBackground { get; }
		Brush ExtremelyFaintBackground { get; }
		IObservable<IColorMap> IconColorMap { get; }
	}
}