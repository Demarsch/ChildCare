using Core;
using DataLib;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Linq;

namespace Registry
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
            PersonAddresses = new ObservableCollection<PersonAddressViewModel>(service.GetPersonAddresses(this.personId).Select(x => new PersonAddressViewModel(x, service)));
            ListAddressTypes = new ObservableCollection<AddressType>(service.GetAddressTypes());
            AddPersonAddressCommand = new RelayCommand(AddPersonAddress);
            DeletePersonAddressCommand = new RelayCommand<PersonAddressViewModel>(DeletePersonAddress);
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
            set
            {
                Set("PersonAddresses", ref personAddresses, value);
                RaisePropertyChanged("PersonAddressesHasNoItems");
            }
        }

        public bool PersonAddressesHasNoItems
        {
            get { return PersonAddresses == null || PersonAddresses.Count < 1; }
        }


        public string PersonAddressesString
        {
            get
            {
                var resStr = string.Empty;
                var dateTimeNow = DateTime.Now;
                foreach (var personAddress in PersonAddresses.Where(x => dateTimeNow >= x.BeginDate && dateTimeNow < x.EndDate))
                {
                    if (resStr != string.Empty)
                        resStr += "\r\n";
                    resStr += personAddress.PersonAddressString;
                }
                return resStr;
            }
        }

        #endregion

        #region Commands

        public ICommand AddPersonAddressCommand { get; set; }
        private void AddPersonAddress()
        {
            PersonAddresses.Add(new PersonAddressViewModel(new PersonAddress() { EndDateTime = DateTime.MaxValue.Date }, service));
            RaisePropertyChanged("PersonAddressesHasNoItems");
        }

        public ICommand DeletePersonAddressCommand { get; set; }
        private void DeletePersonAddress(PersonAddressViewModel personAddressViewModel)
        {
            PersonAddresses.Remove(personAddressViewModel);
            RaisePropertyChanged("PersonAddressesHasNoItems");
        }

        #endregion

        #region Implementation IDialogViewModel

        public string Title
        {
            get { return "Адресные данные"; }
        }

        public string ConfirmButtonText
        {
            get { return "Применить"; }
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

        public bool CanBeClosed()
        {
            //TODO: put logic here if you want to respond to window close event and save your changes
            return true;
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
