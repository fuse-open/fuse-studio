using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ColorPickerControls.Dialogs;
using ColorPicker;
namespace ColorPickerControls.Chips
{
    /// <summary>
    /// Interaction logic for ForegroundBackgroundChip.xaml
    /// </summary>
    public partial class ForegroundBackgroundChip : UserControl
    {
        public ForegroundBackgroundChip()
        {
            InitializeComponent();
            foreChip.ColorChanged += (s, ea) => ForegroundColor = ea.Value;
            backChip.ColorChanged += (s, ea) => BackgroundColor = ea.Value;
        }

        public event EventHandler<EventArgs<Color>> ForegroundColorChanged;
        public event EventHandler<EventArgs<SolidColorBrush>> ForegroundBrushChanged;
        public event EventHandler<EventArgs<Color>> BackgroundColorChanged;
        public event EventHandler<EventArgs<SolidColorBrush>> BackgroundBrushChanged;

        public static Type ClassType
        {
            get { return typeof(ForegroundBackgroundChip); }
        }

        #region DefaultForeground

        public static DependencyProperty DefaultForegroundProperty = DependencyProperty.Register("DefaultForeground", typeof(Color), ClassType, new PropertyMetadata(Colors.Black , OnDefaultForegroundChanged));
        [Category("ColorPicker")]
        public Color DefaultForeground
        {
            get
            {
                return (Color)GetValue(DefaultForegroundProperty);
            }
            set
            {
                SetValue(DefaultForegroundProperty, value);
            }
        }

        private static void OnDefaultForegroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var chip = (ForegroundBackgroundChip)d;
            chip.rForegroundDefault.Fill = new SolidColorBrush((Color)e.NewValue);
        }

        #endregion

        #region DefaultBackground

        public static DependencyProperty DefaultBackgroundProperty = DependencyProperty.Register("DefaultBackground", typeof(Color), ClassType, 
            new PropertyMetadata(Colors.White ,  OnDefaultBackgroundChanged));
           

        [Category("ColorPicker")]
        public Color DefaultBackground
        {
            get
            {
                return (Color)GetValue(DefaultBackgroundProperty);
            }
            set
            {
                SetValue(DefaultBackgroundProperty, value);
            }
        }

        private static void OnDefaultBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var chip = (ForegroundBackgroundChip) d;
            chip.rBackgroundDefault.Fill = new SolidColorBrush((Color)e.NewValue);
        }

        #endregion

        #region ForegroundColor

        public static DependencyProperty ForegroundColorProperty = DependencyProperty.Register("ForegroundColor", typeof(Color), ClassType, new PropertyMetadata(Colors.Black, OnForegroundColorChanged));
         [Category("ColorPicker")]
        public Color ForegroundColor
        {
            get
            {
                return (Color)GetValue(ForegroundColorProperty);
            }
            set
            {
                SetValue(ForegroundColorProperty, value);
            }
        }

        private static void OnForegroundColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var chip = (ForegroundBackgroundChip) d;
            chip.foreChip.Color =(Color) e.NewValue;
            chip.ForegroundBrush = new SolidColorBrush(chip.foreChip.Color);
            if (chip.ForegroundColorChanged != null)
            {
                chip.ForegroundColorChanged(chip, new EventArgs<Color>(chip.foreChip.Color));
            }
        }

        #endregion


        #region BackgroundColor

        public static DependencyProperty BackgroundColorProperty = DependencyProperty.Register("BackgroundColor", typeof(Color), ClassType, new PropertyMetadata(Colors.Black, OnBackgroundColorChanged));
         [Category("ColorPicker")]
        public Color BackgroundColor
        {
            get
            {
                return (Color)GetValue(BackgroundColorProperty);
            }
            set
            {
                SetValue(BackgroundColorProperty, value);
            }
        }

        private static void OnBackgroundColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var chip = (ForegroundBackgroundChip)d;
            chip.backChip.Color = (Color)e.NewValue;
            chip.BackgroundBrush = new SolidColorBrush(chip.backChip.Color);
            if (chip.BackgroundColorChanged != null)
            {
                chip.BackgroundColorChanged(chip, new EventArgs<Color>(chip.backChip.Color));
            }
        }

        #endregion

        #region BackgroundBrush

        public static DependencyProperty BackgroundBrushProperty = DependencyProperty.Register("BackgroundBrush", typeof(SolidColorBrush), ClassType, 
            new PropertyMetadata(new SolidColorBrush(Colors.White ), OnBackgroundBrushChanged));

        public SolidColorBrush BackgroundBrush
        {
            get
            {
                return (SolidColorBrush)GetValue(BackgroundBrushProperty);
            }
            set
            {
                SetValue(BackgroundBrushProperty, value);
            }
        }

        private static void OnBackgroundBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var chip = (ForegroundBackgroundChip)d;
            if (chip.BackgroundBrushChanged != null)
            {
                chip.BackgroundBrushChanged(chip, new EventArgs<SolidColorBrush>(chip.backChip.Brush));
            }

        }

        #endregion

        #region ForegroundBrush

        public static DependencyProperty ForegroundBrushProperty = DependencyProperty.Register("ForegroundBrush", typeof(SolidColorBrush), ClassType, 
            new PropertyMetadata(new SolidColorBrush(Colors.Black ), OnForegroundBrushChanged));
          [Category("ColorPicker")]
        public SolidColorBrush ForegroundBrush
        {
            get
            {
                return (SolidColorBrush)GetValue(ForegroundBrushProperty);
            }
            set
            {
                SetValue(ForegroundBrushProperty, value);
            }
        }

        private static void OnForegroundBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var chip = (ForegroundBackgroundChip)d;
            if (chip.ForegroundBrushChanged != null)
            {
                chip.ForegroundBrushChanged(chip, new EventArgs<SolidColorBrush>(chip.foreChip.Brush));
            }
        }

        #endregion




        private EColorDialog mColorDialog = EColorDialog.Full;
        [Category("ColorPicker")]
        public EColorDialog ColorDialog
        {
            get { return mColorDialog; }
            set { mColorDialog = value;
                foreChip.ColorDialog = value;
                backChip.ColorDialog = value;
            }
        }

        private void rForegroundDefault_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ForegroundColor = DefaultForeground;
            BackgroundColor = DefaultBackground;
        }

        private void rBackgroundDefault_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ForegroundColor = DefaultForeground;
            BackgroundColor = DefaultBackground;
        }

        private void rSwitch_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var color = ForegroundColor;
            ForegroundColor = BackgroundColor;
            BackgroundColor = color;
        }

      

        



    }
}
