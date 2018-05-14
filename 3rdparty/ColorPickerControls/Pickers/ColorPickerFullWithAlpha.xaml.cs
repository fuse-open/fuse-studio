using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ColorPicker.ExtensionMethods;
using ColorPicker;
namespace ColorPickerControls.Pickers
{
    /// <summary>
    /// Interaction logic for ColorPickerFullWithAlpha.xaml
    /// </summary>
    public partial class ColorPickerFullWithAlpha : UserControl
    {
        public   ColorPickerFullWithAlpha()
        {
            InitializeComponent();
            colorSelector.ColorChanged += colorSelector_ColorChanged;

            alphaDisplay.AlphaChanged += alphaDisplay_AlphaChanged;


	        SelectedColorChanged += (s, a) => _selectedColorChanged.OnNext(a.Value);

        }

        void alphaDisplay_AlphaChanged(object sender, ColorPicker.EventArgs<byte> e)
        {
            SetValue(SelectedColorProperty,SelectedColor.WithAlpha(e.Value));
            if (SelectedColorChanged != null)
            {
                SelectedColorChanged(this, new EventArgs<Color>(SelectedColor));
            }
        }

        void colorSelector_ColorChanged(object sender, ColorPicker.EventArgs<Color> e)
        {
            SetValue(SelectedColorProperty, e.Value.WithAlpha(alphaDisplay.Alpha ));
            if (SelectedColorChanged != null)
            {
                SelectedColorChanged(this, new EventArgs<Color>(SelectedColor));
            }
        }

        public static Type ClassType
        {
            get { return typeof(ColorPickerFullWithAlpha); }
        }
        #region InitialColor

        public static DependencyProperty InitialColorProperty = DependencyProperty.Register("InitialColor", typeof(Color), ClassType,
            new FrameworkPropertyMetadata(Colors.Transparent, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnInitialColorChanged));
        [Category("ColorPicker")]
        public Color InitialColor
        {
            get
            {
                return (Color)GetValue(InitialColorProperty);
            }
            set
            {
                SetValue(InitialColorProperty, value);
            }
        }


        private static void OnInitialColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var cpf = (ColorPickerFullWithAlpha)d;
            cpf.newCurrent.CurrentColor = (Color)e.NewValue;

        }


        #endregion

	    bool _isUpdatingView = true;
	    readonly Subject<Color> _selectedColorChanged = new Subject<Color>();

	    public IObservable<Color> UserChangedColor
	    {
			get { return _selectedColorChanged.Where(_ => !_isUpdatingView); }
	    }

        public event EventHandler<EventArgs<Color>> SelectedColorChanged;
        #region SelectedColor

        public static DependencyProperty SelectedColorProperty = DependencyProperty.Register("SelectedColor", typeof(Color), ClassType,
            new FrameworkPropertyMetadata(Colors.Transparent, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedColorChanged));
        [Category("ColorPicker")]
        public Color SelectedColor
        {
            get
            {
                return (Color)GetValue(SelectedColorProperty);
            }
            set
            {
	            _isUpdatingView = true;
	            try
	            {
					SetValue(SelectedColorProperty, value);
	            }
	            finally
	            {
		            _isUpdatingView = false;
	            }
            }
        }


        private static void OnSelectedColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var cpf = (ColorPickerFullWithAlpha)d;
            var color =  (Color)e.NewValue;
            cpf.colorSelector.Color = color;
            cpf.alphaDisplay.Alpha = color.A;

            if (cpf.SelectedColorChanged != null)
            {
                cpf.SelectedColorChanged(cpf, new EventArgs<Color>(color ));
            }

        }


        #endregion

         [Category("ColorPicker")]
        public ColorSelector.ESelectionRingMode SelectionRingMode
        {
            get { return colorSelector.SelectionRingMode; }
            set { colorSelector.SelectionRingMode = value; }
        }
    }
}
