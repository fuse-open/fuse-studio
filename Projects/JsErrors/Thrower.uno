using Uno;
using Fuse.Controls;
using Fuse.Gestures;

public class Thrower : Button
{
    public Thrower()
    {
        Fuse.Gestures.Clicked.AddHandler(this, ClickHandler);
    }
    public void ClickHandler(object _, object __)
    {
        throw new Exception("Bye from Uno");
    }

}
