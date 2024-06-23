using Uno;
using Uno.Collections;

namespace Fuse
{
    public abstract partial class Node
    {
        Node _host;
        public Node ParentNode { get { return _host; } }
    }
}
