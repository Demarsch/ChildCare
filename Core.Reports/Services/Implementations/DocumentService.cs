using System;
using System.Collections.Generic;
using System.Linq;
using Core.Reports.DTO;
using Core.Data.Services;
using Core.Data;
using Core.Data.Misc;

namespace Core.Reports.Services
{
    public class DocumentService : IDocumentService
    {
        IDbContextProvider contextProvider;

        public DocumentService(IDbContextProvider dbContextProvider)
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

        public void SaveTemplate(string reportName, string template, bool IsDocX)
        {
            using (var context = contextProvider.CreateNewContext())
            {
                var t = context.Set<ReportTemplate>().First(x => x.Name == reportName);
                t.Template = template;
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

        public bool CheckNameInUse(string reportName, int exceptReportId)
        {
            using (var context = contextProvider.CreateNewContext())
            {
                return context.Set<ReportTemplate>().Any(x => x.Name == reportName && x.Id != exceptReportId);
            }
        }

        public IDisposableQueryable<ReportTemplate> GetPrintedDocuments(string documentOption)
        {
            var context = contextProvider.CreateNewContext();
            var query = context.Set<PrintedDocument>().Where(x => x.Options == documentOption);
            if (query.Any())
                query = query.Union(query.SelectMany(x => x.PrintedDocuments1.Where(a => a.ReportTemplateId.HasValue)));

            return new DisposableQueryable<ReportTemplate>(query.Where(x => x.ReportTemplateId.HasValue).Distinct().Select(x => x.ReportTemplate), context);
        }
        
        public IDisposableQueryable<PrintedDocument> GetPrintedDocumentById(int printedDocumentId)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<PrintedDocument>(context.Set<PrintedDocument>().Where(x => x.Id == printedDocumentId), context);
        }

        public IDisposableQueryable<Person> GetPersonById(int id)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<Person>(context.Set<Person>().Where(x => x.Id == id), context);
        }

        public IDisposableQueryable<RecordContract> GetContractById(int id)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<RecordContract>(context.Set<RecordContract>().Where(x => x.Id == id), context);
        }

        public string GetDBSettingValue(string parameter, bool useDisplayName = false)
        {
            var setting = contextProvider.CreateNewContext().Set<DBSetting>().FirstOrDefault(x => x.Name == parameter);
            if (setting != null)
                return (useDisplayName ? setting.DisplayName : setting.Value);
            return string.Empty;
        }
    }
}
