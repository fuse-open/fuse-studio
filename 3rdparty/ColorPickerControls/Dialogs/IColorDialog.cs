using System;
using System.Windows.Media;
using ColorPicker;

namespace ColorPickerControls.Dialogs
{
    interface IColorDialog
    {
	    event EventHandler<EventArgs<Color>> PreviewColor;
        Color SelectedColor { get; set; }
        Color InitialColor { get; set; }
        bool? ShowDialog();
    }
}
