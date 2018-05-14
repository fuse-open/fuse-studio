using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ColorPicker;
namespace ColorPickerControls.Pickers
{
    /// <summary>
    /// Interaction logic for ColorPickerFull.xaml
    /// </summary>
    public partial class ColorPickerStandard : UserControl
    {

        public static Type ClassType
        {
            get { return typeof(ColorPickerStandard); }
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
            var cpf = (ColorPickerStandard)d;
           cpf.newCurrent.CurrentColor = (Color) e.NewValue ;
         
        }


        #endregion

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
                SetValue(SelectedColorProperty, value);
            }
        }


        private static void OnSelectedColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var cpf = (ColorPickerStandard)d;
            cpf.colorSelector.Color = (Color)e.NewValue;
            if (cpf.SelectedColorChanged != null)
            {
                cpf.SelectedColorChanged(cpf, new EventArgs<Color>((Color)e.NewValue));
            }
            
        }


        #endregion


        public ColorPickerStandard()
        {
            InitializeComponent();
  
            SetBinding(SelectedColorProperty, "Color");
            DataContext = colorSelector;
        }

         [Category("ColorPicker")]
        public ColorSelector.ESelectionRingMode SelectionRingMode
        {
            get { return colorSelector.SelectionRingMode; }
            set { colorSelector.SelectionRingMode = value; }
        }
    }
}
