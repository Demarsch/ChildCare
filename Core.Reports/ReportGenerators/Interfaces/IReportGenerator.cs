using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Reports
{
    public interface IReportGenerator : IDisposable
    {
        object Template { get; set; }
        ReportData Data { get; set; }
        string Title { get; set; }
        bool Editable { get; set; }

        string Save();

        string Show();

        string Print();

        void LoadTemplateFromFile(string fileName);
    }
}
