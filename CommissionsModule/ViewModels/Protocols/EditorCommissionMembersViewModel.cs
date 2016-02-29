using CommissionsModule.Services;
using Core.Data.Misc;
using Core.Data.Services;
using Core.Services;
using Core.Wpf.Mvvm;
using Core.Wpf.Services;
using log4net;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace CommissionsModule.ViewModels
{
    public class EditorCommissionMembersViewModel : BindableBase, IDialogViewModel, IDataErrorInfo
    {
        private readonly ICommissionService commissionService;
        private readonly ILog logService;
        private readonly IDialogService messageService;
        private readonly ICacheService cacheService;
        private readonly IUserService userService;
        private CancellationTokenSource currentSavingToken;

        public EditorCommissionMembersViewModel(ICommissionService commissionService,
                                      IDialogServiceAsync dialogService,
                                      IDialogService messageService,
                                      ILog logService,
                                      ICacheService cacheService, 
                                      IUserService userService)
        {
            if (logService == null)
            {
                throw new ArgumentNullException("logService");
            }            
            if (dialogService == null)
            {
                throw new ArgumentNullException("dialogService");
            }
            if (messageService == null)
            {
                throw new ArgumentNullException("messageService");
            }
            if (userService == null)
            {
                throw new ArgumentNullException("userService");
            }
            if (cacheService == null)
            {
                throw new ArgumentNullException("cacheService");
            }            
            this.logService = logService;
            this.commissionService = commissionService;
            this.messageService = messageService;
            this.cacheService = cacheService;
            this.userService = userService;

            BusyMediator = new BusyMediator();
            CloseCommand = new DelegateCommand<bool?>(Close);
        }

        #region Properties
        public BusyMediator BusyMediator { get; set; }

        #endregion

        private bool HasAllRequiredFields()
        {
            throw new NotImplementedException();
        }

        private Task SaveCommissionMembers()
        {
            throw new NotImplementedException();
        }  

        #region IDialogViewModel

        public string Title
        {
            get { return "Состав комиссии"; }
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

        private async void Close(bool? validate)
        {
            if (validate == true)
            {
                if (HasAllRequiredFields() && IsValid)
                {
                    await SaveCommissionMembers();
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
                /*if (columnName == "TalonNumber")
                {
                    result = string.IsNullOrEmpty(TalonNumber) ? "Укажите номер талона" : string.Empty;
                } */               
                if (string.IsNullOrEmpty(result))
                    invalidProperties.Remove(columnName);
                else
                    invalidProperties.Add(columnName);
                return result;
            }
        }

        #endregion

        internal void Initialize(int commissionProtocolId)
        {
            
        }
    }
}
