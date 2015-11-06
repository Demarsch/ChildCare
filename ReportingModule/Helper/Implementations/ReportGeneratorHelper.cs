using ReportingModule.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReportingModule
{
    public class ReportGeneratorHelper : IReportGeneratorHelper
    {
        private IReportModuleFileOperations fileOperations;
        private IReportTemplateService templateService;

        public ReportGeneratorHelper(IReportModuleFileOperations fileOperations, IReportTemplateService templateService)
        {
            this.fileOperations = fileOperations;
            this.templateService = templateService;
        }
        
        public IReportGenerator CreateDocX(string templateName)
        {
            try
            {
                var t = templateService.GetTemplate(templateName);
                if (!t.IsDocXTemplate)
                    throw new ArgumentException(string.Format("Шаблона отчета {0} не отмечен как DocX", templateName));
                return new DocXReportGenerator(fileOperations) { Template = t.Template, Title = t.Title };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Не удалось загрузить шаблон отчета " + templateName, ex);
            }            
        }
    }
}
