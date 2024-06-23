using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Outracks.CodeCompletionFactory.UXNinja;
using Outracks.IO;
using Outracks.UnoDevelop.CodeNinja;
using Outracks.UnoDevelop.UXNinja;
using Uno.Build.Packages;
using Uno.Logging;
using Uno.ProjectFormat;
using Uno.UXNinja.PerformanceTests.Core.Attributes;
using Uno.UXNinja.TestsCommon;
using IContext = Outracks.CodeCompletion.UXNinja.PerformanceTests.Core.Context.IContext;

namespace Uno.UXNinja.PerformanceTests.Core
{
    public abstract class TestBase
    {
        private static Project _project;
        private static DummyEditorManager _editors;
        private IContext _context;

        public double ExecutionTime { get; private set; }

        static TestBase()
        {
            _editors = new DummyEditorManager();
            _project = new Project("Test");
        }

        internal void SetupContext(IContext context)
        {
            _context = context;
        }

        protected void TestPerformance(string code, [CallerMemberName] string testName = "")
        {
            ConfigureProjectReferences();

            //get calling method info
            StackTrace stackTrace = new StackTrace();
            var callingMethod = stackTrace.GetFrame(1).GetMethod();
            var testAttribute = (PerformanceTestAttribute)callingMethod.GetCustomAttributes(typeof(PerformanceTestAttribute), true)[0];

            var caret = GetCaret(ref code);
            var filePath = AbsoluteDirectoryPath.Parse(Directory.GetCurrentDirectory()) / new FileName(testName);

            var log = new DummyLogger();
            var context = Context.CreateContext(filePath, code, caret, new DummySourcePackage());
            var mainPackage = PackageCache.GetPackage(new Log(log.TextWriter), _project);
            mainPackage.SetCacheDirectory((AbsoluteDirectoryPath.Parse(mainPackage.CacheDirectory) / new DirectoryName("UxCompletion")).NativePath);

            var build = new CodeNinjaBuild(log, _project, _editors, mainPackage, mainPackage.References.ToList(), code, filePath);
            build.Execute();

            Stopwatch sw = new Stopwatch();
            sw.Start();
            SuggestionParser.GetSuggestions(build.Compiler, context, caret, new DummyReader());
            sw.Stop();

            if (_context == null)
                throw new ArgumentNullException("_context");

            _context.Logger.LogTimeEvent(testName, testAttribute.Description, sw.ElapsedMilliseconds / 1000f);
        }

        private int GetCaret(ref string code)
        {
            var caret = 0;
            if (code.Contains("$"))
            {
                caret = code.IndexOf("$");
                code = code.Substring(0, caret) + code.Substring(caret + 1, code.Length - caret - 1);
            }
            return caret;
        }

        private void ConfigureProjectReferences()
        {
            _project.MutablePackageReferences.Clear();
            _project.MutableProjectReferences.Clear();

            if (!File.Exists(_context.UnoLibraryPath))
            {
                throw new FileNotFoundException("Unable to load uno library project with test elements and controls");
            }

            _project.MutableProjectReferences.Add(new ProjectReference(new DummySource(), _context.UnoLibraryPath));
        }
    }
}
