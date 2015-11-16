using System;
using System.Collections.Generic;
using System.Linq;
using Core.Reports.DTO;
using Core.Data.Services;
using Core.Data;
using Core.Data.Misc;

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

        public void SaveTemplate(int id, string name, ReportTemplateDTO template, string description)
        {
            //todo: implement not only string template saving/using ?
            using (var context = contextProvider.CreateNewContext())
            {
                var t = context.Set<ReportTemplate>().Find(id);
                if (t == null)
                {
                    t = new ReportTemplate();
                    context.Set<ReportTemplate>().Add(t);
                }
                t.Description = description;
                t.IsDocXTemplate = template.IsDocXTemplate;
                t.Name = name;
                t.ReportTitle = template.Title;
                t.Template = template.Template as string;
                context.SaveChanges();
            }
        }

        public IDisposableQueryable<ReportTemplate> GetAll()
        {
            var context = contextProvider.CreateNewContext();
            context.Configuration.ProxyCreationEnabled = false;
            return new DisposableQueryable<ReportTemplate>(context.Set<ReportTemplate>().AsNoTracking(), context);
        }
    }
}
