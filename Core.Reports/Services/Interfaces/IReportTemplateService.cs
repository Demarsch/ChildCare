using System;
using System.Collections.Generic;
using System.Linq;
using Core.Reports.DTO;

namespace Core.Reports.Services
{
    public interface IReportTemplateService
    {
        ReportTemplateDTO GetTemplate(string reportName);
        void SaveTemplate(string reportName, string template, bool IsDoc);

        ICollection<ReportTemplateDTOInfo> GetAllInfoes();
        int SaveTemplateInfo(ReportTemplateDTOInfo templateInfo);

        bool CheckNameInUse(string reportName, int exceptReportId);
    }
}
