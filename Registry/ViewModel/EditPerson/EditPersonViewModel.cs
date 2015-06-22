using Core;
using DataLib;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using log4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Linq;

namespace Registry
{
    public class EditPersonViewModel : ObservableObject
    {
        private readonly ILog log;

        private readonly IPersonService service;

        private readonly IDialogService dialogService;

        private readonly IDocumentService documentService;

        private EditPersonDataViewModel editPersonDataViewModel;
        public EditPersonDataViewModel EditPersonDataViewModel
        {
            get { return editPersonDataViewModel; }
            set
            {
                Set(() => EditPersonDataViewModel, ref editPersonDataViewModel, value);
            }
        }

        private ObservableCollection<EditPersonDataViewModel> editPersonRelativeDataViewModels;
        public ObservableCollection<EditPersonDataViewModel> EditPersonRelativeDataViewModels
        {
            get { return editPersonRelativeDataViewModels; }
            set
            {
                Set(() => EditPersonRelativeDataViewModels, ref editPersonRelativeDataViewModels, value);
            }
        }

        /// <summary>
        /// Use this for creating new person
        /// </summary>
        public EditPersonViewModel(ILog log, IPersonService service, IDialogService dialogService, IDocumentService documentService)
        {
            if (log == null)
                throw new ArgumentNullException("log");
            if (service == null)
                throw new ArgumentNullException("service");
            if (dialogService == null)
                throw new ArgumentNullException("dialogService");
            if (documentService == null)
                throw new ArgumentNullException("documentService");
            this.documentService = documentService;
            this.dialogService = dialogService;
            this.service = service;
            this.log = log;
            IsPersonEditing = true;
            ReturnToPersonEditingCommand = new RelayCommand(ReturnToPersonEditing);
            EditPersonRelativeDataViewModels = new ObservableCollection<EditPersonDataViewModel>();
            EditPersonDataViewModel = new EditPersonDataViewModel(log, service, dialogService, documentService);
            RelativeRelations = new ObservableCollection<RelativeRelationship>(service.GetRelativeRelationships());
            SaveChangesCommand = new RelayCommand(SaveChanges);
        }

        public EditPersonViewModel(ILog log, IPersonService service, IDialogService dialogService, IDocumentService documentService, int personId)
            : this(log, service, dialogService, documentService)
        {
            Id = personId;
            this.log = log;
        }

        /// <summary>
        /// TODO: Use this for creating new person with default data from search
        /// </summary>
        public EditPersonViewModel(ILog log, IPersonService service, IDialogService dialogService, IDocumentService documentService, string personData)
            : this(log, service, dialogService, documentService)
        {

        }

        private int id;
        public int Id
        {
            get { return id; }
            set
            {
                Set("Id", ref id, value);
                editPersonDataViewModel.Id = id;
                SetRelatives();
            }
        }

        public ICommand ReturnToPersonEditingCommand { get; private set; }

        private void ReturnToPersonEditing()
        {
            SelectedRelativeViewModel = null;
        }

        public ICommand SaveChangesCommand { get; set; }
        private async void SaveChanges()
        {
            var task = Task.Factory.StartNew(SaveData);
            IsSaveEnabled = false;
            await task;
            IsSaveEnabled = true;
        }

        private void SaveData()
        {
            var personNames = new List<PersonName>();
            var person = EditPersonDataViewModel.GetUnsavedPerson(out personNames);
            var personDataSaveDTO = new PersonDataSaveDTO()
            {
                Person = person,
                PersonNames = personNames,
                PersonInsuranceDocuments = EditPersonDataViewModel.GetUnsavedPersonInsuranceDocuments(),
                PersonAddresses = EditPersonDataViewModel.GetUnsavedPersonAddresses(),
                PersonIdentityDocuments = EditPersonDataViewModel.GetUnsavedPersonIdentityDocuments(),
                PersonDisabilities = EditPersonDataViewModel.GetUnsavedPersonDisabilities(),
                PersonSocialStatuses = EditPersonDataViewModel.GetUnsavedPersonSocialStatuses(),
                HealthGroupId = EditPersonDataViewModel.HealthGroupId,
                NationalityId = EditPersonDataViewModel.NationalityId,
                RelativeToPersonId = 0
            };

            var personRelativesDataSaveDTO = new List<PersonDataSaveDTO>();
            foreach (var personRelativeDataViewModels in EditPersonRelativeDataViewModels)
            {
                personNames = new List<PersonName>();
                person = personRelativeDataViewModels.GetUnsavedPerson(out personNames);
                personRelativesDataSaveDTO.Add(new PersonDataSaveDTO()
                {
                    Person = person,
                    PersonNames = personNames,
                    PersonInsuranceDocuments = personRelativeDataViewModels.GetUnsavedPersonInsuranceDocuments(),
                    PersonAddresses = personRelativeDataViewModels.GetUnsavedPersonAddresses(),
                    PersonIdentityDocuments = personRelativeDataViewModels.GetUnsavedPersonIdentityDocuments(),
                    PersonDisabilities = personRelativeDataViewModels.GetUnsavedPersonDisabilities(),
                    PersonSocialStatuses = personRelativeDataViewModels.GetUnsavedPersonSocialStatuses(),
                    HealthGroupId = personRelativeDataViewModels.HealthGroupId,
                    NationalityId = personRelativeDataViewModels.NationalityId,
                    RelativeToPersonId = personRelativeDataViewModels.RelativeToPersonId,
                    IsRepresentative = personRelativeDataViewModels.IsRepresentative,
                    RelativeRelationId = personRelativeDataViewModels.RelativeRelationId
                });
            }

            service.SavePersonData(personDataSaveDTO, personRelativesDataSaveDTO);
        }

        private bool isSaveEnabled = true;
        public bool IsSaveEnabled
        {
            get { return isSaveEnabled; }
            set { Set(() => IsSaveEnabled, ref isSaveEnabled, value); }
        }

        private EditPersonDataViewModel selectedRelativeViewModel;
        public EditPersonDataViewModel SelectedRelativeViewModel
        {
            get { return selectedRelativeViewModel; }
            set
            {
                Set(() => SelectedRelativeViewModel, ref selectedRelativeViewModel, value);
                IsPersonEditing = (selectedRelativeViewModel == null);
            }
        }

        private bool isPersonEditing;
        public bool IsPersonEditing
        {
            get { return isPersonEditing; }
            set { Set("IsPersonEditing", ref isPersonEditing, value); }
        }


        private async void SetRelatives()
        {
            var task = Task.Factory.StartNew(SetRelativesAsync);
            await task;
        }

        private void SetRelativesAsync()
        {
            var listRelatives = service.GetPersonRelativesDTO(Id);
            //listRelatives.Add(new PersonRelativeDTO()
            //    {
            //        PersonId = Id,
            //        RelativePersonId = -1,
            //        ShortName = "Новый родственник",
            //        RelativeRelationId = 0,
            //        IsRepresentative = false,
            //        PhotoUri = string.Empty
            //    });

            var listViewModels = new List<EditPersonDataViewModel>();
            foreach (var item in listRelatives)
                listViewModels.Add(new EditPersonDataViewModel(log, service, dialogService, documentService) { Id = item.RelativePersonId, RelativeToPersonId = this.Id });
            EditPersonRelativeDataViewModels = new ObservableCollection<EditPersonDataViewModel>(listViewModels);
        }

        public string PersonFullName
        {
            get { return EditPersonDataViewModel.PersonFullName; }
        }

        public ObservableCollection<RelativeRelationship> RelativeRelations { get; set; }
    }
}
