using System;
using System.IO;
using Uno.UXNinja.PerformanceTests.Core.Loggers.LoggersEntities;

namespace Uno.UXNinja.PerformanceTests.Core.Loggers
{
    public abstract class BaseResultLogger : IResultLogger
    {
        protected string _logDirectoryPath;
        protected string _logFileName;
        protected string _targetName;
        protected string _branchName;
        protected ProjectEntity _project;

        public BaseResultLogger(string logDirectoryName, string branchName, string buildNumber)
        {
            var currDate = DateTime.Now;
            _logDirectoryPath = GetLogDirectoryFullPath(currDate, logDirectoryName, buildNumber);
            _branchName = branchName;
        }

        public void LogTimeEvent(string name, string description, double seconds)
        {
            _project.Events.Add(new EventEntity(name, description, seconds * 1000));
        }

        public void LogError(string message)
        {
            if (_project != null)
            {
                _project.Events.Clear();
                _project.ErrorMessage = message;
            }
        }

        public virtual void ProjectStarted(string projectName)
        {
            _project = new ProjectEntity(_branchName, _targetName, projectName);
            _logFileName = Path.Combine(_logDirectoryPath, projectName + ".xml");
        }

        public abstract void ProjectFinished();

        protected abstract string GetLogDirectoryFullPath(DateTime currDate, string logDirectoryName, string buildNumber);
    }
}
