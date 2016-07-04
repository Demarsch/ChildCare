using System;
using Prism.Mvvm;

namespace Core.Reports
{
    public class PrintedDocumentViewModel : BindableBase
    {
        private int templateId;
        public int TemplateId
        {
            get { return templateId; }
            set { SetProperty(ref templateId, value); }
        }
       
        private bool isChecked;
        public bool IsChecked
        {
            get { return isChecked; }
            set { SetProperty(ref isChecked, value); }
        }

        private string systemName;
        public string SystemName
        {
            get { return systemName; }
            set { SetProperty(ref systemName, value); }
        }

        private string reportFullName;
        public string ReportFullName
        {
            get { return reportFullName; }
            set { SetProperty(ref reportFullName, value); }
        }

        private string reportShortName;
        public string ReportShortName
        {
            get { return reportShortName; }
            set { SetProperty(ref reportShortName, value); }
        }

        private string options;
        public string Options
        {
            get { return options; }
            set { SetProperty(ref options, value); }
        }
    }
}
