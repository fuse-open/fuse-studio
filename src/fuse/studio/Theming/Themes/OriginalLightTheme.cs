using System;
using System.Reactive.Linq;
using Outracks.Fusion;

namespace Outracks.Fuse.Theming.Themes
{
	internal class OriginalLightTheme : ITheme
	{
		public Color LineBrush
		{
			get { return Color.FromRgb(0xe0e0e0); }
		}

		public Brush WeakLineBrush
		{
			get { return Color.FromRgb(0xf1f1f1); }
		}

		public Brush Background
		{
			get { return Color.FromRgb(0x3F434B); }
		}

		public Brush PanelBackground
		{
			get { return Color.FromRgb(0xFCFCFC); }
		}

		public Brush WorskpaceBackground
		{
			get { return Color.FromRgb(0xF4F5F2); }
		}

		public Brush TopBarBackground
		{
			get { return Color.FromRgb(0x31343A); }
		}

		public Brush DefaultText
		{
			get { return Color.FromRgb(0x676767); }
		}

		public Brush DisabledText
		{
			get { return Color.FromRgb(0xc7c7c7); }
		}

		public Brush DescriptorText
		{
			get { return Color.FromRgb(0xa3a3a3); }
		}

		public Color Active
		{
			get { return Color.FromRgb(0x4953d7); }
		}

		public Brush ActiveHover
		{
			get { return Color.FromRgb(0x5460fa); }
		}

		public Brush Purple
		{
			get { return Color.FromRgb(0xfd71ff); }
		}

		public Brush Link
		{
			get { return Color.FromRgb(0x707aff); }
		}

		public Brush Margin
		{
			get { return Active; }
		}

		public Brush Padding
		{
			get { return Color.FromRgb(0xda394c); }
		}

		public Brush FieldBackground
		{
			get { return Color.White; }
		}

		public Stroke FieldStroke
		{
			get { return Stroke.Create(1, Color.FromRgb(0xE0E0E0)); }
		}

		public Stroke FieldFocusStroke
		{
			get { return Stroke.Create(1, Color.FromRgb(0x898e99)); }
		}

		public Stroke FieldErrorStroke
		{
			get { return Stroke.Create(1, Color.FromRgb(0xdd4a5c)); }
		}

		public Brush Shadow
		{
			get { return Color.FromRgb(0xcccccc); }
		}

		public Font DefaultFont
		{
			get
			{
				return Font.SystemDefault(size: 11.0);
			}
		}
		public Font DefaultFontBold
		{
			get
			{
				return Font.SystemDefault(size: 11.0, bold: true);
			}
		}

		public Font DescriptorFont
		{
			get
			{
				return Font.SystemDefault(size: 11.0);
			}
		}

		public Font HeaderFont
		{
			get
			{
				return Font.SystemDefault(size: 13.0);
			}
		}

		public Brush SplashTextColor
		{
			get { return Color.FromRgb(0x676767); }
		}

		public Brush SplashSelectColor
		{
			get { return Color.FromRgb(0xf2f2f2); }
		}

		public Color IconPrimary
		{
			get { return Active; }
		}

		public Color IconSecondary
		{
			get { return Color.FromRgb(0xc7c7c7); }
		}

		public Brush OutlineVerticalLineBackground
		{
			get { return Color.FromRgb(0xefefef); }
		}

		public Brush SplashBackground
		{
			get { return Background; }
		}

		public Brush SplashPanelBackground
		{
			get { return PanelBackground; }
		}

		public Brush SplashLink
		{
			get { return Link; }
		}

		public Brush SplashActive
		{
			get { return Color.FromRgb(0x6dc0d2); }
		}

		public Brush SwitchInactiveBackground
		{
			get { return FieldBackground; }
		}

		public Brush SwitchInactiveStroke
		{
			get { return FieldStroke.Brush; }
		}

		public Brush SwitchActiveBackground
		{
			get { return Active; }
		}

		public Brush SwitchActiveStroke
		{
			get { return Active; }
		}

		public Brush SwitchThumbFill
		{
			get { return PanelBackground; }
		}

		public Brush SwitchThumbStroke
		{
			get { return FieldStroke.Brush; }
		}

		public Brush OutlineIcon
		{
			get { return Color.FromRgb(0xc7c7c7); }
		}

		public Brush Cancel
		{
			get { return Color.FromRgb(0xE35667); }
		}
		public Brush FaintBackground
		{
			get { return Color.FromRgb(0xF7F7F7); }
		}
		public Brush ExtremelyFaintBackground
		{
			get { return Color.FromRgb(0xFAFAFA); }
		}

		public IObservable<IColorMap> IconColorMap { get { return Observable.Return(ColorMap.Singleton); } }
	}
}
