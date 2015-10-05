using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Core;
using GalaSoft.MvvmLight.CommandWpf;
using log4net;
using MainLib;

namespace Registry
{
    public class PatientSearchViewModel : BasicViewModel
    {
        #region Fields
        private const int UserInputSearchThreshold = 3;

        private const int PatientDisplayCount = 5;

        private readonly ILog log;

        private readonly IPatientService patientService;

        private readonly IPersonService personService;

        private readonly IDialogService dialogService;

        private readonly IDocumentService documentService;
        #endregion

        #region Constructors
        public PatientSearchViewModel(IPatientService patientService, IPersonService personService, ILog log, IDialogService dialogService, IDocumentService documentService, PatientAssignmentListViewModel patientAssignmentListViewModel)
        {
            if (patientService == null)
                throw new ArgumentNullException("patientService");
            if (log == null)
                throw new ArgumentNullException("log");
            if (patientAssignmentListViewModel == null)
                throw new ArgumentNullException("patientAssignmentListViewModel");
            if (personService == null)
                throw new ArgumentNullException("personService");
            if (dialogService == null)
                throw new ArgumentNullException("dialogService");
            if (documentService == null)
                throw new ArgumentNullException("documentService");
            this.documentService = documentService;
            this.dialogService = dialogService;
            this.personService = personService;
            this.log = log;
            this.patientService = patientService;
            patients = new ObservableCollection<PersonViewModel>();
            PatientAssignmentListViewModel = patientAssignmentListViewModel;
            editPersonViewModel = new EditPersonViewModel(log, personService, dialogService, documentService);
            currentPatient = new PersonViewModel(null);
            NewPatientCommand = new RelayCommand(NewPatient);
            EditPatientCommand = new RelayCommand(EditPatient);
            ContractsCommand = new RelayCommand(PatientContracts);
            PersonDocumentsCommand = new RelayCommand(PersonDocuments);
        }
        #endregion

        #region Properties
        public PatientAssignmentListViewModel PatientAssignmentListViewModel { get; private set; }

        private ObservableCollection<PersonViewModel> patients;

        public ObservableCollection<PersonViewModel> Patients
        {
            get { return patients; }
            private set { Set("Patients", ref patients, value); }
        }

        private string searchString;

        public string SearchString
        {
            get { return searchString; }
            set
            {
                value = value.Trim();
                if (!Set("SearchString", ref searchString, value))
                    return;
                SearchPatients(value);
            }
        }

        private PersonViewModel selectedPatient;
        //The difference between this property and CurrentPatient is that one is bound to ListBox and may become null when user searches patients.
        //On the other hand CurrentPatient is empty initially (when no patient is selected) and after user selects or creates a patient, it will never become empty again
        //meaning that user can't deselect patient
        public PersonViewModel SelectedPatient
        {
            get { return selectedPatient; }
            set
            {
                Set("SelectedPatient", ref selectedPatient, value);
                if (value == null)
                    return;
                SelectPatient(value);
            }
        }

        private PersonViewModel currentPatient;

        public PersonViewModel CurrentPatient
        {
            get { return currentPatient; }
            set
            {
                var isPatientSelected = IsPatientSelected;
                if (Set("CurrentPatient", ref currentPatient, value) && IsPatientSelected != isPatientSelected)
                    RaisePropertyChanged("IsPatientSelected");
            }
        }

        public bool IsPatientSelected { get { return currentPatient != null; } }

        private bool isLookingForPatient;

        public bool IsLookingForPatient
        {
            get { return isLookingForPatient; }
            set { Set("IsLookingForPatient", ref isLookingForPatient, value); }
        }

        private bool noOneisFound;

        public bool NoOneisFound
        {
            get { return noOneisFound; }
            set { Set("NoOneisFound", ref noOneisFound, value); }
        }

        private readonly EditPersonViewModel editPersonViewModel;
        #endregion

        #region Methods
        private void SearchPatients(string searchString)
        {
            IsLookingForPatient = false;
            FailReason = string.Empty;
            NoOneisFound = false;
            Patients.Clear();
            if (searchString == null || searchString.Length < UserInputSearchThreshold)
                return;
            IsLookingForPatient = true;
            var uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            Task.Delay(500)
                .ContinueWith((_, x) => SearchPatientsImpl(x as string), searchString)
                .ContinueWith(PatientsSearched, uiScheduler);
        }

        private IEnumerable<PersonViewModel> SearchPatientsImpl(string searchString)
        {
            if (searchString != SearchString)
                return null;
            return patientService.GetPatients(searchString, PatientDisplayCount).Select(x => new PersonViewModel(x)).ToArray();
        }
        //TODO: use fail reason on view side
        private void PatientsSearched(Task<IEnumerable<PersonViewModel>> sourceTask)
        {
            var anotherSearchWasExecuted = false;
            try
            {
                var result = sourceTask.Result;
                anotherSearchWasExecuted = result == null || sourceTask.AsyncState as string != SearchString;
                if (anotherSearchWasExecuted)
                    return;
                Patients = new ObservableCollection<PersonViewModel>(result);
            }
            catch (AggregateException ex)
            {
                var innerException = ex.InnerExceptions[0];
                //TODO: probably move this string to separate localizable dll
                FailReason = "В процессе поиск пациента возникла ошибка. Возможно отсутствует связь с базой данной. Повторите поиск еще раз. Если ошибка повторится, обратитесь в службу поддержки";
                log.Error(string.Format("Failed to find patients for user input of '{0}'", sourceTask.AsyncState), innerException);
            }
            finally
            {
                if (!anotherSearchWasExecuted)
                {
                    IsLookingForPatient = false;
                    NoOneisFound = Patients.Count == 0;
                }
            }
        }

        private void SelectPatient(PersonViewModel person)
        {
            CurrentPatient = person;
            PatientAssignmentListViewModel.PatientId = person.Id;
        }
        #endregion

        #region Commands
        public ICommand NewPatientCommand { get; private set; }

        private void NewPatient()
        {
            editPersonViewModel.Id = 0;
            var editPersonDataView = new EditPersonView() { DataContext = editPersonViewModel };
            editPersonDataView.ShowDialog();
        }

        public ICommand EditPatientCommand { get; private set; }

        private void EditPatient()
        {
            if (currentPatient.IsEmpty)
                return;
            editPersonViewModel.Id = currentPatient.Id;
            editPersonViewModel.SelectedPageIndex = 0;
            var editPersonDataView = new EditPersonView() { DataContext = editPersonViewModel };
            editPersonDataView.ShowDialog();
        }

        public ICommand PersonDocumentsCommand { get; private set; }
        private void PersonDocuments()
        {
            if (currentPatient.IsEmpty)
                return;
            editPersonViewModel.Id = currentPatient.Id;
            editPersonViewModel.SelectedPageIndex = 1;
            var editPersonDataView = new EditPersonView() { DataContext = editPersonViewModel };
            editPersonDataView.ShowDialog();
        }

        public ICommand ContractsCommand { get; private set; }
        private void PatientContracts()
        {
            if (currentPatient.IsEmpty)
                return;
            editPersonViewModel.Id = currentPatient.Id;
            editPersonViewModel.SelectedPageIndex = 2;
            var editPersonDataView = new EditPersonView() { DataContext = editPersonViewModel };
            editPersonDataView.ShowDialog();
        }        

        public ICommand CreateAmbCardCommand { get; private set; }
        private void CreateAmbCard()
        {
            if (currentPatient.IsEmpty)
                return;
            CurrentPatient.AmbNumberString = personService.CreateAmbCard(currentPatient.Id);
        }
        #endregion
    }
}
