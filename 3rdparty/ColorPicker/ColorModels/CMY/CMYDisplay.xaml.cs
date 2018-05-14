using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ColorPicker.ExtensionMethods;

namespace ColorPicker.ColorModels.CMY
{
    /// <summary>
    /// Interaction logic for CMYDisplay.xaml
    /// </summary>
    public partial class CMYDisplay : UserControl
    {
        public enum EDisplayMode
        {
            ByteDisplay,
            PercentNoDecimal
        }

        private static CMYModel sModel = new CMYModel();
        private static Cyan sCyan = new Cyan();
        private static Magenta sMagenta = new Magenta();
        private static Yellow sYellow = new Yellow();
        private Func<Color, string> c;
        private Func<Color, string> m;
        private Func<Color, string> y;
         
        public CMYDisplay()
        {
            InitializeComponent();
            txtCUnit.Text = "";
            txtMUnit.Text = "";
            txtYUnit.Text = "";
			c = color => sCyan.Value(color).ToString(CultureInfo.InvariantCulture);
			m = color => sMagenta.Value(color).ToString(CultureInfo.InvariantCulture);
			y = color => sYellow.Value(color).ToString(CultureInfo.InvariantCulture);
            CyanFormat = "N0";
           MagentaFormat = "N0";
            YellowFormat = "N0";
        }

        public static Type ClassType
        {
            get { return typeof(CMYDisplay); 
            }
        }


        public string CyanFormat { get; set; }
        public string MagentaFormat { get; set; }
        public string YellowFormat { get; set; }

        #region DisplayMode

        public static DependencyProperty DisplayModeProperty = DependencyProperty.Register("DisplayMode", typeof(EDisplayMode), ClassType, new PropertyMetadata(EDisplayMode.ByteDisplay, new PropertyChangedCallback(OnDisplayModeChanged)));

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
            var display = (CMYDisplay)d;
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
					c = color => sCyan.Value(color).ToString(CultureInfo.InvariantCulture);
					m = color => sMagenta.Value(color).ToString(CultureInfo.InvariantCulture);
					y = color => sYellow.Value(color).ToString(CultureInfo.InvariantCulture);
                    break;
                case EDisplayMode.PercentNoDecimal:
                    txtCUnit.Text = "%";
                    txtMUnit.Text = "%";
                    txtYUnit.Text = "%";

                    c = color => sModel.CComponent(color).PercentageOf(255).ToString(CyanFormat);
                    m = color => sModel.MComponent(color).PercentageOf(255).ToString(MagentaFormat);
                    y = color => sModel.YComponent(color).PercentageOf(255).ToString(YellowFormat);
                    break;

            }
        }

        
        #endregion
        #region Color

        public static DependencyProperty ColorProperty = DependencyProperty.Register("Color", typeof(Color), ClassType,
             new FrameworkPropertyMetadata(Colors.Black, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnColorChanged));

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
        {
            var color = (Color)e.NewValue;
            var display = (CMYDisplay)d;
            display.OnColorChanged(color);
        }

        private void OnColorChanged(Color color)
        {
            txtC.Text = c(color) ;
            txtM.Text = m(color);
            txtY.Text = y(color);
             

            if (ColorChanged != null)
            {
                ColorChanged(this, new EventArgs<Color>(color));
            }
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
            try
            {
                //Color = System.Windows.Media.Color.FromRgb(byte.Parse(txtR.Text), byte.Parse(txtG.Text),
                //                                           byte.Parse(txtB.Text));
            }
            catch
            {
            }
        }
    }
}
