using Core.Data;
using Core.Data.Misc;
using Core.Services;
using Core.Wpf.Mvvm;
using Core.Wpf.Services;
using log4net;
using OrganizationContractsModule.Services;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace OrganizationContractsModule.ViewModels
{
    public class AddRecordViewModel : BindableBase, IDialogViewModel
    {
        private readonly IContractService contractService;
        private readonly ILog logService;
        private readonly IDialogService messageService;
        private readonly ICacheService cacheService;

        public AddRecordViewModel(IContractService contractService,
                                      IDialogService messageService,
                                      ILog logService,
                                      ICacheService cacheService)
        {
            if (logService == null)
            {
                throw new ArgumentNullException("logService");
            }           
           
            if (messageService == null)
            {
                throw new ArgumentNullException("messageService");
            }            
            if (cacheService == null)
            {
                throw new ArgumentNullException("cacheService");
            }
            
            this.logService = logService;
            this.contractService = contractService;
            this.messageService = messageService;
            this.cacheService = cacheService;
            
            BusyMediator = new BusyMediator();
            CloseCommand = new DelegateCommand<bool?>(Close);
        }

        #region Properties

        public BusyMediator BusyMediator { get; set; }

        private ObservableCollectionEx<RecordTypeViewModel> recordTypes;
        public ObservableCollectionEx<RecordTypeViewModel> RecordTypes
        {
            get { return recordTypes; }
            set { SetProperty(ref recordTypes, value); }
        }

        private RecordTypeViewModel selectedRecordType;
        public RecordTypeViewModel SelectedRecordType
        {
            get { return selectedRecordType; }
            set { SetProperty(ref selectedRecordType, value); }
        }

        #endregion

        internal void Initialize()
        {
            var query = cacheService.GetItems<RecordType>().Where(x => x.IsRecord && x.Assignable == true).Select(x => new RecordTypeViewModel() { Id = x.Id, Name = x.Name });
            RecordTypes = new ObservableCollectionEx<RecordTypeViewModel>(query);
        }        

        #region IDialogViewModel

        public string Title
        {
            get { return "Выбор услуг"; }
        }

        public string ConfirmButtonText
        {
            get { return "Выбрать"; }
        }

        public string CancelButtonText
        {
            get { return "Отмена"; }
        }

        public DelegateCommand<bool?> CloseCommand { get; private set; }

        private async void Close(bool? validate)
        {
            if (validate == true)
                OnCloseRequested(new ReturnEventArgs<bool>(true));  
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


    }
}
