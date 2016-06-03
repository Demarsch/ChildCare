using Core.Data;
using Core.Data.Misc;
using Core.Wpf.Mvvm;
using Core.Wpf.PopupWindowActionAware;
using Core.Wpf.Services;
using log4net;
using Prism.Commands;
using Prism.Interactivity.InteractionRequest;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Data.Entity;
using Core.Extensions;
using Core.Wpf.Misc;
using Core.Misc;
using OrganizationContractsModule.Services;
using System.Windows.Navigation;

namespace OrganizationContractsModule.ViewModels
{
    public class AddContractOrganizationViewModel : BindableBase, IDataErrorInfo, IDisposable, IDialogViewModel
    {
        private readonly IContractService contractService;
        private readonly ILog logService;
        public BusyMediator BusyMediator { get; set; }
        public FailureMediator FailureMediator { get; private set; }
        private readonly CommandWrapper saveChangesCommandWrapper;
        public int orgId = SpecialValues.NonExistingId;
        public bool SaveSuccesfull { get; private set; }

        public AddContractOrganizationViewModel(IContractService contractService, ILog logService)
        {
            if (logService == null)
            {
                throw new ArgumentNullException("logService");
            }
            if (contractService == null)
            {
                throw new ArgumentNullException("contractService");
            }           
            this.contractService = contractService;
            this.logService = logService;
            
            BusyMediator = new BusyMediator();
            FailureMediator = new FailureMediator();
            saveChangesCommandWrapper = new CommandWrapper { Command = new DelegateCommand(() => SaveChangesAsync()), CommandName = "Повторить" };            
            SaveSuccesfull = false;
            CloseCommand = new DelegateCommand<bool?>(Close);
        }
        
        private string name;
        public string Name
        {
            get { return name; }
            set { SetProperty(ref name, value); }
        }

        private string details;
        public string Details
        {
            get { return details; }
            set { SetProperty(ref details, value); }
        }       

        public ICommand CreateOrgCommand { get; private set; }
        private async void SaveChangesAsync()
        {
            FailureMediator.Deactivate();            
            logService.InfoFormat("Saving data for new Org");
            BusyMediator.Activate("Сохранение изменений...");
            try
            {
                SaveSuccesfull = false;
                Org org = new Org();
                org.Name = Name;
                org.Details = Details.ToSafeString();
                org.IsLpu = false;
                org.UseInContract = true;
                org.BeginDateTime = DateTime.Now;
                org.EndDateTime = DateTime.MaxValue;
                var result = await contractService.SaveOrgAsync(org);
                orgId = result;
                SaveSuccesfull = true;
            }
            catch (OperationCanceledException)
            {
                //Nothing to do as it means that we somehow cancelled save operation
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to save data for org");
                FailureMediator.Activate("Не удалось сохранить данные новой организации. Попробуйте еще раз или обратитесь в службу поддержки", saveChangesCommandWrapper, ex);
            }
            finally
            {
                BusyMediator.Deactivate();
            }
        }
             
        #region IDataErrorInfo implementation
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

        string IDataErrorInfo.this[string columnName]
        {
            get
            {
                if (!saveWasRequested)
                {
                    invalidProperties.Remove(columnName);
                    return string.Empty;
                }
                var result = string.Empty;
                switch (columnName)
                {
                    case "Name":
                        result = string.IsNullOrEmpty(Name) ? "Укажите название организации" : string.Empty;
                        break;                   
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

        string IDataErrorInfo.Error
        {
            get { throw new NotImplementedException(); }
        }
        #endregion

        #region IDialogViewModel

        public string Title
        {
            get { return "Новая организация"; }
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

        private void Close(bool? validate)
        {
            saveWasRequested = true;
            if (validate == true)
            {
                if (IsValid)          
                {
                    SaveChangesAsync();
                    OnCloseRequested(new ReturnEventArgs<bool>(true));                
                }
            }
            else
                OnCloseRequested(new ReturnEventArgs<bool>(false));
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
            saveChangesCommandWrapper.Dispose();
        }
    }
}
