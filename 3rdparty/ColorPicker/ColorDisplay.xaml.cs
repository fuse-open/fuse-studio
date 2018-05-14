using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ColorPicker
{
    /// <summary>
    /// Interaction logic for ColorDisplay.xaml
    /// </summary>
    public partial class ColorDisplay : UserControl
    {

        public static Type ClassType
        {
            get { return typeof(ColorDisplay); }
        }


        #region Color

        public static DependencyProperty ColorProperty = DependencyProperty.Register("Color", typeof(Color), ClassType, new PropertyMetadata(Colors.Gray, OnColorChanged));
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
            var display = (ColorDisplay)d;
            var color = (Color)e.NewValue;
            display.OnColorChanged(color);
        }

        private void OnColorChanged(Color color)
        {
            
            colorRect.Background = new SolidColorBrush(color);
        }
        #endregion

        public ColorDisplay()
        {
            InitializeComponent();
        }
    }
}
