using Core;
using DataLib;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MainLib
{
    class PersonAddressesViewModel : ObservableObject, IDialogViewModel
    {
        #region Fields

        private int personId;

        private IPersonService service;

        #endregion fields

        #region Constructors

        public PersonAddressesViewModel(int personId, IPersonService service)
        {
            if (service == null)
                throw new ArgumentNullException("service");
            this.service = service;
            this.personId = personId;
            AddIPersonAddressCommand = new RelayCommand(AddPersonAddress);
            DeletePersonAddressCommand = new RelayCommand<PersonAddressViewModel>(DeleteInsuranceDocument);
            CancelCommand = new RelayCommand(Cancel);
            AcceptCommand = new RelayCommand(Accept);
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

        private bool? isChangesAccepted;
        public bool? IsChangesAccepted
        {
            get { return isChangesAccepted; }
            set { Set("IsChangesAccepted", ref isChangesAccepted, value); }
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

        public ICommand CancelCommand { get; set; }
        private void Cancel()
        {
            IsChangesAccepted = false;
        }

        public ICommand AcceptCommand { get; set; }
        private void Accept()
        {
            IsChangesAccepted = true;
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

        public RelayCommand<bool> CloseCommand
        {
            get { throw new NotImplementedException(); }
        }

        public event EventHandler<System.Windows.Navigation.ReturnEventArgs<bool>> CloseRequested;

        #endregion
    }
}
