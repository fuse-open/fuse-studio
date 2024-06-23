using Uno;
using Fuse;
using Uno.UX;

public class GenerateFactoryTest
{
	IFactory _factory;
	public IFactory Factory
	{
		get { return _factory; }
		set
		{
			_factory = value;
			var test = (TestClass)value.New();
			debug_log("Factory: " + test);
			debug_log("Value: " + test.Foo);
		}
	}
}

public class TestClass
{
	public string Foo { get; set; }

}