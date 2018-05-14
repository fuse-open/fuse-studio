using System;
using System.IO;
using Uno.UXNinja.PerformanceTests.Core.Loggers.LoggersEntities;

namespace Uno.UXNinja.PerformanceTests.Core.Loggers
{
    public class ResultFileLogger : BaseResultLogger
    {
        public ResultFileLogger(string logDirectoryName, string branchName, string buildNumber)
            : base(logDirectoryName, branchName, buildNumber)
        {
            if (!Directory.Exists(_logDirectoryPath))
                Directory.CreateDirectory(_logDirectoryPath);
        }

        #region Override Methods

        public override void ProjectStarted(string projectName)
        {
            base.ProjectStarted(projectName);

            if (File.Exists(_logFileName))
                File.Delete(_logFileName);
        }

        public override void ProjectFinished()
        {
            if (_project != null)
                File.WriteAllText(_logFileName, _project.ToXmlString<ProjectEntity>());
        }

        protected override string GetLogDirectoryFullPath(DateTime currDate, string logDirectoryName, string buildNumber)
        {
            var sessionDirectoryName = string.IsNullOrEmpty(buildNumber) ? currDate.Ticks.ToString() : buildNumber;
            var dateDirectoryName = string.IsNullOrEmpty(_branchName) ? Path.Combine(logDirectoryName, "unnamed", currDate.ToString("MM_dd_yyy"))
                                                                      : Path.Combine(logDirectoryName, _branchName, currDate.ToString("MM_dd_yyy"));

            var num = GetNumberOfSessionDirectories(dateDirectoryName);
            return Path.Combine(dateDirectoryName, string.Format("{0}__{1}", num + 1, sessionDirectoryName));
        }

        #endregion

        #region Private Methods

        private int GetNumberOfSessionDirectories(string dateDirectoryName)
        {
            if (!Directory.Exists(dateDirectoryName))
                return 0;

            return Directory.GetDirectories(dateDirectoryName).Length;
        }

        #endregion
    }
}
