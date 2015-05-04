using Core;
using DataLib;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Navigation;

namespace MainLib
{
    public class PersonAddressesViewModel : ObservableObject, IDialogViewModel, IDataErrorInfo
    {
        #region Fields

        private int personId;

        private IPersonService service;

        private readonly IDialogService dialogService;

        #endregion fields

        #region Constructors

        public PersonAddressesViewModel(int personId, IPersonService service, IDialogService dialogService)
        {
            if (service == null)
                throw new ArgumentNullException("service");
            if (dialogService == null)
                throw new ArgumentNullException("dialogService");
            this.dialogService = dialogService;
            this.service = service;
            this.personId = personId;
            AddIPersonAddressCommand = new RelayCommand(AddPersonAddress);
            DeletePersonAddressCommand = new RelayCommand<PersonAddressViewModel>(DeleteInsuranceDocument);
            CloseCommand = new RelayCommand<bool>(Close);
        }

        #endregion

        #region Properties

        private ObservableCollection<AddressType> listAddressTypes = new ObservableCollection<AddressType>();
        public ObservableCollection<AddressType> ListAddressTypes
        {
            get { return listAddressTypes; }
            set { Set("ListAddressTypes", ref listAddressTypes, value); }
        }

        private ObservableCollection<PersonAddressViewModel> personAddresses = new ObservableCollection<PersonAddressViewModel>();
        public ObservableCollection<PersonAddressViewModel> PersonAddresses
        {
            get { return personAddresses; }
            set { Set("PersonAddresses", ref personAddresses, value); }
        }

        private DateTime beginDate = DateTime.MinValue;
        public DateTime BeginDate
        {
            get { return beginDate; }
            set
            {
                Set("BeginDate", ref beginDate, value);
                RaisePropertyChanged("PersonAddressState");
            }
        }

        private DateTime endDate = DateTime.MinValue;
        public DateTime EndDate
        {
            get { return endDate; }
            set
            {
                Set("EndDate", ref endDate, value);
                RaisePropertyChanged("PersonAddressState");
            }
        }

        private bool withoutEndDate;
        public bool WithoutEndDate
        {
            get { return withoutEndDate; }
            set
            {
                Set("WithoutEndDate", ref withoutEndDate, value);
                EndDate = DateTime.MaxValue;
                RaisePropertyChanged("WithEndDate");
            }
        }

        public bool WithEndDate
        {
            get { return !WithoutEndDate; }
        }

        public ItemState PersonAddressState
        {
            get
            {
                var datetimeNow = DateTime.Now;
                if (datetimeNow >= BeginDate && datetimeNow < EndDate)
                    return ItemState.Active;
                return ItemState.Inactive;
            }
        }

        #endregion

        #region Commands
        public ICommand AddIPersonAddressCommand { get; set; }
        private void AddPersonAddress()
        {
            PersonAddresses.Add(new PersonAddressViewModel(new PersonAddress()));
        }

        public ICommand DeletePersonAddressCommand { get; set; }
        private void DeleteInsuranceDocument(PersonAddressViewModel personAddressViewModel)
        {
            PersonAddresses.Remove(personAddressViewModel);
        }

        #endregion

        #region Implementation IDialogViewModel

        public string Title
        {
            get { return "Адреса пациента"; }
        }

        public string ConfirmButtonText
        {
            get { return "Выбрать"; }
        }

        public string CancelButtonText
        {
            get { return "Отмена"; }
        }

        private bool saveWasRequested;
        private readonly HashSet<string> invalidProperties = new HashSet<string>();
        public RelayCommand<bool> CloseCommand { get; set; }

        private void Close(bool validate)
        {
            saveWasRequested = true;
            if (validate)
            {
                RaisePropertyChanged(string.Empty);
                if (invalidProperties.Count == 0)
                {
                    OnCloseRequested(new ReturnEventArgs<bool>(true));
                }
            }
            else
            {
                OnCloseRequested(new ReturnEventArgs<bool>(false));
            }
        }

        public event EventHandler<System.Windows.Navigation.ReturnEventArgs<bool>> CloseRequested;

        protected virtual void OnCloseRequested(ReturnEventArgs<bool> e)
        {
            var handler = CloseRequested;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        #endregion

        #region Implementation IDataErrorInfo

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
                //if (columnName == "SelectedFinancingSource")
                //{
                //    result = selectedFinancingSource == null || !selectedFinancingSource.IsActive ? "Укажите источник финансирования" : string.Empty;
                //}
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
