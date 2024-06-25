using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Outracks.CodeCompletion.UXNinja.PerformanceTests.Core.Context;
using Uno.UXNinja.PerformanceTests.Core;
using Uno.UXNinja.PerformanceTests.Core.Attributes;
using Uno.UXNinja.PerformanceTests.Core.Loggers;

namespace Outracks.CodeCompletion.UXNinja.PerformanceTests.Core.Runner
{
    public class PerformanceTestRunner
    {
        public void DiscoverAndRun(RunnerOptions options)
        {
            var logger = ResultLoggersFactory.GetLogger(options.LogDirectoryName, options.BranchName, options.BuildNumber);
            foreach (var c in GetClassesWithTests(options))
            {
                var testMethods = c.GetMethods().Where(m => m.GetCustomAttribute<PerformanceTestAttribute>() != null
                                                            && m.GetCustomAttribute<IgnorePerformanceTestAttribute>() == null);

                logger.ProjectStarted(c.Name);

                Thread t = new Thread(() =>
                {
                    foreach (var method in testMethods)
                    {
                        TestBase classInstance = (TestBase)Activator.CreateInstance(c, null);
                        classInstance.SetupContext(new UxPerformanceTestContext(logger, options.UnoLibraryPath));
                        method.Invoke(classInstance, null);
                    }
                });

                t.Start();
                if (!t.Join(options.TestTimeout))
                {
                    t.Abort();
                    logger.LogError("The operation has timed-out");
                }

                logger.ProjectFinished();
            }
        }

        private IEnumerable<Type> GetClassesWithTests(RunnerOptions options)
        {
            var res = new List<Type>();

            //add current path to search paths
            options.Paths.Add(Directory.GetCurrentDirectory());

            foreach (var path in options.Paths)
            {
                var di = new DirectoryInfo(path);
                foreach (var fi in di.EnumerateFiles("*.dll", SearchOption.AllDirectories))
                {
                    var assembly = Assembly.LoadFile(fi.FullName);
                    res.AddRange(assembly.GetTypes().Where(t => t.BaseType == typeof(TestBase)));
                }
            }

            return res.GroupBy(u => u.GUID).Select(u => u.First());
        }
    }
}
