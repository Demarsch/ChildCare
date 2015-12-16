using Core.Wpf.Mvvm;
using Core.Wpf.Services;
using log4net;
using Prism.Commands;
using Prism.Mvvm;
using Shared.PatientRecords.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace Shared.PatientRecords.ViewModels
{
    public class AnalyseRefferenceCollectionViewModel : BindableBase, IDisposable, IDialogViewModel, IDataErrorInfo
    {
        private readonly IPatientRecordsService recordService;
        private readonly ILog logService;
        private readonly IDialogService messageService;

        public AnalyseRefferenceCollectionViewModel(IPatientRecordsService recordService, IDialogService messageService, ILog logService)
        {
            if (logService == null)
            {
                throw new ArgumentNullException("logService");
            }
            if (recordService == null)
            {
                throw new ArgumentNullException("recordService");
            }            
            if (messageService == null)
            {
                throw new ArgumentNullException("messageService");
            }            
            this.recordService = recordService;
            this.logService = logService;
            this.messageService = messageService;

            CloseCommand = new DelegateCommand<bool?>(Close);            
        }

        internal async void Initialize(int recordTypeId, int parameterRecordTypeId)
        {

        }

        private void SaveAnalyseRefferences()
        {
            
        }



        #region IDialogViewModel

        public string Title
        {
            get { return "Редактор референсных значений для анализов"; }
        }

        public string ConfirmButtonText
        {
            get { return "Сохранить"; }
        }

        public string CancelButtonText
        {
            get { return "Отмена"; }
        }

        public DelegateCommand<bool?> CloseCommand { get; private set; }

        public bool SaveIsSuccessful = false;

        private void Close(bool? validate)
        {
            saveWasRequested = true;
            if (validate == true)
            {
                if (IsValid)
                {
                    SaveAnalyseRefferences();
                    SaveIsSuccessful = true;
                }
                else
                {
                    messageService.ShowWarning("Проверьте правильность заполнения полей.");
                }
            }
            else
            {
                OnCloseRequested(new ReturnEventArgs<bool>(false));
            }
        }

        public event EventHandler<ReturnEventArgs<bool>> CloseRequested;

        protected virtual void OnCloseRequested(ReturnEventArgs<bool> e)
        {
            var handler = CloseRequested;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        #endregion

        public void Dispose()
        {
        }

        #region IDataError

        private bool saveWasRequested;

        private readonly HashSet<string> invalidProperties = new HashSet<string>();

        private bool IsValid
        {
            get
            {
                saveWasRequested = true;
                OnPropertyChanged(string.Empty);
                return invalidProperties.Count < 1;
            }
        }

        string IDataErrorInfo.Error
        {
            get { throw new NotImplementedException(); }
        }

        public string this[string columnName]
        {
            get
            {
                if (!saveWasRequested)
                {
                    invalidProperties.Remove(columnName);
                    return string.Empty;
                }
                var result = string.Empty;
                if (columnName == "SelectedAnalyseId")
                {
                    //result = selectedAnalyseId.IsNewOrNonExisting() ? "Укажите наименование исследования" : string.Empty;
                }                

                if (string.IsNullOrEmpty(result))
                {
                    invalidProperties.Remove(columnName);
                }
                else
                {
                    invalidProperties.Add(columnName);
                }
                return result;
            }
        }

        #endregion
    }
}
