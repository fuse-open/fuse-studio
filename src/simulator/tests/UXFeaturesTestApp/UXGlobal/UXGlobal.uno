using Uno;
using Uno.UX;

public class UXGlobalTestClass
{
	public string StringProp { get; set; }
}

public class DebugLogGlobalTestClass
{
	UXGlobalTestClass _prop;
	public UXGlobalTestClass Prop
	{
		get { return _prop; }
		set
		{
			_prop = value;
			debug_log("StringProp: " + value.StringProp);
		}
	}
}
