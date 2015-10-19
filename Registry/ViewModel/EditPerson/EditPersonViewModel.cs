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
using MainLib.ViewModel;
using MainLib.View;

namespace Registry
{
    public class EditPersonViewModel : ObservableObject
    {
        private readonly ILog log;

        private readonly IPersonService service;

        private readonly IRecordService recordService;

        private readonly IVisitService visitService;

        private readonly IAssignmentService assignmentService;

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
        public EditPersonViewModel(ILog log, IPersonService service, IDialogService dialogService, IDocumentService documentService, IRecordService recordService, IVisitService visitService, IAssignmentService assignmentService)
        {
            if (log == null)
                throw new ArgumentNullException("log");
            if (service == null)
                throw new ArgumentNullException("service");
            if (dialogService == null)
                throw new ArgumentNullException("dialogService");
            if (documentService == null)
                throw new ArgumentNullException("documentService");
            if (recordService == null)
                throw new ArgumentNullException("recordService");
            if (visitService == null)
                throw new ArgumentNullException("visitService");
            if (assignmentService == null)
                throw new ArgumentNullException("assignmentService");
            this.documentService = documentService;
            this.dialogService = dialogService;
            this.service = service;
            this.recordService = recordService;
            this.assignmentService = assignmentService;
            this.visitService = visitService;
            this.log = log;
            IsPersonEditing = true;
            ReturnToPersonEditingCommand = new RelayCommand(ReturnToPersonEditing);
            EditPersonRelativeDataViewModels = new ObservableCollection<EditPersonDataViewModel>();
            EditPersonDataViewModel = new EditPersonDataViewModel(log, service, dialogService, documentService);
            RelativeRelations = new ObservableCollection<RelativeRelationship>(service.GetRelativeRelationships());
            SaveChangesCommand = new RelayCommand(SaveChanges);
            AddNewPersonToRelativeCommand = new RelayCommand(AddNewPersonToRelative);
            RemovePersonFromRelativeCommand = new RelayCommand(RemovePersonFromRelative);
        }

        public EditPersonViewModel(ILog log, IPersonService service, IDialogService dialogService, IDocumentService documentService, IRecordService recordService, IVisitService visitService, IAssignmentService assignmentService, int personId)
            : this(log, service, dialogService, documentService, recordService, visitService, assignmentService)
        {
            Id = personId;
            this.log = log;
        }

        /// <summary>
        /// TODO: Use this for creating new person with default data from search
        /// </summary>
        public EditPersonViewModel(ILog log, IPersonService service, IDialogService dialogService, IDocumentService documentService, IRecordService recordService, IVisitService visitService, IAssignmentService assignmentService, string personData)
            : this(log, service, dialogService, documentService, recordService, visitService, assignmentService)
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
                LoadPersonDocuments();
                LoadContracts();
                LoadVisits();
            }
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
                MaritalStatusId = EditPersonDataViewModel.MaritalStatusId,
                EducationId = EditPersonDataViewModel.EducationId,
                RelativeToPersonId = -1
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
                    MaritalStatusId = personRelativeDataViewModels.MaritalStatusId,
                    EducationId = personRelativeDataViewModels.EducationId,
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
                RaisePropertyChanged(() => IsNullSelectedRelativeViewModel);
            }
        }

        public bool IsNullSelectedRelativeViewModel
        {
            get { return SelectedRelativeViewModel == null; }
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

        private PersonDocumentsViewModel personDocuments;
        public PersonDocumentsViewModel PersonDocuments
        {
            get { return personDocuments; }
            set { Set("PersonDocuments", ref personDocuments, value); }
        }

        private PersonContractsViewModel personContracts;
        public PersonContractsViewModel PersonContracts
        {
            get { return personContracts; }
            set { Set("PersonContracts", ref personContracts, value); }
        }

        private MainLib.PersonVisitItemsListViewModels.PersonVisitItemsListViewModel personVisits;
        public MainLib.PersonVisitItemsListViewModels.PersonVisitItemsListViewModel PersonVisits
        {
            get { return personVisits; }
            set { Set(() => PersonVisits, ref personVisits, value); }
        }

        private int selectedPageIndex;
        public int SelectedPageIndex
        {
            get { return selectedPageIndex; }
            set { Set("SelectedPageIndex", ref selectedPageIndex, value); }
        }

        private void LoadPersonDocuments()
        {
            PersonDocuments = new PersonDocumentsViewModel(service, documentService, dialogService, log);
            if (Id != 0)
                PersonDocuments.Load(Id);
        }

        private void LoadContracts()
        {
            PersonContracts = new PersonContractsViewModel(service, recordService, assignmentService, dialogService, log);
            if (Id != 0)
                PersonContracts.Load(Id);
        }

        private void LoadVisits()
        {
            PersonVisits = new MainLib.PersonVisitItemsListViewModels.PersonVisitItemsListViewModel(Id, service, visitService, assignmentService, recordService);
        }

        public string PersonFullName
        {
            get { return EditPersonDataViewModel.PersonFullName; }
        }

        public ObservableCollection<RelativeRelationship> RelativeRelations { get; set; }


        #region Commands

        public ICommand AddNewPersonToRelativeCommand { get; set; }
        private void AddNewPersonToRelative()
        {
            var newRelative = new EditPersonDataViewModel(log, service, dialogService, documentService) { RelativeToPersonId = Id };
            EditPersonRelativeDataViewModels.Add(newRelative);
            SelectedRelativeViewModel = newRelative;
        }

        public ICommand RemovePersonFromRelativeCommand { get; set; }
        private void RemovePersonFromRelative()
        {
            if (SelectedRelativeViewModel == null)
                return;
            var selectedIndex = EditPersonRelativeDataViewModels.IndexOf(SelectedRelativeViewModel);
            EditPersonRelativeDataViewModels.Remove(SelectedRelativeViewModel);
            if (EditPersonRelativeDataViewModels.Count > 0)
                if (selectedIndex < EditPersonRelativeDataViewModels.Count)
                    SelectedRelativeViewModel = EditPersonRelativeDataViewModels[selectedIndex];
                else
                    SelectedRelativeViewModel = EditPersonRelativeDataViewModels[selectedIndex - 1];
            else
                SelectedRelativeViewModel = null;
        }

        public ICommand ReturnToPersonEditingCommand { get; set; }
        private void ReturnToPersonEditing()
        {
            SelectedRelativeViewModel = null;
        }

        public ICommand SaveChangesCommand { get; set; }
        private async void SaveChanges()
        {
            IsSaveEnabled = false;
            var notEroors = true;
            notEroors &= EditPersonDataViewModel.Invalidate();
            foreach (var personRelativeDataViewModels in EditPersonRelativeDataViewModels)
            {
                notEroors &= personRelativeDataViewModels.Invalidate();
            }
            if (!notEroors)
            {
                IsSaveEnabled = true;
                return;
            }
            var task = Task.Factory.StartNew(SaveData);

            await task;
            IsSaveEnabled = true;
        }

        #endregion
    }

}
