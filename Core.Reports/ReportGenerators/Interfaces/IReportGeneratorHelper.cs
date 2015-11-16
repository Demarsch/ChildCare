using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Reports
{
    public interface IReportGeneratorHelper
    {
        IReportGenerator CreateDocX(string templateName);
    }
}
