using Core.Reports.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Reports
{
    public class ReportGeneratorHelper : IReportGeneratorHelper
    {
        IReportTemplateService templateService;
        Func<DocXReportGenerator> docXgeneratorCreator;

        public ReportGeneratorHelper(IReportTemplateService templateService, Func<DocXReportGenerator> docXgeneratorCreator)
        {
            this.templateService = templateService;
            this.docXgeneratorCreator = docXgeneratorCreator;
        }

        public IReportGenerator CreateDocX(string templateName)
        {
            try
            {
                var t = templateService.GetTemplate(templateName);
                if (!t.IsDocXTemplate)
                    throw new ArgumentException(string.Format("Шаблон отчета {0} не отмечен как DocX", templateName));
                var r = docXgeneratorCreator();
                r.Template = t.Template;
                r.Title = t.Title;
                return r;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Не удалось загрузить шаблон отчета " + templateName, ex);
            }            
        }


        public IReportGenerator CreateDocXFromFile(string fileName)
        {
            try
            {
                var r = docXgeneratorCreator();
                r.LoadTemplateFromFile(fileName);
                r.Title = "Отчет";
                return r;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Не удалось загрузить шаблон отчета из файла " + fileName, ex);
            }        
        }
    }
}
