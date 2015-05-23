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
    public class PersonSocialStatusesViewModel : ObservableObject, IDialogViewModel, IDataErrorInfo
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
            CloseCommand = new RelayCommand<bool>(Close);
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
