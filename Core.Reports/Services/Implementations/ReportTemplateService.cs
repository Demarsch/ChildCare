using System;
using System.Collections.Generic;
using System.Linq;
using Core.Reports.DTO;
using Core.Data.Services;
using Core.Data;
using Core.Data.Misc;
using System.Threading.Tasks;

namespace Core.Reports.Services
{
    public class ReportTemplateService : IReportTemplateService
    {
        IDbContextProvider contextProvider;

        public ReportTemplateService(IDbContextProvider dbContextProvider)
        {
            contextProvider = dbContextProvider;
        }

        public ReportTemplateDTO GetTemplate(string reportName)
        {
            using (var context = contextProvider.CreateNewContext())
            {
                return context.Set<ReportTemplate>().Where(x => x.Name == reportName).Select(x => new ReportTemplateDTO { IsDocXTemplate = x.IsDocXTemplate, Template = x.Template, Title = x.ReportTitle }).FirstOrDefault();
            }
        }

        public void SaveTemplate(string reportName, object template, bool IsDocX)
        {
            using (var context = contextProvider.CreateNewContext())
            {
                var t = context.Set<ReportTemplate>().First(x => x.Name == reportName);
                t.Template = template as string;
                t.IsDocXTemplate = IsDocX;
                context.SaveChanges();
            }
        }

        public ICollection<ReportTemplateDTOInfo> GetAllInfoes()
        {
            using (var context = contextProvider.CreateNewContext())
            {
                return context.Set<ReportTemplate>().OrderBy(x => x.Name).Select(x => new ReportTemplateDTOInfo
                {
                    Description = x.Description,
                    Id = x.Id,
                    IsDocXTemplate = x.IsDocXTemplate,
                    Name = x.Name,
                    Title = x.ReportTitle,
                }).ToList();
            }
        }

        public int SaveTemplateInfo(ReportTemplateDTOInfo templateInfo)
        {
            using (var context = contextProvider.CreateNewContext())
            {
                var t = context.Set<ReportTemplate>().Find(templateInfo.Id);
                if (t == null)
                {
                    t = new ReportTemplate();
                    context.Set<ReportTemplate>().Add(t);
                }
                t.Description = templateInfo.Description;
                t.IsDocXTemplate = templateInfo.IsDocXTemplate;
                t.Name = templateInfo.Name;
                t.ReportTitle = templateInfo.Title;
                context.SaveChanges();
                return t.Id;
            }
        }
    }
}
