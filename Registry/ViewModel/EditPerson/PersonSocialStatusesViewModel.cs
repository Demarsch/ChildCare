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
    public class PersonSocialStatusesViewModel : ObservableObject, IDialogViewModel
    {
        #region Fields

        private int personId;

        private IPersonService service;

        private readonly IDialogService dialogService;

        #endregion fields

        #region Constructors

        public PersonSocialStatusesViewModel(int personId, IPersonService service, IDialogService dialogService)
        {
            if (service == null)
                throw new ArgumentNullException("service");
            if (dialogService == null)
                throw new ArgumentNullException("dialogService");
            this.dialogService = dialogService;
            this.service = service;
            this.personId = personId;
            PersonSocialStatuses = new ObservableCollection<PersonSocialStatusViewModel>(service.GetPersonSocialStatuses(this.personId).Select(x => new PersonSocialStatusViewModel(x, service)));
            ListSocialStatusTypes = new ObservableCollection<SocialStatusType>(service.GetSocialStatusTypes());
            AddPersonSocialStatusCommand = new RelayCommand(AddPersonSocialStatus);
            DeletePersonSocialStatusCommand = new RelayCommand<PersonSocialStatusViewModel>(DeletePersonSocialStatus);
            CloseCommand = new RelayCommand<object>(x => Close((bool?)x));
        }

        #endregion

        #region Properties

        private ObservableCollection<SocialStatusType> listSocialStatusTypes = new ObservableCollection<SocialStatusType>();
        public ObservableCollection<SocialStatusType> ListSocialStatusTypes
        {
            get { return listSocialStatusTypes; }
            set { Set("ListSocialStatusTypes", ref listSocialStatusTypes, value); }
        }

        private ObservableCollection<PersonSocialStatusViewModel> personSocialStatuses = new ObservableCollection<PersonSocialStatusViewModel>();
        public ObservableCollection<PersonSocialStatusViewModel> PersonSocialStatuses
        {
            get { return personSocialStatuses; }
            set
            {
                Set("PersonSocialStatuses", ref personSocialStatuses, value);
                RaisePropertyChanged("PersonSocialStatusesHasNoItems");
            }
        }

        public bool PersonSocialStatusesHasNoItems
        {
            get { return PersonSocialStatuses == null || PersonSocialStatuses.Count < 1; }
        }

        public string PersonSocialStatusesString
        {
            get
            {
                var resStr = string.Empty;
                var dateTimeNow = DateTime.Now;
                foreach (var personSocialStatus in PersonSocialStatuses.Where(x => dateTimeNow >= x.BeginDate && dateTimeNow < x.EndDate))
                {
                    if (resStr != string.Empty)
                        resStr += "\r\n";
                    resStr += personSocialStatus.PersonSocialStatusesString;
                }
                return resStr;
            }
        }

        #endregion

        #region Methods

        public List<PersonSocialStatus> GetUnsavedPersonSocialStatuses()
        {
            List<PersonSocialStatus> listPersonSocialStatuses = new List<PersonSocialStatus>();
            PersonSocialStatus personSocialStatus = null;
            foreach (var personSocialStatusesViewModel in PersonSocialStatuses)
            {
                personSocialStatus = personSocialStatusesViewModel.SetData();
                personSocialStatus.PersonId = personId;
                listPersonSocialStatuses.Add(personSocialStatus);
            }
            return listPersonSocialStatuses;
        }

        #endregion

        #region Commands

        public ICommand AddPersonSocialStatusCommand { get; set; }
        private void AddPersonSocialStatus()
        {
            PersonSocialStatuses.Add(new PersonSocialStatusViewModel(new PersonSocialStatus() { EndDateTime = DateTime.MaxValue.Date }, service));
            RaisePropertyChanged("PersonSocialStatusesHasNoItems");
        }

        public ICommand DeletePersonSocialStatusCommand { get; set; }
        private void DeletePersonSocialStatus(PersonSocialStatusViewModel personDisabilityViewModel)
        {
            PersonSocialStatuses.Remove(personDisabilityViewModel);
            RaisePropertyChanged("PersonSocialStatusesHasNoItems");
        }

        #endregion

        #region Implementation IDialogViewModel

        public string Title
        {
            get { return "Социальный статус"; }
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
                foreach (var personSocialStatusesViewModel in PersonSocialStatuses)
                {
                    notEroors &= personSocialStatusesViewModel.Invalidate();
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
