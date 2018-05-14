using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ColorPicker.ExtensionMethods;

namespace ColorPicker.ColorModels.RGB
{
    /// <summary>
    /// Interaction logic for RgbDisplay.xaml
    /// </summary>
    public partial class RgbDisplay : UserControl
    {
        public enum EDisplayMode
        {
            ByteDisplay,
            PercentNoDecimal
        }

        private bool processEvents = true;

        private Func< Color,string> r;
        private Func<Color, string> g;
        private Func<Color, string> b;

        private Func<Color, byte, Color> setR;
        private Func<Color, byte, Color> setG;
        private Func<Color, byte, Color> setB;

        public static Type ClassType
        {
            get { return typeof (RgbDisplay); }
        }
        public RgbDisplay()
        {
            InitializeComponent();
			r = c => c.R.ToString(CultureInfo.InvariantCulture);
			g = c => c.G.ToString(CultureInfo.InvariantCulture);
			b = c => c.B.ToString(CultureInfo.InvariantCulture);


            setR = (c, newR) => c.WithR(newR);
            setG = (c, newG) => c.WithG(newG);
            setB = (c, newB) => c.WithB(newB);
        }




        private static  Red sRed = new Red();
        private static Green sGreen = new Green();
        private static Blue sBlue = new Blue();

        public event EventHandler<EventArgs<Color>> ColorChanged;
        
        public event EventHandler<EventArgs<NormalComponent>> ColorComponentChanged;


        #region DisplayMode

        public static DependencyProperty DisplayModeProperty = DependencyProperty.Register("DisplayMode", typeof(EDisplayMode), ClassType, new PropertyMetadata(EDisplayMode.ByteDisplay, OnDisplayModeChanged));

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
            var display = (RgbDisplay) d;
            var displayMode = (EDisplayMode) e.NewValue;
            display.OnDisplayModeChanged(displayMode);

        }

        private void OnDisplayModeChanged( EDisplayMode displayMode)
        {
           switch (displayMode)
           {
               case EDisplayMode.ByteDisplay :
                   txtRUnit.Text  = "";
                   txtGUnit.Text = "";
                   txtBUnit.Text = "";

				   r = c => c.R.ToString(CultureInfo.InvariantCulture);
				   g = c => c.G.ToString(CultureInfo.InvariantCulture);
				   b = c => c.B.ToString(CultureInfo.InvariantCulture);

                   setR = (c, newR) => c.WithR(newR);
                   setG = (c, newG) => c.WithG(newG);
                   setB = (c, newB) => c.WithB(newB);

                   break;
               case EDisplayMode.PercentNoDecimal :
                   txtRUnit.Text  = "%";
                   txtGUnit.Text = "%";
                   txtBUnit.Text = "%";

				   r = c => c.R.AsPercent().ToString(CultureInfo.InvariantCulture);
				   g = c => c.G.AsPercent().ToString(CultureInfo.InvariantCulture);
				   b = c => c.B.AsPercent().ToString(CultureInfo.InvariantCulture);

                  setR = (c, newR) => c.WithR(FromPercent(newR));
                   setG = (c, newG) => c.WithG(FromPercent(newG));
                   setB = (c, newB) => c.WithB(FromPercent(newB));
                   break;

           }
                   processEvents = false;
                   txtR.Text = r(Color);
                   txtG.Text = g(Color);
                   txtB.Text = b(Color);
                   processEvents = true;
        }

        #endregion

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
            var c = (Color) e.NewValue;
            var rd = (RgbDisplay) d;
            rd.OnColorChanged(c);
        }

        private void OnColorChanged(Color c)
        {
            
           processEvents = false;
           txtR.Text = r(c);
           txtG.Text = g(c);
           txtB.Text = b(c);
           processEvents = true;
            if (ColorChanged != null)
            {
                ColorChanged(this, new EventArgs<Color>(c));
            }
        }

        #endregion


        #region NormalComponent

        public static DependencyProperty NormalComponentProperty = DependencyProperty.Register("NormalComponent", typeof(NormalComponent), ClassType,
            new FrameworkPropertyMetadata(sRed, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,OnColorComponentChanged));

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
                var rd = (RgbDisplay) d;
                rd.OnColorComponentChanged(cp);
            }catch
            {
            }
        }

        #endregion

        private void OnColorComponentChanged(NormalComponent colorPlaneColorComponent)
        {

            if (colorPlaneColorComponent.Name == "RGB_Red")
            {
                 rR.IsChecked = true;
            }
            else if (colorPlaneColorComponent.Name == "RGB_Green")
            {
                 rG.IsChecked = true;
            }
            else if (colorPlaneColorComponent.Name == "RGB_Blue")
            {
                rB.IsChecked = true;
            }
            else
            {
                rR.IsChecked = false;
                rG.IsChecked = false;
                rB.IsChecked = false;
            }

            if (ColorComponentChanged != null)
            {
                ColorComponentChanged(this, new EventArgs<NormalComponent>(colorPlaneColorComponent));
            }

           
            
        }


   

        private void rR_Checked(object sender, RoutedEventArgs e)
        {
            NormalComponent = sRed;
            if (ColorComponentChanged != null)
            {
                ColorComponentChanged(this, new EventArgs<NormalComponent>(sRed));
            }
        }

        private void rG_Checked(object sender, RoutedEventArgs e)
        {
            NormalComponent = sGreen;
            if (ColorComponentChanged != null)
            {
                ColorComponentChanged(this, new EventArgs<NormalComponent>(sGreen));
            }
        }

        private void rB_Checked(object sender, RoutedEventArgs e)
        {
            NormalComponent = sBlue;
            if (ColorComponentChanged != null)
            {
                ColorComponentChanged(this, new EventArgs<NormalComponent>(sBlue));
            }
        }







        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (processEvents)
                {
                    if (sender == txtR)
                    {
                        Color = setR(Color, byte.Parse(txtR.Text, CultureInfo.InvariantCulture));
                    }
                    else if (sender == txtG)
                    {
                        Color = setG(Color, byte.Parse(txtG.Text, CultureInfo.InvariantCulture));
                    }
                    else if (sender == txtB)
                    {
						Color = setB(Color, byte.Parse(txtB.Text, CultureInfo.InvariantCulture));
                    }
                    //Color = System.Windows.Media.Color.FromRgb(byte.Parse(txtR.Text), byte.Parse(txtG.Text),
                    //                                           byte.Parse(txtB.Text));
                }
            }catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }



        private void txtR_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (processEvents) 
            {
                e.Handled = NumbersOnly(e.Text);
                base.OnPreviewTextInput(e); 
            }
        }

        private bool NumbersOnly(string text)
        {
            String okChars = "0123456789";
            return text.ToCharArray().All(c => okChars.IndexOf(c) == -1);
        }

        public static byte FromPercent(  int percent)
        {
            return Convert.ToByte((double)percent / 100 * 255);
        }

        
    }
}
