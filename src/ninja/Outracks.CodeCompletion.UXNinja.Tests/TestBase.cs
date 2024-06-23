using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Outracks.CodeCompletion;
using Outracks.CodeCompletionFactory.UXNinja;
using Outracks.IO;
using Outracks.UnoDevelop.CodeNinja;
using Outracks.UnoDevelop.UXNinja;
using Uno.Build.Packages;
using Uno.Logging;
using Uno.ProjectFormat;
using Uno.UXNinja.TestsCommon;

namespace Uno.UXNinja.Tests
{
    public class TestException : Exception
    {
        public TestException(string message)
            : base(message)
        {
        }
    }

    public class TestBase
    {
        static Project _project;
        static DummyEditorManager _editors;

        static TestBase()
        {
            var unoprojfile = AbsoluteDirectoryPath.Parse(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)) / ".." / ".." / ".." / "Outracks.CodeCompletion.UXNinja.TestsCommon" / "TestData" / new FileName("Outracks.UXNinja.Tests.Library.unoproj");
            var relativeUnoprojFile = unoprojfile.RelativeTo(DirectoryPath.GetCurrentDirectory());

            _editors = new DummyEditorManager();
            _project = new Project("Test");
            _project.MutablePackageReferences.Clear();
            _project.MutableProjectReferences.Clear();
            _project.MutableProjectReferences.Add(new ProjectReference(new DummySource(), relativeUnoprojFile.NativeRelativePath));
        }

        public void AssertUX(string code, [CallerMemberName] string path = "")
        {
            int caret = 0;
            var assertCodeSnippets = GetAsserts(ref code, ref caret);
            var filePath = AbsoluteDirectoryPath.Parse(Directory.GetCurrentDirectory()) / new FileName(path);

            var log = new DummyLogger();
            var context = Context.CreateContext(filePath, code, caret, new DummySourcePackage());
            var mainPackage = PackageCache.GetPackage(new Log(log.TextWriter), _project);
            mainPackage.SetCacheDirectory((AbsoluteDirectoryPath.Parse(mainPackage.CacheDirectory) / new DirectoryName("UxCompletion")).NativePath);

            var build = new CodeNinjaBuild(log, _project, _editors, mainPackage, mainPackage.References.ToList(), code, filePath);
            build.Execute();

            var suggestions = SuggestionParser.GetSuggestions(build.Compiler, context, caret, new DummyReader());

            if (assertCodeSnippets != null)
            {
                foreach (var codeSnippet in assertCodeSnippets)
                    OnAssert(code, codeSnippet, suggestions, log);
            }
        }

        IEnumerable<string> GetAsserts(ref string code, ref int caret)
        {
            string[] items = null;
            while (code.Contains('$'))
            {
                caret = code.IndexOf('$');
                int e = code.IndexOf(')', caret);

                string macro = code.Substring(caret + 2, e - caret - 2);
                code = code.Substring(0, caret) + code.Substring(e + 1, code.Length - e - 1);

                items = macro.Split(',');
                for (int k = 0; k < items.Length; k++)
                    items[k] = items[k].Trim();
            }
            return items;
        }

        void OnAssert(string sourceCode, string codeSnippet, IEnumerable<SuggestItem> suggestions, DummyLogger log)
        {
            bool testIsInverted = false;
            if (codeSnippet.Contains('!'))
            {
                testIsInverted = true;
                codeSnippet = codeSnippet.Trim('!');
            }

            bool found = suggestions.Any(suggestion => suggestion.Text == codeSnippet);

            string expression = string.Format("{0} should {2}be among [{1}]",
                                              codeSnippet,
                                              string.Join(",", suggestions.Select(x => x.Text)),
                                              testIsInverted ? "not " : "");

            bool result = testIsInverted ^ found;

            if (!result)
            {
                throw new TestException(expression + "\n\n Compiler log: " + log.sb.ToString() + "\n\nSource code:\n" + sourceCode);
            }
        }
    }
}
