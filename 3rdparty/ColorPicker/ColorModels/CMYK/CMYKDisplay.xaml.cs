using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ColorPicker.ExtensionMethods;

namespace ColorPicker.ColorModels.CMYK
{
    /// <summary>
    /// Interaction logic for CMYKDisplay.xaml
    /// </summary>
    public partial class CMYKDisplay : UserControl
    {
        public enum EDisplayMode
        {
            ByteDisplay,
            PercentNoDecimal
        }
        private bool processEvents = true;
        private static CMYKModel sModel = new CMYKModel();
        private Func<Color, string> c;
        private Func<Color, string> m;
        private Func<Color, string> y;
        private Func<Color, string> k;

        private Color setColor = Colors.Black;
        public string CyanFormat { get; set; }
        public string MagentaFormat { get; set; }
        public string YellowFormat { get; set; }
        public string BlackFormat { get; set; }
        public CMYKDisplay()
        {
            InitializeComponent();
            c = color => sModel.CComponent(color, KGreedieness).ToString(CyanFormat);
            m = color => sModel.MComponent(color, KGreedieness).ToString(MagentaFormat);
            y = color => sModel.YComponent(color, KGreedieness).ToString(YellowFormat);
            k = color => sModel.KComponent(color, KGreedieness).ToString(BlackFormat);
            CyanFormat = "N0";
            MagentaFormat = "N0";
            YellowFormat = "N0";
            BlackFormat = "N0";
        }

        public static Type ClassType
        {
            get { return typeof(CMYKDisplay); }
        }
        #region Color

        public static DependencyProperty ColorProperty = DependencyProperty.Register("Color", typeof(Color), ClassType,
             new FrameworkPropertyMetadata(Colors.Black, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnColorChanged));


        [Category("ColorPicker")]
        public Color Color
        {
            get
            {
                return (Color)GetValue(ColorProperty);
            }
            set
            {
                SetValue(ColorProperty, value);
            }
        }

        private static void OnColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {       var color = (Color) e.NewValue;
                var display = (CMYKDisplay) d;

            if ((Color)e.NewValue != (Color)e.OldValue && display.setColor != color )
            {
                display.OnColorChanged(color);
            }
        }

        private void OnColorChanged(Color color)
        {
            displayComponents(color);
            if (ColorChanged != null)
            {
                ColorChanged(this, new EventArgs<Color>(color));
            }
        }

        #endregion

        #region DisplayMode

        private void displayComponents(Color color)
        {
              processEvents = false;
            txtC.Text = c(color);
            txtM.Text = m(color);
            txtY.Text = y(color);
            txtK.Text = k(color);
            processEvents =true;
        }

        public static DependencyProperty DisplayModeProperty = DependencyProperty.Register("DisplayMode", typeof(EDisplayMode), ClassType, new PropertyMetadata(EDisplayMode.ByteDisplay, new PropertyChangedCallback(OnDisplayModeChanged)));

        [Category("ColorPicker")]
        public EDisplayMode DisplayMode
        {
            get
            {
                return (EDisplayMode)GetValue(DisplayModeProperty);
            }
            set
            {
                SetValue(DisplayModeProperty, value);
            }
        }

        private static void OnDisplayModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var display = (CMYKDisplay)d;
            var displayMode = (EDisplayMode)e.NewValue;
            display.OnDisplayModeChanged(displayMode);

        }

        private void OnDisplayModeChanged(EDisplayMode displayMode)
        {
            switch (displayMode)
            {
                case EDisplayMode.ByteDisplay:
                    txtCUnit.Text = "";
                    txtMUnit.Text = "";
                    txtYUnit.Text = "";
                    txtKUnit.Text = "";
                    c = color => sModel.CComponent(color, KGreedieness).ToString(CyanFormat);
                    m = color => sModel.MComponent(color, KGreedieness).ToString(MagentaFormat);
                    y = color => sModel.YComponent(color, KGreedieness).ToString(YellowFormat);
                    k = color => sModel.KComponent(color, KGreedieness).ToString(BlackFormat);
                    break;
                case EDisplayMode.PercentNoDecimal:
                    txtCUnit.Text = "%";
                    txtMUnit.Text = "%";
                    txtYUnit.Text = "%";
                    txtKUnit.Text = "%";

                    c = color => sModel.CComponent(color, KGreedieness).PercentageOf(255).ToString(CyanFormat);
                    m = color => sModel.MComponent(color, KGreedieness).PercentageOf(255).ToString(MagentaFormat);
                    y = color => sModel.YComponent(color, KGreedieness).PercentageOf(255).ToString(YellowFormat);
                   k = color => sModel.KComponent(color, KGreedieness).PercentageOf(255).ToString(BlackFormat);
                    break;

            }
        }


        #endregion


        #region KGreedieness

public static DependencyProperty KGreedienessProperty   = DependencyProperty.Register("KGreedieness", typeof(double), ClassType,
    new PropertyMetadata(.7,new PropertyChangedCallback(OnKGreedienessChanged),CoerceKGreedieness));
  [Category("ColorPicker")]
public double KGreedieness
  {
    get
        {
            return (double)GetValue(KGreedienessProperty);
        }
        set
        {
            SetValue(KGreedienessProperty, value);
        }
    }

private static object CoerceKGreedieness(DependencyObject d, object value)
{
    
return value;

}

private static void OnKGreedienessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
{
 
}

#endregion


        public event EventHandler<EventArgs<Color>> ColorChanged;


        private void txtR_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = NumbersOnly(e.Text);
            base.OnPreviewTextInput(e);

        }

        private bool NumbersOnly(string text)
        {
            String okChars = "0123456789";
            return text.ToCharArray().All(c => okChars.IndexOf(c) == -1);
        }


        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            if (processEvents) { 
                try
                {
					setColor = sModel.Color(
						double.Parse(txtC.Text, CultureInfo.InvariantCulture),
						double.Parse(txtM.Text, CultureInfo.InvariantCulture),
						double.Parse(txtY.Text, CultureInfo.InvariantCulture),
						double.Parse(txtK.Text, CultureInfo.InvariantCulture));
                    Color = setColor;                                      
                }
                catch
                {
                }

            }
        }

    }
}
