using System;
using System.Collections.Generic;
using System.Linq;

namespace ReportingModule
{
    public interface IReportGeneratorHelper
    {
        IReportGenerator CreateDocX(string templateName);
    }
}
