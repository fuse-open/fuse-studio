using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ColorPicker.ColorModels.HSB
{
    /// <summary>
    /// Interaction logic for HsbDisplay.xaml
    /// </summary>
    public partial class HsbDisplay : UserControl
    {
        private static HSBModel sModel = new HSBModel();
        private static Hue sHue = new Hue();
        private static Brightness sBrightness = new Brightness();
        private static Saturation sSaturation = new Saturation();

        public event EventHandler<EventArgs<NormalComponent>> ColorComponentChanged;
        public static Type ClassType
        {
            get { return typeof(HsbDisplay); }
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
        {
            var c = (Color)e.NewValue;
            var rd = (HsbDisplay)d;
            rd.OnColorChanged(c);
        }

        private void OnColorChanged(Color c)
        {
            txtH.Text = sHue.Value(c).ToString(CultureInfo.InvariantCulture);
			txtS.Text = sSaturation.Value(c).ToString(CultureInfo.InvariantCulture);
			txtB.Text = sBrightness.Value(c).ToString(CultureInfo.InvariantCulture);

            if (ColorChanged != null)
            {
                ColorChanged(this, new EventArgs<Color>(c));
            }
        }

        #endregion

        #region NormalComponent

        public static DependencyProperty NormalComponentProperty = DependencyProperty.Register("NormalComponent", typeof(NormalComponent), ClassType,
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnColorComponentChanged));

        [Category("ColorPicker")]
        public NormalComponent NormalComponent
        {
            get
            {
                return (NormalComponent)GetValue(NormalComponentProperty);
            }
            set
            {
                SetValue(NormalComponentProperty, value);
            }
        }

        private static void OnColorComponentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                var cp = (NormalComponent)e.NewValue;
               
                var rd = (HsbDisplay)d;
                rd.OnColorComponentChanged(cp);
            }
            catch
            {
            }
        }

        private void OnColorComponentChanged(NormalComponent colorPlaneColorComponent)
        {

            if (colorPlaneColorComponent.Name == "HSB_Hue")
            {
                rH.IsChecked = true;
            }
            else if (colorPlaneColorComponent.Name == "HSB_Saturation")
            {
                rS.IsChecked = true;
            }
            else if (colorPlaneColorComponent.Name == "HSB_Brightness")
            {
                rB.IsChecked = true;
            }
            else
            {
                rH.IsChecked = false;
                rS.IsChecked = false;
                rB.IsChecked = false;
            }

            if (ColorComponentChanged != null)
            {
                ColorComponentChanged(this, new EventArgs<NormalComponent>(colorPlaneColorComponent));
            }



        }

        #endregion

        public event EventHandler<EventArgs<Color>> ColorChanged;


        public HsbDisplay()
        {
            InitializeComponent();
		}


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
	        if (string.IsNullOrWhiteSpace(txtH.Text) ||
		        string.IsNullOrWhiteSpace(txtS.Text) ||
		        string.IsNullOrWhiteSpace(txtB.Text))
	        {
		        return;
	        }

	        try
            {
                Color = sModel.Color(
					Convert.ToDouble(txtH.Text, CultureInfo.InvariantCulture),
					Convert.ToDouble(txtS.Text, CultureInfo.InvariantCulture) / 100,
					Convert.ToDouble(txtB.Text, CultureInfo.InvariantCulture) / 100);
            }
            catch
            {
            }
        }

        private void rH_Checked(object sender, RoutedEventArgs e)
        {
            NormalComponent = sHue;
        }

        private void rS_Checked(object sender, RoutedEventArgs e)
        {
            NormalComponent = sSaturation;
        }

        private void rB_Checked(object sender, RoutedEventArgs e)
        {
            NormalComponent = sBrightness;
        }
    }


}
