using Uno;
using Uno.UX;

public class BindingTestClass1
{
	public object P1 { get; set; }
	public BindingTestBaseClass P2 { get; set; }
	public BindingTestClass2 P3 { get; set; }
	public BindingTestInterface P4 { get; set; }
}

public interface BindingTestInterface{ }

public class BindingTestBaseClass { }

public class BindingTestClass2 : BindingTestBaseClass, BindingTestInterface { }