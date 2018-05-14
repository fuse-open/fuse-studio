using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ColorPicker.ColorModels;
using ColorPicker.ExtensionMethods;

namespace ColorPicker
{
    /// <summary>
    /// Interaction logic for ColorSelector.xaml
    /// </summary>
    public partial class ColorSelector : UserControl
    {
        
        public enum ESelectionRingMode
        {
            white,
            Black,
            BlackAndWhite,
            BlackOrWhite
        }

        private enum EColorChangeSource
        {
            ColorPropertySet,
            MouseDown,
            SliderMove,

        }

        public event EventHandler<EventArgs<Color>> ColorChanged;

        private bool ProcessSliderEvents { get; set; }

        private EColorChangeSource mColorChangeSource = EColorChangeSource.ColorPropertySet;
        private readonly TranslateTransform selectionTransform = new TranslateTransform();



        private readonly WriteableBitmap mSelectionPane = new WriteableBitmap(256, 256, 96, 96, PixelFormats.Bgr24, null);
        private readonly WriteableBitmap mNormalPane = new WriteableBitmap(24, 256, 96, 96, PixelFormats.Bgr24, null);

        public static Type ClassType
        {
            get { return typeof(ColorSelector); }
        }

        public ColorSelector()
        {
            InitializeComponent();



            NormalComponent = new ColorModels.HSB.Hue();
            colorPlane.Source = mSelectionPane;
            normalColorImage.Source = mNormalPane;

            colorPlane.MouseDown += colorPlane_MouseDown;
            selectionEllipse.RenderTransform = selectionTransform;
            selectionOuterEllipse.RenderTransform = selectionTransform;
            ProcessSliderEvents = true;
        }

        #region Color

        public static DependencyProperty ColorProperty = DependencyProperty.Register("Color", typeof(Color), ClassType,
            new FrameworkPropertyMetadata(Colors.Transparent, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnColorChanged));
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
             
            var c = (Color)e.NewValue;
            var oldC = (Color)e.OldValue;
            if (c != oldC)
            {
                var cs = (ColorSelector)d;
                cs.OnColorChanged(c);
            }
        }

        private void OnColorChanged(Color color)
        {
            if (mColorChangeSource == EColorChangeSource.ColorPropertySet)
            {
                UpdateColorPlaneBitmap(NormalComponent.Value(color));
                SelectionPoint = NormalComponent.PointFromColor(color);
                selectionTransform.X = SelectionPoint.X - (mSelectionPane.PixelWidth / 2.0);
                selectionTransform.Y = SelectionPoint.Y - (mSelectionPane.PixelHeight / 2.0);


                sNormal.Value = NormalComponent.Value(color);

                if (!NormalComponent.IsNormalIndependantOfColor)
                {
                    NormalComponent.UpdateNormalBitmap(mNormalPane, color);
                }

            }

            if (SelectionRingMode == ESelectionRingMode.BlackOrWhite)
            {
                AdjustSelectionRing(color);
            }
            if (ColorChanged != null)
            {
                ColorChanged(this, new EventArgs<Color>(color));
            }
        }

        private  void AdjustSelectionRing(Color c)
        {
            if (Color.Brightness() > .6)
            {
                selectionEllipse.Stroke = new SolidColorBrush(Colors.Black);
            }
            else
            {
                selectionEllipse.Stroke = new SolidColorBrush(Colors.White);
            };
        }


        #endregion

        #region NormalComponent

        public static DependencyProperty NormalComponentProperty = DependencyProperty.Register("NormalComponent", typeof(NormalComponent),
            ClassType, new FrameworkPropertyMetadata(new ColorModels.HSB.Hue(), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnNormalComponentChanged));
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

        private static void OnNormalComponentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                var cc = (NormalComponent)e.NewValue;
                var cs = (ColorSelector)d;
                cs.OnNormalComponentChanged(cc);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void OnNormalComponentChanged(NormalComponent cc)
        {

            SelectionPoint = cc.PointFromColor(Color);
            selectionTransform.X = SelectionPoint.X - (colorPlane.ActualWidth / 2);
            selectionTransform.Y = SelectionPoint.Y - (colorPlane.ActualHeight / 2);
            ProcessSliderEvents = false;
            sNormal.Minimum = cc.MinValue;
            sNormal.Maximum = cc.MaxValue;
            sNormal.Value = cc.Value(Color);
            ProcessSliderEvents = true;
            cc.UpdateNormalBitmap(mNormalPane, Color);
            cc.UpdateColorPlaneBitmap(mSelectionPane, cc.Value(Color));

        }


        #endregion

        #region SelectionRingMode

        public static DependencyProperty SelectionRingModeProperty = DependencyProperty.Register("SelectionRingMode", typeof(ESelectionRingMode), ClassType, new PropertyMetadata(ESelectionRingMode.BlackAndWhite, OnSelectionRingModeChanged));

