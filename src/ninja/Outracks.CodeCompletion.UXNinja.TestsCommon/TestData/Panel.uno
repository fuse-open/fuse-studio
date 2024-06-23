using Uno;
using Uno.Collections;
using Uno.UX;

namespace Fuse
{
    public class Panel : Element
    {
        IList<Element> _children = null;
        [UXPrimary]
        public IList<Element> Children
        {
            get { return _children; }
        }

        Element _appearance;
        [DesignerName("Appearance"), Group("Style")]
        public Element Appearance
        {
            get { return _appearance; }
            set { _appearance = value; }
        }

        [Hide]
        public bool HasChildren { get { return _children != null && _children.Count > 0; } }    
    }
}
