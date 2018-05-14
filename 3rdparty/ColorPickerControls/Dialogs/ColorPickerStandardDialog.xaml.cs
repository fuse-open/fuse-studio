using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using ColorPicker;

namespace ColorPickerControls.Dialogs
{
    /// <summary>
    /// Interaction logic for ColorPickerDialog.xaml
    /// </summary>
    public partial class ColorPickerStandardDialog : Window  , IColorDialog
    {
		public event EventHandler<EventArgs<Color>> PreviewColor
		{
			add { colorPickerFull.SelectedColorChanged += value; }
			remove { colorPickerFull.SelectedColorChanged -= value; }
		}

        public ColorPickerStandardDialog()
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
            get { return colorPickerFull.SelectedColor; }
            set { colorPickerFull.SelectedColor = value; }
        }

         [Category("ColorPicker")]
        public Color InitialColor
        {
            get { return colorPickerFull.InitialColor; }
            set
            {
                colorPickerFull.InitialColor = value;
                colorPickerFull.SelectedColor = value;
            }
        }

         [Category("ColorPicker")]
        public ColorSelector.ESelectionRingMode SelectionRingMode
        {
            get { return colorPickerFull.SelectionRingMode; }
            set { colorPickerFull.SelectionRingMode = value; }
        }
    }
}
