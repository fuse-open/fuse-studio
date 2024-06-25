using Uno;
using Uno.UX;

public class UXValueTestClass
{
	string _string1;
	int _int1;
	float _float1;

   [UXConstructor]
	public UXValueTestClass
		([UXParameter("String1")]string s1,
		 [UXParameter("Int1")]int i1,
		 [UXParameter("Float1")]float f1)
	{
		_string1 = s1;
		_int1 = i1;
		_float1 = f1;

		debug_log("String1: " + _string1);
		debug_log("Int1: " + _int1);
		debug_log("Float1: " + _float1);
	}
}

public class UXValueTestClass2
{
	public UXValueTestClass2(){}

	public UXValueTestClass2(int foo)
	{
		debug_log("FOOO: " + foo);
	}
}