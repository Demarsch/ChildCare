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

namespace AdminModule.ViewModels
{
    public class ReportTemplatesManagerViewModel : BindableBase, IConfirmNavigationRequest, IDataErrorInfo
    {
        ILog log;
        IReportTemplateService templateService;
        Func<ReportTemplateEditorViewModel> templateEditorCreator;

        public ReportTemplatesManagerViewModel(ILog log, IReportTemplateService templateService, Func<ReportTemplateEditorViewModel> templateEditorCreator)
        {
            this.log = log;
            this.templateService = templateService;
            this.templateEditorCreator = templateEditorCreator;
        }

        #region NavigationReguest

        public void ConfirmNavigationRequest(NavigationContext navigationContext, Action<bool> continuationCallback)
        {
            continuationCallback(true);
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
        }

        #endregion

        private ObservableCollectionEx<ReportTemplateEditorViewModel> items;
        public ObservableCollectionEx<ReportTemplateEditorViewModel> Items
        { 
            get
            {
                if (items != null)
                    return items;

                items = new ObservableCollectionEx<ReportTemplateEditorViewModel>();
                Initialize();
                return items;
            }
        }

        public void ReloadTemplates()
        {
            Initialize();
        }

        public async void Initialize()
        {
            var t = await Task.Run(() => templateService.GetAllInfoes().ToArray());
            while (Items.Count < t.Count() + 1)
                Items.Add(templateEditorCreator());
            while (Items.Count > t.Count() + 1)
                Items.RemoveAt(Items.Count - 1);
            Items[0].Initialize(null, ReloadTemplates);
            for (int i = 0; i < t.Count(); i++)
                Items[i + 1].Initialize(t[i], ReloadTemplates);
        }

        public string Error
        {
            get { throw new NotImplementedException(); }
        }

        public string this[string columnName]
        {
            get { throw new NotImplementedException(); }
        }
    }
}
