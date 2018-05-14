using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ColorPickerControls.Dialogs;
using ColorPicker;
namespace ColorPickerControls.Chips
{
    /// <summary>
    /// Interaction logic for ColorChip.xaml
    /// </summary>
    public partial class ColorChip : UserControl
    {

        public enum EMouseEvent
        {
            mouseDown,
            mouseUp,
            mouseDoubleClick
        }

        public event EventHandler<EventArgs<Color>> ColorChanged;

        public ColorChip()
        {
            InitializeComponent();
        }

        private EColorDialog mColorDialog = EColorDialog.Full;
        [Category("ColorPicker")]
        public EColorDialog ColorDialog
        {
            get { return mColorDialog; }
            set { mColorDialog = value; }
        }
 

        private EMouseEvent mDialogEvent = EMouseEvent.mouseDown;

        [Category("ColorPicker")]
        public EMouseEvent DialogEvent
        {
            get{ return mDialogEvent;}
            set { mDialogEvent = value; }
        }

        public static Type ClassType
        {
            get { return typeof(ColorChip); }
        }

        #region Color

        public static DependencyProperty ColorProperty = DependencyProperty.Register("Color", typeof(Color), ClassType, new PropertyMetadata(Colors.Gray, new PropertyChangedCallback(OnColorChanged)));
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
            var chip = (ColorChip) d;
            var color = (Color) e.NewValue;
            chip.OnColorChanged(color);
        }

        private void OnColorChanged(Color color)
        {
            Brush = new SolidColorBrush(color);
            colorRect.Background =  Brush;
        }
        #endregion


        #region Brush

        public static DependencyProperty BrushProperty = DependencyProperty.Register("Brush", typeof(SolidColorBrush), ClassType, new PropertyMetadata(null, OnBrushChanged));

        public SolidColorBrush Brush
        {
            get
            {
                return (SolidColorBrush)GetValue(BrushProperty);
            }
            set
            {
                SetValue(BrushProperty, value);
            }
        }

        private static void OnBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var chip = (ColorChip)d;
            var brush = (SolidColorBrush) e.NewValue;
            chip.Color = brush.Color;
        }

        #endregion





        private IColorDialog CreateDialog()
        {
            IColorDialog dialog = null;
            switch (ColorDialog)
            {
                case EColorDialog.Full:
                   dialog = new ColorPickerFullDialog();
                    break;
                case EColorDialog.FullWithAlpha:
                    dialog = new ColorPickerFullWithAlphaDialog();
                    break;
                case EColorDialog.Standard:
                    dialog = new ColorPickerStandardDialog();
                    break;
                case EColorDialog.StandardWithAlpha :
                    dialog = new ColorPickerStandardWithAlphaDialog();
                    break;
            }
            return dialog;
        }

	    public event EventHandler<EventArgs<Color>> PreviewColor;

	    protected void OnPreviewColor(EventArgs<Color> color)
	    {
			Color = color.Value; 
		    var handler = PreviewColor;
		    if (handler != null)
			    handler(this, color);
	    }

        private void ShowDialog()
        {
            var dia =  CreateDialog();
            var initialColor =  ((SolidColorBrush)colorRect.Background).Color;
            dia.InitialColor = initialColor; //set the initial color
	        dia.PreviewColor += (s, a) => OnPreviewColor(a);

	        if (dia.ShowDialog() == true)
	        {
		        if (dia.SelectedColor != initialColor)
		        {
			        Color = dia.SelectedColor; 
			        if (ColorChanged != null)
			        {
				        ColorChanged(this, new EventArgs<Color>(dia.SelectedColor));
			        }
		        }
	        }
	        else
	        {
				Color = initialColor; 
				if (ColorChanged != null)
				{
					ColorChanged(this, new EventArgs<Color>(initialColor));
				}
	        }
        }

        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (DialogEvent == EMouseEvent.mouseDown)
            {
                ShowDialog();
                e.Handled = true;
            }
        }

        private void UserControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (DialogEvent == EMouseEvent.mouseUp)
            {
                ShowDialog();
                e.Handled = true;
            }
        }

        private void UserControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DialogEvent == EMouseEvent.mouseDoubleClick )
            {
                ShowDialog();
                e.Handled = true;
            }
        }
       



    }

}
