using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Outracks.Fuse.Templates;
using Outracks.IO;

namespace Outracks.Templates.Tests
{
    public class TemplateTest
    {
	  
		[Test]
	    public void Template01()
	    {
		    var resolver = new VariableResolverDummy(
			    new Dictionary<string, string>()
			    {
				    { "DefaultNamespace", "Foo" }
			    });

			var result = new TemplateParser(resolver).ReplaceVariables("_$DefaultNamespace$_");
			Assert.True("Foo" == result);
	    }

		[Test]
		public void Template02()
		{
			var resolver = new VariableResolverDummy(
				new Dictionary<string, string>()
			    {
				    { "DefaultNamespace", "Foo" }
			    });

			var result = new TemplateParser(resolver).ReplaceVariables(@"using Uno;

namespace _$DefaultNamespace$_
{
	class App : Application
	{
	}
}");
			Assert.True(@"using Uno;

namespace Foo
{
	class App : Application
	{
	}
}" == result);
		}

		[Ignore("Need to make a IFileSystem")]
		[Test]
		public void Template03()
		{
			if (Directory.Exists("Foo"))
			{
				Directory.Delete("Foo", true);
			}

			var variableResolver = new VariableResolverDummy(
				new Dictionary<string, string>()
				{
					{ "namespace", "Test" },
					{ "filename", "Foo" }
				});

			var root = AbsoluteDirectoryPath.Parse(Directory.GetCurrentDirectory());
			IFileSystem fileSystem = null; // TODO
			var templates = TemplateLoader.LoadTemplatesFrom(root / "Templates" / "Projects", fileSystem).ToList(); // TODO: real test fs
			var templateSpawner = new TemplateSpawner(variableResolver, fileSystem);

			templateSpawner.Spawn(templates[2], root / "Foo");
		}
    }
}
