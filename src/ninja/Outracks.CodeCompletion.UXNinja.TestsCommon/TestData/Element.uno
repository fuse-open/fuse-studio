using Uno;
using Uno.Collections;

namespace Fuse
{
    public abstract class Element : Node
    {
        internal float2 _actualSize;

        [Hide]
        public float2 ActualSize
        {
            get { return _actualSize; }
        }

        float _width;
        public float Width
        {
            get { return _width; }
            set { _width = value; }
        }

        float _height;
        public float Height
        {
            get { return _height; }
            set { _height = value; }
        }
    }
}
