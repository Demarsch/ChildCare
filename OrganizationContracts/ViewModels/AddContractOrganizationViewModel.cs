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

namespace OrganizationContractsModule.ViewModels
{
    public class AddContractOrganizationViewModel : BindableBase, INotification, IPopupWindowActionAware, IDataErrorInfo
    {
        private readonly IContractService contractService;
        private readonly ILog logService;
        public BusyMediator BusyMediator { get; set; }
        public FailureMediator FailureMediator { get; private set; }
        private readonly CommandWrapper saveChangesCommandWrapper;
        public int orgId = SpecialValues.NonExistingId;
        public bool saveSuccesfull = false;

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
            saveChangesCommandWrapper = new CommandWrapper { Command = new DelegateCommand(() => SaveChangesAsync()) };
            CreateOrgCommand = new DelegateCommand(SaveChangesAsync);
            CancelCommand = new DelegateCommand(Cancel);
        }

        public void IntializeCreation(string title)
        {  
            this.Title = title;
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

        public ICommand CancelCommand { get; private set; }
        private void Cancel()
        {
            saveSuccesfull = false;
            HostWindow.Close();
        }

        public ICommand CreateOrgCommand { get; private set; }
        private async void SaveChangesAsync()
        {
            FailureMediator.Deactivate();
            if (!IsValid)
            {
                return;
            }
            logService.InfoFormat("Saving data for new Org");
            BusyMediator.Activate("Сохранение изменений...");
            var saveSuccesfull = false;
            try
            {
                Org org = new Org();
                org.Name = Name;
                org.Details = Details;
                org.IsLpu = false;
                org.UseInContract = true;
                var result = await contractService.SaveOrgAsync(org);
                orgId = result;
                saveSuccesfull = true;
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
                if (saveSuccesfull)
                    HostWindow.Close();
            }
        }
        
        #region IPopupWindowActionAware implementation
        public System.Windows.Window HostWindow { get; set; }

        public INotification HostNotification { get; set; }
        #endregion

        #region INotification implementation
        public object Content { get; set; }

        public string Title { get; set; }
        #endregion

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
    }
}
