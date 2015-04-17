using System;
using System.Collections.Generic;

namespace Registry
{
    public class RecordTypeViewModel
    {
        public string Name { get; private set; }

        public RecordTypeViewModel(string name, IEnumerable<ScheduledAssignmentViewModel> assignments)
        {
            Name = name;
            if (assignments == null)
                throw new ArgumentNullException("assignments");
            Assignments = assignments;
        }

        public IEnumerable<ScheduledAssignmentViewModel> Assignments { get; private set; }
    }
}