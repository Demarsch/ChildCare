using System;
using System.Data.Entity;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Data;
using log4net;
using Prism;
using Prism.Mvvm;
using Prism.Commands;
using Prism.Interactivity;
using Prism.Events;
using Prism.Common;
using Prism.Regions;
using Core.Data;
using Core.Data.Misc;
using Core.Data.Services;
using Core.Extensions;
using Core.Misc;
using Core.Services;
using Core.Wpf.Events;
using Core.Wpf.Misc;
using Core.Wpf.Mvvm;
using Core.Wpf.Services;
using Shell.Shared;
using Core.Reports.Services;
using Core.Reports.DTO;
using Core.Reports;

namespace AdminModule.ViewModels
{
    public class ReportTemplateEditorViewModel : BindableBase
    {
        ILog log;
        IReportTemplateService templateService;
        IFileService fileService;
        IReportGeneratorHelper reportHelper;

        public ReportTemplateEditorViewModel(ILog log, IReportTemplateService templateService, IFileService fileService, IReportGeneratorHelper reportHelper)
        {
            this.log = log;
            this.templateService = templateService;
            this.fileService = fileService;
            this.reportHelper = reportHelper;
        }

        private int templateId;
        public int TemplateId { get { return templateId; } set { SetProperty(ref templateId, value); } }

        private string templateName;
        public string TemplateName { get { return templateName; } set { SetProperty(ref templateName, value); } }

        private string templateTitle;
        public string TemplateTitle { get { return templateTitle; } set { SetProperty(ref templateTitle, value); } }

        private string templateDescription;
        public string TemplateDescription { get { return templateDescription; } set { SetProperty(ref templateDescription, value); } }

        private bool templateIsDocX;
        public bool TemplateIsDocX { get { return templateIsDocX; } set { SetProperty(ref templateIsDocX, value); } }

        private Action reloadCallback;

        public void Initialize(ReportTemplateDTOInfo repTemplate, Action reloadCallback)
        {
            this.reloadCallback = reloadCallback;
            if (repTemplate != null)
            {
                TemplateId = repTemplate.Id;
                TemplateName = repTemplate.Name;
                TemplateTitle = repTemplate.Title;
                TemplateDescription = repTemplate.Description;
                TemplateItemName = TemplateName;
                TemplateIsDocX = repTemplate.IsDocXTemplate;
                EmptyTemplate = !TemplateIsDocX;
            }
            else
            {
                TemplateId = 0;
                TemplateName = string.Empty;
                TemplateTitle = string.Empty;
                TemplateDescription = string.Empty;
                TemplateItemName = "[новый шаблон]";
                TemplateIsDocX = false;
                EmptyTemplate = false;
            }
            OpenedInEditor = false;
        }

        private string templateItemName;
        public string TemplateItemName { get { return templateItemName; } set { SetProperty(ref templateItemName, value); } }

        private DelegateCommand saveCommand;
        public DelegateCommand SaveCommand { get { return saveCommand ?? (saveCommand = new DelegateCommand(SaveCommandAction)); } }
        private void SaveCommandAction()
        {
            if (TemplateName.Length == 0)
            {
                MessageText = "Не указан идентификатор шаблона";
                MessageState = true;
                return;
            }

            if (templateService.CheckNameInUse(TemplateName, TemplateId))
            {
                MessageText = string.Format("Другой отчет с идентификатором {0} уже существует", TemplateName);
                MessageState = true;
                return;
            }

            if (TemplateTitle.Length == 0)
            {
                MessageText = "Не указан заголовок шаблона";
                MessageState = true;
                return;
            }

            var inv = fileService.FileNameInvalidChars(TemplateTitle);
            if (inv.Length > 0)
            {
                MessageText = string.Format("Заголовок содержит недопустимые символы ( {0} )", inv);
                MessageState = true;
                return;
            }

            var temp = new ReportTemplateDTOInfo()
            {
                Id = TemplateId,
                Description = TemplateDescription,
                IsDocXTemplate = TemplateIsDocX,
                Name = TemplateName,
                Title = TemplateTitle
            };

            var id = templateService.SaveTemplateInfo(temp);

            TemplateItemName = TemplateName;

            if (TemplateId != 0)
                return;

            TemplateId = id;

            EmptyTemplate = !TemplateIsDocX;

            if (reloadCallback != null)
                reloadCallback();
        }

        private bool openedInEditor;
        public bool OpenedInEditor { get { return openedInEditor; } set { SetProperty(ref openedInEditor, value); } }

        private bool emptyTemplate;
        public bool EmptyTemplate { get { return emptyTemplate; } set { SetProperty(ref emptyTemplate, value); } }

        private string reportFileName;

        private DelegateCommand openCommand;
        public DelegateCommand OpenCommand { get { return openCommand ?? (openCommand = new DelegateCommand(OpenCommandAction)); } }
        public void OpenCommandAction()
        {
            // from base for editing
            try
            { 
                if (TemplateIsDocX)
                {
                    using (var rep = reportHelper.CreateDocX(TemplateItemName))
                    {
                        rep.Title = "Шаблон";
                        rep.Editable = true;
                        reportFileName = rep.Show();
                        OpenedInEditor = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageText = ex.Message;
                MessageState = true;
            }
        }

        private DelegateCommand fileCommand;
        public DelegateCommand FileCommand { get { return fileCommand ?? (fileCommand = new DelegateCommand(FileCommandAction)); } }
        public void FileCommandAction()
        {
            // from file
            if (EmptyTemplate)
            {
                var res = fileService.OpenFileDialog(false, FileServiceFilters.DocX);
                if (!res.Any())
                    return;

                try
                {
                    if (res[0].EndsWith(FileServiceFilters.DocXExtention, StringComparison.CurrentCultureIgnoreCase))
                    {
                        using (var rep = reportHelper.CreateDocXFromFile(res[0]))
                        {
                            templateService.SaveTemplate(TemplateItemName, rep.Template, true);
                            OpenedInEditor = false;
                            TemplateIsDocX = true;
                            EmptyTemplate = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageText = ex.Message;
                    MessageState = true;
                }
            }
        }
	      
        private DelegateCommand loadCommand;
        public DelegateCommand LoadCommand { get { return loadCommand ?? (loadCommand = new DelegateCommand(LoadCommandAction)); } }
        public void LoadCommandAction()
        {
            // save to base
            if (!OpenedInEditor)
                return;

            try
            {
                if (TemplateIsDocX)
                {
                    using (var rep = reportHelper.CreateDocXFromFile(reportFileName))
                    {
                        templateService.SaveTemplate(TemplateItemName, rep.Template, true);
                        OpenedInEditor = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageText = ex.Message;
                MessageState = true;
            }
        }
        
        private bool messageState;
        public bool MessageState { get { return messageState; } set { SetProperty(ref messageState, value); } }

        private string messageText;
        public string MessageText { get { return messageText; } set { SetProperty(ref messageText, value); } }

        private DelegateCommand closeMessageCommand;
        public DelegateCommand CloseMessageCommand { get { return closeMessageCommand ?? (closeMessageCommand = new DelegateCommand(() => MessageState = false)); } }
        
    }
}