         [Category("ColorPicker")]
        public ESelectionRingMode SelectionRingMode
        {
            get
            {
                return (ESelectionRingMode)GetValue(SelectionRingModeProperty);
            }
            set
            {
                SetValue(SelectionRingModeProperty, value);
            }
        }

        private static void OnSelectionRingModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var colorSelector = (ColorSelector) d;
            var selectionRingMode = (ESelectionRingMode) e.NewValue;
            colorSelector.OnSelectionRingModeChanged(selectionRingMode);
        }

        private void  OnSelectionRingModeChanged(ESelectionRingMode selectionRingMode)
        {
            switch (selectionRingMode )
            {
                case ESelectionRingMode.Black:
                    selectionEllipse.Stroke = new SolidColorBrush(Colors.Black);
                    selectionOuterEllipse.Visibility = Visibility.Collapsed;
                    break;
                case ESelectionRingMode.white:
                    selectionEllipse.Stroke = new SolidColorBrush(Colors.White);
                    selectionOuterEllipse.Visibility = Visibility.Collapsed;
                    break;
                case ESelectionRingMode.BlackAndWhite:
                      selectionEllipse.Stroke = new SolidColorBrush(Colors.White);
                    selectionOuterEllipse.Visibility = Visibility.Visible ;
                    break;
                case ESelectionRingMode.BlackOrWhite:
                    AdjustSelectionRing(Color);
                       
                    selectionOuterEllipse.Visibility = Visibility.Collapsed;
                    break;  
            }
        }

        #endregion


        #region Event Handlers

        void colorPlane_MouseDown(object sender, MouseButtonEventArgs e)
        {

            mColorChangeSource = EColorChangeSource.MouseDown;

            ProcessMousedown(e.GetPosition((IInputElement)sender));


            mColorChangeSource = EColorChangeSource.ColorPropertySet;
			e.Handled = true;
        }

        private void ProcessMousedown(Point selectionPoint)
        {
            SelectionPoint = selectionPoint;
            selectionTransform.X = SelectionPoint.X - (colorPlane.ActualWidth / 2);
            selectionTransform.Y = SelectionPoint.Y - (colorPlane.ActualHeight / 2);
            var newColor = NormalComponent.ColorAtPoint(SelectionPoint, (int)sNormal.Value);
            if (!NormalComponent.IsNormalIndependantOfColor)
            {
                NormalComponent.UpdateNormalBitmap(mNormalPane, newColor);
            }
            Color = newColor;

        }


        private void sNormal_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

            mColorChangeSource = EColorChangeSource.SliderMove;
            if (ProcessSliderEvents)
            {
                ProcessSliderEvents = false;
                Color = NormalComponent.ColorAtPoint(SelectionPoint, (int)e.NewValue);
                UpdateColorPlaneBitmap((int)e.NewValue);
                ProcessSliderEvents = true;
            }
            mColorChangeSource = EColorChangeSource.ColorPropertySet;

        }

        private void colorPlane_MouseMove(object sender, MouseEventArgs e)
        {
            mColorChangeSource = EColorChangeSource.MouseDown;

            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                var point = e.GetPosition((IInputElement)sender);
                if (point.X != 256 && point.Y != 256) //Avoids problem that occurs when dragging to edge of colorPane
                {

                    ProcessMousedown(point);
                }
            }


            mColorChangeSource = EColorChangeSource.ColorPropertySet;
			e.Handled = true;
        }

        private void normalColorImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var yPos = (e.GetPosition((IInputElement)sender)).Y;
            var proportion = 1 - yPos / 255;
            var componentRange = NormalComponent.MaxValue - NormalComponent.MinValue;

            var normalValue = NormalComponent.MinValue + proportion * componentRange;

            sNormal.Value = normalValue;
	        e.Handled = true;
        }
        #endregion


        private int lastColorComponentValue = -1;
        private string lastComponentName = "";
        private void UpdateColorPlaneBitmap(int colorComponentValue)
        {

            if (lastColorComponentValue != colorComponentValue || lastComponentName != NormalComponent.Name)
            {
                NormalComponent.UpdateColorPlaneBitmap(mSelectionPane, colorComponentValue);
                lastColorComponentValue = colorComponentValue;
                lastComponentName = NormalComponent.Name;
            }
        }



        private Point SelectionPoint { get; set; }


        public void IncrementNormalSlider()
        {
            sNormal.Value++;
        }

        private void normalColorImage_MouseMove(object sender, MouseEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                var yPos = (e.GetPosition((IInputElement)sender)).Y;
                var proportion = 1 - yPos / 255;
                var componentRange = NormalComponent.MaxValue - NormalComponent.MinValue;

                var normalValue = NormalComponent.MinValue + proportion * componentRange;

                sNormal.Value = normalValue;
	            e.Handled = true;
            }

        }

    }
}
