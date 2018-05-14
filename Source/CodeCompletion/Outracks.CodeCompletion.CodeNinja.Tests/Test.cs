using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Outracks.CodeCompletion;
using Outracks.IO;
using Uno.ProjectFormat;
using Uno;
using Uno.Logging;

namespace Outracks.CodeNinja.Tests.Tests
{
    public class TestException : Exception
    {
        public TestException(string message) : base(message)
        {
        }
    }

    public abstract class Test
    {
	    static Project _project;

        static Test()
        {
			_project = new Project("Test");

            //_solution = Solution.Create(new FilePath("Tests/Tests.unosln"));

            //var proj = Project.Load(new FilePath("Tests/Tests.unoproj"));
            //proj.OnCreate();

            //_solution.Children.Add(proj);
            //_solution.StartupProject = proj;

			_project.MutablePackageReferences.Add(new PackageReference(Source.Unknown, "UnoCore"));
			/*_project.PackageReferences.Add(new PackageReference("FuseCore"));
			_project.PackageReferences.Add(new PackageReference("Fuse.Animations"));
			_project.PackageReferences.Add(new PackageReference("Fuse.Drawing"));
			_project.PackageReferences.Add(new PackageReference("Fuse.Drawing.Primitives"));
			_project.PackageReferences.Add(new PackageReference("Fuse.Entities"));
			_project.PackageReferences.Add(new PackageReference("Fuse.Elements"));
			_project.PackageReferences.Add(new PackageReference("Fuse.Controls"));*/

		}

	    protected void AssertCode(string code, [CallerMemberName] string path = "")
	    {		    
	    }
#if false
        DummyLogger _log;

        protected void AssertCode(string code, [CallerMemberName] string path = "")
        {
            int caret = 0;
            var assertCodeSnippets = GetAsserts(ref code, ref caret);

	        var ubProject = _project;

	        var filePath = AbsoluteDirectoryPath.Parse(Directory.GetCurrentDirectory()) / new FileName(path);
			_log = new DummyLogger();
            var editors = new DummyEditorManager();
	        var mainPackage = new PackageCache(new Log(_log.TextWriter), ubProject.Config).GetPackage(ubProject);
			mainPackage.SetCacheDirectory((AbsoluteDirectoryPath.Parse(mainPackage.CacheDirectory) / new DirectoryName("CodeCompletion")).NativePath);
            var build = new CodeNinjaBuild(_log, ubProject, editors, mainPackage, mainPackage.References.ToList(), code, filePath);

            build.Execute();

            var engine = new DummyEngine(build.Compiler);
            var codeCompleter = new CodeCompleter(engine.Compiler,
				new Source(build.ProjectPackage, filePath.NativePath),
                new CodeReader(code, caret),
                caret,
                Parser.Parse(code));

            if (codeCompleter.Context.NodePath.Count < 1)
            {
                throw new TestException("Invalid node path was generated for the test case");
            }

            ConfidenceLevel confidenceLevel;
            var suggestions = codeCompleter.SuggestCompletion("", out confidenceLevel);

            if (assertCodeSnippets != null)
            {
                foreach (var codeSnippet in assertCodeSnippets)
                    OnAssert(code, codeSnippet, suggestions);
            }

        }

        static IEnumerable<string> GetAsserts(ref string code, ref int caret)
        {
            string[] items = null;
            while (code.Contains('$'))
            {
                caret = code.IndexOf('$');
                int e = code.IndexOf(')', caret);

                string macro = code.Substring(caret + 2, e - caret - 2);
                code = code.Substring(0, caret) + ";" + code.Substring(e + 1, code.Length - e - 1);

                items = macro.Split(',');
                for (int k = 0; k < items.Length; k++)
                    items[k] = items[k].Trim();
            }
            return items;
        }

        void OnAssert(string sourceCode, string code, IEnumerable<SuggestItem> suggestions)
        {
            bool testIsInverted = false;
            if (code.Contains('!'))
            {
                testIsInverted = true;
                code = code.Trim('!');
            }

            bool found = suggestions.Any(suggestion => suggestion.Text == code);

            string expression = string.Format("{0} should {2}be among [{1}]",
                                              code,
                                              string.Join(",", suggestions.Select(x => x.Text)),
                                              testIsInverted ? "not " : "");

            bool leftHandValue = !testIsInverted; // expected
            bool rightHandValue = found; // actual
            bool result = testIsInverted ^ found;

            if (!result)
            {
                throw new TestException(expression + "\n\n Compiler log: " + _log.sb.ToString() + "\n\nSource code:\n" + sourceCode);
                //Assert.True(result, expression);
            }
        }
#endif
	}
}
