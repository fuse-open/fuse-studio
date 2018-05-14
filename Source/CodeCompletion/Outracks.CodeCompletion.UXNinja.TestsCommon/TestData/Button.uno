using Fuse;
using Uno;
using Uno.Platform;
using Uno.UX;
using Uno.Collections.EnumerableExtensions;

namespace Custom.Controls
{
    public class Button: Panel
    {
        [Group("Events")]
        public event ClickedHandler Clicked;

        string _text;
        [Group("Common"), DesignerName("Text")]
        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }
    }
}
