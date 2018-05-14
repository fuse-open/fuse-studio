using Uno;
using Uno.UX;
using Fuse;

public class UXClassTest
{
	public UXClassTest()
	{
		var mc = new MyClass();
		debug_log("String: " + mc.MyString);
		debug_log("Int: " + mc.MyInt);
	}
}
