using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ColorPicker.ExtensionMethods;

namespace ColorPicker
{
    /// <summary>
    /// Interaction logic for NewCurrent.xaml
    /// </summary>
    public partial class NewCurrent : UserControl
    {
        public static Type ClassType
        {
            get { return typeof(NewCurrent); }
        }

        public NewCurrent()
        {
            InitializeComponent();
        }

        #region NewColor

        public static DependencyProperty NewColorProperty = DependencyProperty.Register("NewColor", typeof(Color), ClassType, 
            new FrameworkPropertyMetadata(Colors.Gray, new PropertyChangedCallback(OnNewColorChanged)));
         [Category("ColorPicker")]
        public Color NewColor
        {
            get
            {
                return (Color)GetValue(NewColorProperty);
            }
            set
            {
                SetValue(NewColorProperty, value);
            }
        }

        private static void OnNewColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var nc = (NewCurrent)d;
            nc.rNew.Fill = new SolidColorBrush(((Color)e.NewValue).WithAlpha(nc.Alpha));

        }

        #endregion

        #region CurrentColor

        public static DependencyProperty CurrentColorProperty = DependencyProperty.Register("CurrentColor", typeof(Color), ClassType, 
            new FrameworkPropertyMetadata(Colors.Black, new PropertyChangedCallback(OnCurrentColorChanged)));

        /// <summary>
        /// The color being selected 
        /// </summary>
         [Category("ColorPicker")]
        public Color CurrentColor
        {
            get
            {
                return (Color)GetValue(CurrentColorProperty);
            }
            set
            {
                SetValue(CurrentColorProperty, value);
            }
        }

        private static void OnCurrentColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
			//Ignore
        }

        #endregion

        #region Alpha

        public static DependencyProperty AlphaProperty = DependencyProperty.Register("Alpha", typeof(byte), ClassType, new PropertyMetadata((byte)255, new PropertyChangedCallback(OnAlphaChanged)));
        /// <summary>
        /// The Alpha Component of the currrent color
        /// </summary>
          [Category("ColorPicker")]
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
            var nc = (NewCurrent)d;
            nc.rNew.Fill = new SolidColorBrush(nc.NewColor.WithAlpha(Convert.ToByte(e.NewValue)));
        }

        #endregion

        //public bool ShowLabels { get; set; }

    }
}
