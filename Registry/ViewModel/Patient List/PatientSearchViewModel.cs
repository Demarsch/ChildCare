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

        private readonly IRecordService recordService;

        private readonly IDocumentService documentService;
        private readonly IAssignmentService assignmentService;
        #endregion

        #region Constructors
        public PatientSearchViewModel(IPatientService patientService, IPersonService personService, ILog log, IDialogService dialogService, IDocumentService documentService, IRecordService recordService, IAssignmentService assignmentService, PatientAssignmentListViewModel patientAssignmentListViewModel)
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
            this.recordService = recordService;
            this.assignmentService = assignmentService;
            patients = new ObservableCollection<PersonViewModel>();
            PatientAssignmentListViewModel = patientAssignmentListViewModel;
            editPersonViewModel = new EditPersonViewModel(log, personService, dialogService, documentService, recordService, assignmentService);
            currentPatient = new PersonViewModel(null);
            NewPatientCommand = new RelayCommand(NewPatient);
            EditPatientCommand = new RelayCommand(EditPatient);
            ShowContractsCommand = new RelayCommand(ShowContracts);
            ShowPersonDocumentsCommand = new RelayCommand(ShowPersonDocuments);
            CreateAmbCardCommand = new RelayCommand(CreateAmbCard);
            PrintAmbCardFirstListCommand = new RelayCommand(PrintAmbCardFirstList);
            PrintPersonHospListCommand = new RelayCommand(PrintPersonHospList);
            PrintRadiationListCommand = new RelayCommand(PrintRadiationList);
            PrintAllAmbCardCommand = new RelayCommand(PrintAllAmbCard);
            ShowVisitsCommand = new RelayCommand(ShowVisits);
            ShowCasesCommand = new RelayCommand(ShowCases);
            ShowPrintDocumentsCommand = new RelayCommand(ShowPrintDocuments);

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

        public string AmbNumberButtonText
        {
            get
            {
                if (CurrentPatient.IsEmpty) return string.Empty;
                return CurrentPatient.AmbNumberExist ? "а/к " + CurrentPatient.AmbNumberString : "Новая а/к";
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
                if (Set("CurrentPatient", ref currentPatient, value))
                {
                    SetPrintedProperties();
                    RaisePropertyChanged(() => AmbNumberButtonText);
                    if (IsPatientSelected != isPatientSelected)
                        RaisePropertyChanged("IsPatientSelected");
                }
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

        private bool? isPrintedAmbCardFirstList = null;
        public bool? IsPrintedAmbCardFirstList
        {
            get { return isPrintedAmbCardFirstList; }
            set
            {
                if (Set(() => IsPrintedAmbCardFirstList, ref isPrintedAmbCardFirstList, value))
                    RaisePropertyChanged(() => IsPrintedAllAmbCard);
            }
        }

        private bool? isPrintedPersonHospList = null;
        public bool? IsPrintedPersonHospList
        {
            get { return isPrintedPersonHospList; }
            set
            {
                if (Set(() => IsPrintedPersonHospList, ref isPrintedPersonHospList, value))
                    RaisePropertyChanged(() => IsPrintedAllAmbCard);
            }
        }

        private bool? isPrintedRadiationList = null;
        public bool? IsPrintedRadiationList
        {
            get { return isPrintedRadiationList; }
            set
            {
                if (Set(() => IsPrintedRadiationList, ref isPrintedRadiationList, value))
                    RaisePropertyChanged(() => IsPrintedAllAmbCard);
            }
        }

        public bool? IsPrintedAllAmbCard
        {
            get
            {
                if (IsPrintedAmbCardFirstList == null || IsPrintedPersonHospList == null || IsPrintedRadiationList == null)
                    return null;
                return (bool)IsPrintedAmbCardFirstList && (bool)IsPrintedPersonHospList && (bool)IsPrintedRadiationList;
            }
        }

        private readonly EditPersonViewModel editPersonViewModel;
        #endregion

        #region Methods

        private void SetPrintedProperties()
        {
            IsPrintedAmbCardFirstList = (CurrentPatient.AmbCardFirstListHashCode == personService.GetAmbCardFirstList(CurrentPatient.Id).GetHashCode());
            IsPrintedPersonHospList = (CurrentPatient.PersonHospListHashCode == personService.GetPersonHospList(CurrentPatient.Id).GetHashCode());
            IsPrintedRadiationList = (CurrentPatient.RadiationListHashCode == personService.GetRadiationList(CurrentPatient.Id).GetHashCode());
        }

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

        public ICommand ShowPersonDocumentsCommand { get; private set; }
        private void ShowPersonDocuments()
        {
            if (currentPatient.IsEmpty)
                return;
            editPersonViewModel.Id = currentPatient.Id;
            editPersonViewModel.SelectedPageIndex = 1;
            var editPersonDataView = new EditPersonView() { DataContext = editPersonViewModel };
            editPersonDataView.ShowDialog();
        }

        public ICommand ShowContractsCommand { get; private set; }
        private void ShowContracts()
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
            CurrentPatient.AmbNumberString = personService.GetOrCreateAmbCard(currentPatient.Id);
            RaisePropertyChanged(() => AmbNumberButtonText);
        }

        public ICommand PrintAmbCardFirstListCommand { get; private set; }
        private void PrintAmbCardFirstList()
        {
            throw new NotImplementedException();
        }

        public ICommand PrintPersonHospListCommand { get; private set; }
        private void PrintPersonHospList()
        {
            throw new NotImplementedException();
        }

        public ICommand PrintRadiationListCommand { get; private set; }
        private void PrintRadiationList()
        {
            throw new NotImplementedException();
        }

        public ICommand PrintAllAmbCardCommand { get; private set; }
        private void PrintAllAmbCard()
        {
            throw new NotImplementedException();
        }

        public ICommand ShowVisitsCommand { get; private set; }
        private void ShowVisits()
        {
            throw new NotImplementedException();
        }

        public ICommand ShowCasesCommand { get; private set; }
        private void ShowCases()
        {
            throw new NotImplementedException();
        }

        public ICommand ShowPrintDocumentsCommand { get; private set; }
        private void ShowPrintDocuments()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
