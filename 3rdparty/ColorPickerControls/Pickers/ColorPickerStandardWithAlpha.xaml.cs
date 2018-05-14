using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ColorPicker.ExtensionMethods;
using ColorPicker;
namespace ColorPickerControls.Pickers
{
    /// <summary>
    /// Interaction logic for ColorPickerFullWithAlpha.xaml
    /// </summary>
    public partial class ColorPickerStandardWithAlpha : UserControl
    {
        public ColorPickerStandardWithAlpha()
        {
            InitializeComponent();
            colorSelector.ColorChanged += colorSelector_ColorChanged;

            alphaDisplay.AlphaChanged += alphaDisplay_AlphaChanged;
        }

        void alphaDisplay_AlphaChanged(object sender, ColorPicker.EventArgs<byte> e)
        {
            SetValue(SelectedColorProperty,SelectedColor.WithAlpha(e.Value));
            if (SelectedColorChanged != null)
            {
                SelectedColorChanged(this, new EventArgs<Color>(SelectedColor));
            }
        }

        void colorSelector_ColorChanged(object sender, ColorPicker.EventArgs<Color> e)
        {
            SetValue(SelectedColorProperty, e.Value.WithAlpha(alphaDisplay.Alpha ));
            if (SelectedColorChanged != null)
            {
                SelectedColorChanged(this, new EventArgs<Color>(SelectedColor));
            }
        }

        public static Type ClassType
        {
            get { return typeof(ColorPickerStandardWithAlpha); }
        }
        #region InitialColor

        public static DependencyProperty InitialColorProperty = DependencyProperty.Register("InitialColor", typeof(Color), ClassType,
            new FrameworkPropertyMetadata(Colors.Transparent, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnInitialColorChanged));
        [Category("ColorPicker")]
        public Color InitialColor
        {
            get
            {
                return (Color)GetValue(InitialColorProperty);
            }
            set
            {
                SetValue(InitialColorProperty, value);
            }
        }


        private static void OnInitialColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var cpf = (ColorPickerStandardWithAlpha)d;
            cpf.newCurrent.CurrentColor = (Color)e.NewValue;
            if (cpf.SelectedColorChanged != null)
            {
                cpf.SelectedColorChanged(cpf, new EventArgs<Color>((Color)e.NewValue));
            }
        }


        #endregion

        public event EventHandler<EventArgs<Color>> SelectedColorChanged;
        #region SelectedColor

        public static DependencyProperty SelectedColorProperty = DependencyProperty.Register("SelectedColor", typeof(Color), ClassType,
            new FrameworkPropertyMetadata(Colors.Transparent, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedColorChanged));
        [Category("ColorPicker")]
        public Color SelectedColor
        {
            get
            {
                return (Color)GetValue(SelectedColorProperty);
            }
            set
            {
                SetValue(SelectedColorProperty, value);
            }
        }


        private static void OnSelectedColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var cpf = (ColorPickerStandardWithAlpha)d;
            var color =  (Color)e.NewValue;
            cpf.colorSelector.Color = color;
            cpf.alphaDisplay.Alpha = color.A;

        }


        #endregion

         [Category("ColorPicker")]
        public ColorSelector.ESelectionRingMode SelectionRingMode
        {
            get { return colorSelector.SelectionRingMode; }
            set { colorSelector.SelectionRingMode = value; }
        }
    }
}
