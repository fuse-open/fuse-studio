using Uno;
using Uno.UX;

namespace Fuse
{
    public class Text: Element
    {
        string _content;
        [Group("Common"), UXContent]
        public string Content
        {
            get { return _content; }
            set { _content = value; }
        }

        bool _isMultiline;
        public bool IsMultiline
        {
            get { return _isMultiline; }
            set { _isMultiline = value; }
        }
    }
}
