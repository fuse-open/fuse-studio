using System;
using System.Reactive.Linq;
using Outracks.Fuse.Theming.Themes;
using Outracks.Fusion;

namespace Outracks.Fuse
{
	public static class Theme
	{
		public static readonly IProperty<Themes> CurrentTheme =
			UserSettings
				.Enum<Themes>("Theme")
				.Or(Themes.OriginalDark)
				.AutoInvalidate();

		public static Stroke SelectionStroke(IObservable<bool> isSelected, IObservable<bool> isHovering, IObservable<bool> showOutline)
		{
			return Stroke.Create(
				isSelected.CombineLatest(isHovering, showOutline, 
					(selected, hovering, outline) => 
						(hovering || selected ? 1.0 : 0.0) + 
						(outline ? 1.0 : 0.0)),
				Active,
				isSelected.Select(selected => selected ? StrokeDashArray.Solid : new StrokeDashArray(4,4)));
		}

		public static IControl Header(string text, IObservable<bool> isDisabled = null)
		{
			isDisabled = isDisabled ?? Observable.Return(false);
			return Label
				.Create(text, font: DefaultFont, color: isDisabled.Select(d => d ? DisabledText : DefaultText).Switch())
				.CenterVertically();
		}


		public static Brush IconBrush
		{
			get { return Color.FromRgb(0x838994); }
		}

		public static Brush NotificationBrush
		{
			get { return Color.FromRgb(0xE35667); }
		}
		public static Brush NotificationBarBackground
		{
			get { return Color.FromRgb(0x2377B3); }
		}

		public static Brush NotificationBarForground
		{
			get { return Color.White; }
		}
		public static Brush ErrorColor
		{
			get { return Color.FromRgb(0xBF504C); }
		}

		public static Brush BuildBarBackground
		{
			get { return Color.FromRgb(0x8374D4); }
		}

		public static Brush BuildBarForeground
		{
			get { return Color.White; }
		}

		public static Brush ReifyBarBackground
		{
			get { return Color.FromRgb(0x6CC4B4); }
		}

		public static Brush ReifyBarForeground
		{
			get { return Color.White; }
		}

		public static IObservable<bool> IsDark
		{
			get { return CurrentTheme.Select(t => t == Themes.OriginalDark).Replay(1).RefCount(); }
		}

		static Theme()
		{
			var themeInstance = 
				CurrentTheme.Select(theme =>
				{
					switch (theme)
					{
						case Themes.OriginalLight: 
							return (ITheme)new OriginalLightTheme();

						case Themes.OriginalDark:
						default:
							return (ITheme)new OriginalDarkTheme();
					}
				})
				.Replay(1).RefCount();

			LineBrush = themeInstance.Select(x => (Brush)x.LineBrush).Switch();
			WeakLineBrush = themeInstance.Select(x => x.WeakLineBrush).Switch();
			Background = themeInstance.Select(x => x.Background).Switch();
			PanelBackground = themeInstance.Select(x => x.PanelBackground).Switch();
			WorkspaceBackground = themeInstance.Select(x => x.WorskpaceBackground).Switch(); 
			TopBarBackground = themeInstance.Select(x => x.TopBarBackground).Switch(); 
			DefaultText = themeInstance.Select(x => x.DefaultText).Switch(); 
			DisabledText = themeInstance.Select(x => x.DisabledText).Switch(); 
			DescriptorText = themeInstance.Select(x => x.DescriptorText).Switch();
			Active = themeInstance.Select(x => x.Active).AsBrush();
			ActiveHover = themeInstance.Select(x => x.ActiveHover).Switch(); 
			Purple = themeInstance.Select(x => x.Purple).Switch(); 
			Link = themeInstance.Select(x => x.Link).Switch(); 
			Margin =  themeInstance.Select(x => x.Margin).Switch(); 
			Padding = themeInstance.Select(x => x.Padding).Switch(); 
			FieldBackground = themeInstance.Select(x => x.FieldBackground).Switch(); 
			FieldStroke =  themeInstance.Select(x => x.FieldStroke).Switch(); 
			FieldFocusStroke = themeInstance.Select(x => x.FieldFocusStroke).Switch();
			FieldErrorStroke = themeInstance.Select(x => x.FieldErrorStroke).Switch(); 
			Shadow = themeInstance.Select(x => x.Shadow).Switch(); 
			DefaultFont = themeInstance.Select(x => x.DefaultFont).Switch(); 
			DefaultFontBold = themeInstance.Select(x => x.DefaultFontBold).Switch();
			DescriptorFont = themeInstance.Select(x => x.DescriptorFont).Switch(); 
			HeaderFont = themeInstance.Select(x => x.HeaderFont).Switch(); 
			SplashTextColor = themeInstance.Select(x => x.SplashTextColor).Switch(); 
			SplashSelectColor = themeInstance.Select(x => x.SplashSelectColor).Switch();
			IconPrimary = themeInstance.Select(x => x.IconPrimary).AsBrush(); 
			IconSecondary = themeInstance.Select(x => x.IconSecondary).AsBrush();
			OutlineVerticalLineBackground = themeInstance.Select(x => x.OutlineVerticalLineBackground).Switch();
			SplashBackground = themeInstance.Select(x => x.SplashBackground).Switch();
			SplashPanelBackground = themeInstance.Select(x => x.SplashPanelBackground).Switch();
			SplashLink = themeInstance.Select(x => x.SplashLink).Switch(); 
			SplashActive = themeInstance.Select(x => x.SplashActive).Switch();
			SwitchInactiveBackground = themeInstance.Select(x => x.SwitchInactiveBackground).Switch();
			SwitchInactiveStroke = themeInstance.Select(x => x.SwitchInactiveStroke).Switch();
			SwitchActiveBackground = themeInstance.Select(x => x.SwitchActiveBackground).Switch();
			SwitchActiveStroke = themeInstance.Select(x => x.SwitchActiveStroke).Switch();
			SwitchThumbFill = themeInstance.Select(x => x.SwitchThumbFill).Switch();
			SwitchThumbStroke =  themeInstance.Select(x => x.SwitchThumbStroke).Switch();
			OutlineIcon = themeInstance.Select(x => x.OutlineIcon).Switch();
			Cancel = themeInstance.Select(x => x.Cancel).Switch();
			FaintBackground = themeInstance.Select(x => x.FaintBackground).Switch();
			ExtremelyFaintBackground = themeInstance.Select(x => x.FaintBackground).Switch();
			IconColorMap = themeInstance.Select(x => x.IconColorMap).Switch();
		}

