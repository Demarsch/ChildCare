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
    public class PersonDisabilitiesViewModel : ObservableObject, IDialogViewModel
    {
        #region Fields

        private int personId;

        private IPersonService service;

        private readonly IDialogService dialogService;

        #endregion fields

        #region Constructors

        public PersonDisabilitiesViewModel(int personId, IPersonService service, IDialogService dialogService)
        {
            if (service == null)
                throw new ArgumentNullException("service");
            if (dialogService == null)
                throw new ArgumentNullException("dialogService");
            this.dialogService = dialogService;
            this.service = service;
            this.personId = personId;
            PersonDisabilities = new ObservableCollection<PersonDisabilityViewModel>(service.GetPersonDisabilities(this.personId).Select(x => new PersonDisabilityViewModel(x, service)));
            ListDisabilityTypes = new ObservableCollection<DisabilityType>(service.GetDisabilityTypes());
            AddPersonDisabilityCommand = new RelayCommand(AddPersonDisability);
            DeletePersonDisabilityCommand = new RelayCommand<PersonDisabilityViewModel>(DeletePersonDisability);
            CloseCommand = new RelayCommand<object>(x => Close((bool?)x));
        }

        #endregion

        #region Properties

        private ObservableCollection<DisabilityType> listDisabilityTypes = new ObservableCollection<DisabilityType>();
        public ObservableCollection<DisabilityType> ListDisabilityTypes
        {
            get { return listDisabilityTypes; }
            set { Set("ListDisabilityTypes", ref listDisabilityTypes, value); }
        }

        private ObservableCollection<PersonDisabilityViewModel> personDisabilities = new ObservableCollection<PersonDisabilityViewModel>();
        public ObservableCollection<PersonDisabilityViewModel> PersonDisabilities
        {
            get { return personDisabilities; }
            set
            {
                Set("PersonDisabilities", ref personDisabilities, value);
                RaisePropertyChanged("PersonDisabilitiesHasNoItems");
            }
        }

        public bool PersonDisabilitiesHasNoItems
        {
            get { return PersonDisabilities == null || PersonDisabilities.Count < 1; }
        }

        public string PersonDisabilitiesString
        {
            get
            {
                var resStr = string.Empty;
                var dateTimeNow = DateTime.Now;
                foreach (var personDisability in PersonDisabilities.Where(x => dateTimeNow >= x.BeginDate && dateTimeNow < x.EndDate))
                {
                    if (resStr != string.Empty)
                        resStr += "\r\n";
                    resStr += personDisability.PersonDisabilityString;
                }
                return resStr;
            }
        }

        #endregion

        #region Methods

        public List<PersonDisability> GetUnsavedPersonDisabilities()
        {
            List<PersonDisability> listPersonDisabilities = new List<PersonDisability>();
            PersonDisability personDisability = null;
            foreach (var personDisabilitiesViewModel in PersonDisabilities)
            {
                personDisability = personDisabilitiesViewModel.SetData();
                personDisability.PersonId = personId;
                listPersonDisabilities.Add(personDisability);
            }
            return listPersonDisabilities;
        }
        #endregion

        #region Commands

        public ICommand AddPersonDisabilityCommand { get; set; }
        private void AddPersonDisability()
        {
            PersonDisabilities.Add(new PersonDisabilityViewModel(new PersonDisability(), service));
            RaisePropertyChanged("PersonDisabilitiesHasNoItems");
        }

        public ICommand DeletePersonDisabilityCommand { get; set; }
        private void DeletePersonDisability(PersonDisabilityViewModel personDisabilityViewModel)
        {
            PersonDisabilities.Remove(personDisabilityViewModel);
            RaisePropertyChanged("PersonDisabilitiesHasNoItems");
        }

        #endregion

        #region Implementation IDialogViewModel

        public string Title
        {
            get { return "Инвалидность"; }
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
                foreach (var personDisabilitiesDocumentsViewModel in PersonDisabilities)
                {
                    notEroors &= personDisabilitiesDocumentsViewModel.Invalidate();
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
