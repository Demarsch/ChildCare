using Core;
using DataLib;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Linq;

namespace Registry
{
    public class PersonIdentityDocumentsViewModel : ObservableObject, IDialogViewModel
    {
        #region Fields

        private int personId;

        private IPersonService service;

        private readonly IDialogService dialogService;

        #endregion fields

        #region Constructors

        public PersonIdentityDocumentsViewModel(int personId, IPersonService service, IDialogService dialogService)
        {
            if (service == null)
                throw new ArgumentNullException("service");
            if (dialogService == null)
                throw new ArgumentNullException("dialogService");
            this.dialogService = dialogService;
            this.service = service;
            this.personId = personId;
            PersonIdentityDocuments = new ObservableCollection<PersonIdentityDocumentViewModel>(service.GetPersonIdentityDocuments(this.personId).Select(x => new PersonIdentityDocumentViewModel(x, service)));
            ListIdentityDocumentTypes = new ObservableCollection<IdentityDocumentType>(service.GetIdentityDocumentTypes());
            AddPersonIdentityDocumentCommand = new RelayCommand(AddPersonIdentityDocument);
            DeletePersonIdentityDocumentCommand = new RelayCommand<PersonIdentityDocumentViewModel>(DeleteIdentityDocument);
            CloseCommand = new RelayCommand<object>(x => Close((bool?)x));
        }

        #endregion

        #region Properties

        private ObservableCollection<IdentityDocumentType> listIdentityDocumentTypes = new ObservableCollection<IdentityDocumentType>();
        public ObservableCollection<IdentityDocumentType> ListIdentityDocumentTypes
        {
            get { return listIdentityDocumentTypes; }
            set { Set("ListIdentityDocumentTypes", ref listIdentityDocumentTypes, value); }
        }

        private ObservableCollection<PersonIdentityDocumentViewModel> personIdentityDocuments = new ObservableCollection<PersonIdentityDocumentViewModel>();
        public ObservableCollection<PersonIdentityDocumentViewModel> PersonIdentityDocuments
        {
            get { return personIdentityDocuments; }
            set
            {
                Set("PersonIdentityDocuments", ref personIdentityDocuments, value);
                RaisePropertyChanged("PersonIdentityDocumentsHasNoItems");
            }
        }

        public bool PersonIdentityDocumentsHasNoItems
        {
            get { return PersonIdentityDocuments == null || PersonIdentityDocuments.Count < 1; }
        }

        public string PersonIdentityDocumentsString
        {
            get
            {
                var resStr = string.Empty;
                var dateTimeNow = DateTime.Now;
                foreach (var personAddress in PersonIdentityDocuments.Where(x => dateTimeNow >= x.BeginDate && dateTimeNow < x.EndDate))
                {
                    if (resStr != string.Empty)
                        resStr += "\r\n";
                    resStr += personAddress.PersonAddressString;
                }
                return resStr;
            }
        }

        #endregion

        #region Methods

        public List<PersonIdentityDocument> GetUnsavedPersonIdentityDocuments()
        {
            List<PersonIdentityDocument> listPersonIdentityDocument = new List<PersonIdentityDocument>();
            PersonIdentityDocument personIdentityDocument = null;
            foreach (var personIdentityDocumentViewModel in PersonIdentityDocuments)
            {
                personIdentityDocument = personIdentityDocumentViewModel.SetData();
                personIdentityDocument.PersonId = personId;
                listPersonIdentityDocument.Add(personIdentityDocument);
            }
            return listPersonIdentityDocument;
        }

        #endregion

        #region Commands

        public ICommand AddPersonIdentityDocumentCommand { get; set; }
        private void AddPersonIdentityDocument()
        {
            PersonIdentityDocuments.Add(new PersonIdentityDocumentViewModel(new PersonIdentityDocument() { EndDate = DateTime.MaxValue.Date }, service));
            RaisePropertyChanged("PersonIdentityDocumentsHasNoItems");
        }

        public ICommand DeletePersonIdentityDocumentCommand { get; set; }
        private void DeleteIdentityDocument(PersonIdentityDocumentViewModel personIdentityDocumentViewModel)
        {
            PersonIdentityDocuments.Remove(personIdentityDocumentViewModel);
            RaisePropertyChanged("PersonIdentityDocumentsHasNoItems");
        }

        #endregion

        #region Implementation IDialogViewModel

        public string Title
        {
            get { return "Удостоверения личности"; }
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
                foreach (var personIdentityDocumentsViewModel in PersonIdentityDocuments)
                {
                    notEroors &= personIdentityDocumentsViewModel.Invalidate();
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
