using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ColorPicker
{
    /// <summary>
    /// Interaction logic for AlphaDisplay.xaml
    /// </summary>
    public partial class AlphaDisplay : UserControl
    {
        public static Type ClassType
        {
            get { return typeof(AlphaDisplay); }
        }

        public event EventHandler<EventArgs<byte>> AlphaChanged;

        public AlphaDisplay()
        {
            InitializeComponent();
            imgTransparency.Source = mTransparencyBitmap;
        }
        private WriteableBitmap mTransparencyBitmap = new WriteableBitmap(24, 256, 96, 96, PixelFormats.Bgra32, null); 

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
            var rd = (AlphaDisplay)d;
            rd.OnColorChanged(color);
        }

        private void OnColorChanged(Color color)
        {
            unsafe
            {
                mTransparencyBitmap.Lock();
                int currentPixel = -1;
                byte* pStart = (byte*)(void*)mTransparencyBitmap.BackBuffer;
                for (int iRow = 0; iRow < mTransparencyBitmap.PixelHeight; iRow++)
                {
                    for (int iCol = 0; iCol < mTransparencyBitmap.PixelWidth; iCol++)
                    {
                        currentPixel++;
                        *(pStart + currentPixel * 4 + 0) = color.B; //Blue
                        *(pStart + currentPixel * 4 + 1) = color.G; //Green 
                        *(pStart + currentPixel * 4 + 2) = color.R; //red
                        *(pStart + currentPixel * 4 + 3) = (byte)(255 - iRow); //alpha
                    }
                }

                mTransparencyBitmap.AddDirtyRect(new Int32Rect(0, 0, mTransparencyBitmap.PixelWidth, mTransparencyBitmap.PixelHeight));
                mTransparencyBitmap.Unlock();
            }

          
        }

        #endregion

        #region Alpha

        public static DependencyProperty AlphaProperty = DependencyProperty.Register("Alpha", typeof(byte), ClassType, new PropertyMetadata((byte)255, new PropertyChangedCallback(OnAlphaChanged)));

        public byte Alpha
        {
            get
            {
                return (byte)GetValue(AlphaProperty);
            }
            set
            {
                SetValue(AlphaProperty, value);
            }
        }

        private static void OnAlphaChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var display = (AlphaDisplay) d;
            display.sAlpha.Value = (byte)e.NewValue/2.55 ;
            if (display.AlphaChanged != null)
            {
                display.AlphaChanged(display,new EventArgs<byte>(display.Alpha));
            }
        }

        #endregion

        private void sAlpha_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Alpha = Convert.ToByte( e.NewValue*2.55);
        }

        private void imgTransparency_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var alphaPercent = 100 * (imgTransparency.ActualHeight - (e.GetPosition((IInputElement)sender)).Y) / imgTransparency.ActualHeight;
            sAlpha.Value = alphaPercent;
			e.Handled = true;
        }

        private void imgTransparency_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                var alphaPercent = 100 * (imgTransparency.ActualHeight - (e.GetPosition((IInputElement)sender)).Y) / imgTransparency.ActualHeight;
                sAlpha.Value = alphaPercent;
            }
	        e.Handled = true;
        }
    }
}
