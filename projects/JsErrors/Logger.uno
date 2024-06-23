using Uno;
using Fuse.Controls;
using Fuse.Gestures;

public class Logger : Button
{
    public Logger()
    {
        Fuse.Gestures.Clicked.AddHandler(this, ClickHandler);
    }
    public void ClickHandler(object _, object __)
    {
        debug_log("Hello from Uno");
    }

}
