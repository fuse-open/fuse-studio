using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ColorPicker.ColorModels.Lab
{
    /// <summary>
    /// Interaction logic for LabDisplay.xaml
    /// </summary>
    public partial class LabDisplay : UserControl
    {
        private bool mProcessEvents = true;

        private static LabModel sModel = new LabModel();
        private static Lightness sLightness = new Lightness();
        private static A sA = new A();
        private static B sB = new B();
        public static Type ClassType
        {
            get { return typeof(LabDisplay); }
        }
        public LabDisplay()
        {
            InitializeComponent();
            l = color => sModel.LComponent(color).ToString(LFormat);
            a = color => sModel.AComponent(color).ToString(AFormat);
            b = color => sModel.BComponent(color).ToString(BFormat);
            LFormat = "N0";
            AFormat = "N0";
            BFormat = "N0";
        }

        private Func<Color, string> l;
        private Func<Color, string> a;
        private Func<Color, string> b;
        public string LFormat { get; set; }
        public string AFormat { get; set; }
        public string BFormat { get; set; }

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
            var color = (Color)e.NewValue;
            var display = (LabDisplay)d;
            display.OnColorChanged(color);
        }

        private void OnColorChanged(Color color)
        {
            mProcessEvents = false;
            txtL.Text = l(color);
            txtA.Text = a(color);
            txtB.Text = b(color);
            mProcessEvents = true;

            if (ColorChanged != null)
            {
                ColorChanged(this, new EventArgs<Color>(color));
            }
        }

        #endregion
        public event EventHandler<EventArgs<Color>> ColorChanged;


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
                var rd = (LabDisplay)d;
                rd.OnColorComponentChanged(cp);
            }
            catch
            {
            }
        }

        #endregion

        private void OnColorComponentChanged(NormalComponent colorPlaneColorComponent)
        {

            if (colorPlaneColorComponent.Name == "LAB_Lightness")
            {
                rL.IsChecked = true;
            }
            else if (colorPlaneColorComponent.Name == "LAB_A")
            {
                rA.IsChecked = true;
            }
            else if (colorPlaneColorComponent.Name == "LAB_B")
            {
                rB.IsChecked = true;
            }
            else
            {
                rL.IsChecked = false;
                rA.IsChecked = false;
                rB.IsChecked = false;
            }

         


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
            if (mProcessEvents)
            {
                try
                {
                    Color = sModel.Color(
						Convert.ToDouble(txtL.Text, CultureInfo.InvariantCulture),
						Convert.ToDouble(txtA.Text, CultureInfo.InvariantCulture),
						Convert.ToDouble(txtB.Text, CultureInfo.InvariantCulture));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        private void rL_Checked(object sender, RoutedEventArgs e)
        {
            NormalComponent = sLightness;
        }

        private void rA_Checked(object sender, RoutedEventArgs e)
        {
            NormalComponent = sA;
        }

        private void rB_Checked(object sender, RoutedEventArgs e)
        {
            NormalComponent = sB;
        }
    }
}
