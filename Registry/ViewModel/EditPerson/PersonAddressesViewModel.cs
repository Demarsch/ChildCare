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
    public class PersonAddressesViewModel : ObservableObject, IDialogViewModel
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
            CloseCommand = new RelayCommand<object>(x => Close((bool?)x));
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
            PersonAddresses.Add(new PersonAddressViewModel(new PersonAddress() { BeginDateTime = new DateTime(DateTime.Now.Year, 1, 1), EndDateTime = DateTime.MaxValue.Date }, service));
            RaisePropertyChanged("PersonAddressesHasNoItems");
        }

        public ICommand DeletePersonAddressCommand { get; set; }
        private void DeletePersonAddress(PersonAddressViewModel personAddressViewModel)
        {
            PersonAddresses.Remove(personAddressViewModel);
            RaisePropertyChanged("PersonAddressesHasNoItems");
        }

        #endregion

        #region Methods

        public List<PersonAddress> GetUnsavedPersonAddresses()
        {
            List<PersonAddress> listPersonAddresses = new List<PersonAddress>();
            PersonAddress personAddress = null;
            foreach (var personAddressViewModel in PersonAddresses)
            {
                personAddress = personAddressViewModel.SetData();
                //personAddress.PersonId = personId;
                listPersonAddresses.Add(personAddress);
            }
            return listPersonAddresses;
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

        public RelayCommand<object> CloseCommand { get; set; }

        private void Close(bool? validate)
        {
            saveWasRequested = true;
            if (validate == true)
            {
                var notEroors = true;
                foreach (var personAddressesViewModel in PersonAddresses)
                {
                    notEroors &= personAddressesViewModel.Invalidate();
                }
                if (notEroors)
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
    }
}
