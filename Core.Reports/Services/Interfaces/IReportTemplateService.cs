using System;
using System.Collections.Generic;
using System.Linq;
using Core.Data;
using Core.Data.Misc;
using Core.Reports.DTO;

namespace Core.Reports.Services
{
    public interface IReportTemplateService
    {
        ReportTemplateDTO GetTemplate(string reportName);

        void SaveTemplate(int id, string name, ReportTemplateDTO template, string description);

        IDisposableQueryable<ReportTemplate> GetAll();
    }
}
