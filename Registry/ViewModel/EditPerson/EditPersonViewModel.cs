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

        private EditPersonDataViewModel editPersonDataViewModel;
        public EditPersonDataViewModel EditPersonDataViewModel
        {
            get { return editPersonDataViewModel; }
            set
            {
                Set("EditPersonDataViewModel", ref editPersonDataViewModel, value);
            }
        }

        private EditPersonDataViewModel editPersonRelativeDataViewModel;
        public EditPersonDataViewModel EditPersonRelativeDataViewModel
        {
            get { return editPersonRelativeDataViewModel; }
            set
            {
                Set("EditPersonRelativeDataViewModel", ref editPersonRelativeDataViewModel, value);
            }
        }

        /// <summary>
        /// Use this for creating new person
        /// </summary>
        public EditPersonViewModel(ILog log, IPersonService service, IDialogService dialogService)
        {
            if (log == null)
                throw new ArgumentNullException("log");
            if (service == null)
                throw new ArgumentNullException("service");
            if (dialogService == null)
                throw new ArgumentNullException("dialogService");
            this.dialogService = dialogService;
            this.service = service;
            this.log = log;
            IsPersonEditing = true;
            ReturnToPersonEditingCommand = new RelayCommand(ReturnToPersonEditing);
            EditPersonRelativeDataViewModel = new EditPersonDataViewModel(log, service, dialogService);
            EditPersonDataViewModel = new EditPersonDataViewModel(log, service, dialogService);
            RelativeRelations = new ObservableCollection<RelativeRelationship>(service.GetRelativeRelationships());
            SaveChangesCommand = new RelayCommand(SaveChanges);
        }

        public EditPersonViewModel(ILog log, IPersonService service, IDialogService dialogService, int personId)
            : this(log, service, dialogService)
        {
            Id = personId;
            this.log = log;
        }

        /// <summary>
        /// TODO: Use this for creating new person with default data from search
        /// </summary>
        public EditPersonViewModel(ILog log, IPersonService service, IDialogService dialogService, string personData)
            : this(log, service, dialogService)
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
            SelectedRelative = null;
        }

        public ICommand SaveChangesCommand { get; set; }
        private void SaveChanges()
        {
            var personNames = new List<PersonName>();
            var person = editPersonDataViewModel.GetUnsavedPerson(out personNames);
            var personInsuranceDocuments = editPersonDataViewModel.GetUnsavedPersonInsuranceDocuments();
            var personAddresses = editPersonDataViewModel.GetUnsavedPersonAddresses();
            var personIdentityDocuments = editPersonDataViewModel.GetUnsavedPersonIdentityDocuments();
            var personDisabilities = editPersonDataViewModel.GetUnsavedPersonDisabilities();
            var personSocialStatuses = editPersonDataViewModel.GetUnsavedPersonSocialStatuses();

            service.SavePersonData(person, personNames, personInsuranceDocuments, personAddresses, personIdentityDocuments, personDisabilities, personSocialStatuses);
        }

        private PersonRelativeDTO selectedRelative;
        public PersonRelativeDTO SelectedRelative
        {
            get { return selectedRelative; }
            set
            {
                Set("SelectedRelative", ref selectedRelative, value);
                IsPersonEditing = (selectedRelative == null);
                if (selectedRelative != null)
                    EditPersonRelativeDataViewModel.Id = selectedRelative.RelativePersonId;
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
            listRelatives.Add(new PersonRelativeDTO()
                {
                    RelativePersonId = -1,
                    ShortName = "Новый родственник",
                    RelativeRelationId = 0,
                    IsRepresentative = false,
                    PhotoUri = string.Empty
                });
            //listRelatives.Add(new PersonRelative());
            Relatives = new ObservableCollection<PersonRelativeDTO>(listRelatives);
        }

        private ObservableCollection<PersonRelativeDTO> relatives;
        public ObservableCollection<PersonRelativeDTO> Relatives
        {
            get { return relatives; }
            set { Set("Relatives", ref relatives, value); }
        }

        public string PersonFullName
        {
            get { return EditPersonDataViewModel.PersonFullName; }
        }

        public ObservableCollection<RelativeRelationship> RelativeRelations { get; set; }
    }
}
