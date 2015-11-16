using System;

namespace Core.Reports.Services
{
    public interface IReportFileOperations
    {
        string CreateFileName(string folder, string title, string extention);
        string CreateTempFolder(string prefix);
        void StartDocument(string filename);
        void StartDocument(string filename, string verb);
    }
}
