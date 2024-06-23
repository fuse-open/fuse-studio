namespace RegressionTests
{
	internal class Test
	{
		public readonly string Name;
		public readonly string Path;

		public Test(string name, string path)
		{
			Name = name;
			Path = path;
		}

		public override string ToString()
		{
			return Name + " (" + Path + ")";
		}
	}
}
