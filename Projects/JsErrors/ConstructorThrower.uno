using Uno;
using Fuse.Controls;
using Fuse.Gestures;

public class ConstructorThrower : Button
{
    public ConstructorThrower()
    {
        throw new Exception("Bye from Uno constructor");
    }
}