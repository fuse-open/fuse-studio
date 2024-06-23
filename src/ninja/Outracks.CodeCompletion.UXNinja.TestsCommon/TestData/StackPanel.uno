using Uno;
using Uno.UX;
using Uno.Collections;

namespace Fuse
{
    public class StackPanel : Panel
    {
        Orientation _orientation;
        [Group("Layout")]
        public Orientation Orientation
        {
            get { return _orientation; }
            set { _orientation = value; }
        }

        float _itemSpacing;
        [Group("Layout")]
        public float ItemSpacing
        {
            get { return _itemSpacing; }
            set { _itemSpacing = value; }
        }
    }
}