		public static Brush LineBrush { get; private set; }
		public static Brush WeakLineBrush { get; private set; }
		public static Brush Background { get; private set; }
		public static Brush PanelBackground { get; private set; }
		public static Brush WorkspaceBackground { get; private set; }
		public static Brush TopBarBackground { get; private set; }
		public static Brush DefaultText { get; private set; }
		public static Brush DisabledText { get; private set; }
		public static Brush DescriptorText { get; private set; }
		public static Brush Active { get; private set; }
		public static Brush ActiveHover { get; private set; }
		public static Brush Purple { get; private set; }
		public static Brush Link { get; private set; }
		public static Brush Margin { get; private set; }
		public static Brush Padding { get; private set; }
		public static Brush FieldBackground { get; private set; }
		public static Stroke FieldStroke { get; private set; }
		public static Stroke FieldFocusStroke { get; private set; }
		public static Stroke FieldErrorStroke { get; private set; }
		public static Brush Shadow { get; private set; }
		public static Font DefaultFont { get; private set; }
		public static Font DefaultFontBold { get; private set; }
		public static Font DescriptorFont { get; private set; }
		public static Font HeaderFont { get; private set; }
		public static Brush SplashTextColor { get; private set; }
		public static Brush SplashSelectColor { get; private set; }
		public static Brush IconPrimary { get; private set; }
		public static Brush IconSecondary { get; private set; }
		public static Brush OutlineVerticalLineBackground { get; private set; }
		public static Brush SplashBackground { get; private set; }
		public static Brush SplashPanelBackground { get; private set; }
		public static Brush SplashLink { get; private set; }
		public static Brush SplashActive { get; private set; }
		public static Brush SwitchInactiveBackground { get; private set; }
		public static Brush SwitchInactiveStroke { get; private set; }
		public static Brush SwitchActiveBackground { get; private set; }
		public static Brush SwitchActiveStroke { get; private set; }
		public static Brush SwitchThumbFill { get; private set; }
		public static Brush SwitchThumbStroke { get; private set; }
		public static Brush OutlineIcon { get; private set; }
		public static Brush Cancel { get; private set; }
		public static Brush FaintBackground { get; private set; }
		public static Brush ExtremelyFaintBackground { get; private set; }
		public static IObservable<IColorMap> IconColorMap { get; private set; }
	}
}