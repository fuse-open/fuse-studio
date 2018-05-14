using System;
using System.Collections.Generic;

namespace Uno.UXNinja.PerformanceTests.Core.Loggers.LoggersEntities
{
    public class ProjectEntity
    {
        public ProjectEntity()
        {
            Events = new List<EventEntity>();
        }

        public ProjectEntity(string branchName, string target, string projectName)
            : this()
        {
            Name = projectName;
            BranchName = branchName;
            StartDate = DateTime.Now;
        }

        public string Name { get; set; }

        public string BranchName { get; set; }

        public DateTime StartDate { get; set; }

        public string ErrorMessage { get; set; }

        public string TestDescription { get; set; }

        public List<EventEntity> Events { get; set; }
    }
}
