using Uno;
using Uno.UX;
using Fuse;

class UXResourceTestClass : Node
{
	UXResourceTestClass2 _res;
	public UXResourceTestClass2 Res
	{
		get { return _res; }
		set
		{
			_res = value;
			debug_log("Value: " + value);
		}
	}

}

class UXResourceTestClass2 : Node
{
}
