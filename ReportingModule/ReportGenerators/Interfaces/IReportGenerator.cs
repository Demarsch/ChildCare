using System;
using System.Collections.Generic;
using System.Linq;

namespace ReportingModule
{
    public interface IReportGenerator : IDisposable
    {
        object Template { get; set; }
        ReportData Data { get; set; }
        string Title { get; set; }
        bool Editable { get; set; }

        string Save();

        void Show();

        void Print();

        void LoadTemplateFromFile(string fileName);
    }
}
