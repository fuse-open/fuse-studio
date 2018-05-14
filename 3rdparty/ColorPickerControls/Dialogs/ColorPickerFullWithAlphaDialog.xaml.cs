using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using ColorPicker;

namespace ColorPickerControls.Dialogs
{
    /// <summary>
    /// Interaction logic for ColorPickerFullWithAlphaDialog.xaml
    /// </summary>
    public partial class ColorPickerFullWithAlphaDialog : Window, IColorDialog
    {
		public event EventHandler<EventArgs<Color>> PreviewColor
		{
			add { colorPickerFullWithAlpha.SelectedColorChanged += value; }
			remove { colorPickerFullWithAlpha.SelectedColorChanged -= value; }
		}

        public ColorPickerFullWithAlphaDialog()
        {
            InitializeComponent();
        }

        private void btOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void btCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

         [Category("ColorPicker")]
        public Color SelectedColor
        {
            get { return colorPickerFullWithAlpha.SelectedColor; }
            set { colorPickerFullWithAlpha.SelectedColor = value; }
        }

         [Category("ColorPicker")]
        public Color InitialColor
        {
            get { return colorPickerFullWithAlpha.InitialColor; }
            set
            {
                colorPickerFullWithAlpha.InitialColor = value;
                colorPickerFullWithAlpha.SelectedColor = value;
            }
        }

         [Category("ColorPicker")]
        public ColorSelector.ESelectionRingMode SelectionRingMode
        {
            get { return colorPickerFullWithAlpha.SelectionRingMode; }
            set { colorPickerFullWithAlpha.SelectionRingMode = value; }
        }
    }
}
