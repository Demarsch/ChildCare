using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Core.Data;
using Core.Data.Misc;
using Core.Extensions;
using Core.Misc;
using Core.Services;
using Core.Wpf.Events;
using Core.Wpf.Misc;
using Core.Wpf.Mvvm;
using Core.Wpf.Services;
using Core.Wpf.ViewModels;
using log4net;
using PatientInfoModule.Data;
using PatientInfoModule.Misc;
using PatientInfoModule.Services;
using Prism.Commands;
using Prism.Events;
using Shared.Patient.Services;
using Shared.Patient.ViewModels;
using Shared.Patient.Misc;

namespace PatientInfoModule.ViewModels
{
    public class PatientInfoViewModel : TrackableBindableBase, IActiveDataErrorInfo, IChangeTrackerMediator, IDisposable
    {
        private readonly IPatientService patientService;

        private readonly ILog log;

        private readonly ICacheService cacheService;

        private readonly IDialogServiceAsync dialogService;

        private readonly IDocumentService documentService;

        private readonly IFileService fileService;

        private readonly IEventAggregator eventAggregator;

        private readonly IAddressSuggestionProvider addressSuggestionProvider;

        private readonly ValidationMediator validator;

        public PatientInfoViewModel(IPatientService patientService,
                                    ILog log,
                                    ICacheService cacheService,
                                    IDialogServiceAsync dialogService,
                                    IDocumentService documentService,
                                    IFileService fileService,
                                    IEventAggregator eventAggregator,
                                    IAddressSuggestionProvider addressSuggestionProvider,
                                    IdentityDocumentCollectionViewModel identityDocumentCollectionViewModel,
                                    InsuranceDocumentCollectionViewModel insuranceDocumentCollectionViewModel,
                                    AddressCollectionViewModel addressCollectionViewModel,
                                    DisabilityDocumentCollectionViewModel disabilityDocumentCollectionViewModel,
                                    SocialStatusCollectionViewModel socialStatusCollectionViewModel)
        {
            if (patientService == null)
            {
                throw new ArgumentNullException("patientService");
            }
            if (log == null)
            {
                throw new ArgumentNullException("log");
            }
            if (documentService == null)
            {
                throw new ArgumentNullException("documentService");
            }
            if (fileService == null)
            {
                throw new ArgumentNullException("fileService");
            }
            if (dialogService == null)
            {
                throw new ArgumentNullException("dialogService");
            }
            if (identityDocumentCollectionViewModel == null)
            {
                throw new ArgumentNullException("identityDocumentCollectionViewModel");
            }
            if (insuranceDocumentCollectionViewModel == null)
            {
                throw new ArgumentNullException("insuranceDocumentCollectionViewModel");
            }
            if (addressCollectionViewModel == null)
            {
                throw new ArgumentNullException("addressCollectionViewModel");
            }
            if (disabilityDocumentCollectionViewModel == null)
            {
                throw new ArgumentNullException("disabilityDocumentCollectionViewModel");
            }
            if (socialStatusCollectionViewModel == null)
            {
                throw new ArgumentNullException("socialStatusCollectionViewModel");
            }
            if (cacheService == null)
            {
                throw new ArgumentNullException("cacheService");
            }
            if (addressSuggestionProvider == null)
            {
                throw new ArgumentNullException("addressSuggestionProvider");
            }
            this.cacheService = cacheService;
            this.dialogService = dialogService;
            this.documentService = documentService;
            this.fileService = fileService;
            this.eventAggregator = eventAggregator;
            this.addressSuggestionProvider = addressSuggestionProvider;
            IdentityDocuments = identityDocumentCollectionViewModel;
            InsuranceDocuments = insuranceDocumentCollectionViewModel;
            Addresses = addressCollectionViewModel;
            DisabilityDocuments = disabilityDocumentCollectionViewModel;
            SocialStatuses = socialStatusCollectionViewModel;
            this.patientService = patientService;
            this.log = log;
            TakePhotoCommand = new DelegateCommand(TakePhotoAsync);
            validator = new ValidationMediator(this);
            patientIdBeingLoaded = SpecialValues.NonExistingId;
            currentInstanceChangeTracker = new ChangeTrackerEx<PatientInfoViewModel>(this);
            var changeTracker = new CompositeChangeTracker(currentInstanceChangeTracker,
                                                           IdentityDocuments.ChangeTracker,
                                                           InsuranceDocuments.ChangeTracker,
                                                           Addresses.ChangeTracker,
                                                           DisabilityDocuments.ChangeTracker,
                                                           SocialStatuses.ChangeTracker);
            currentInstanceChangeTracker.RegisterComparer(() => LastName, StringComparer.CurrentCultureIgnoreCase);
            currentInstanceChangeTracker.RegisterComparer(() => FirstName, StringComparer.CurrentCultureIgnoreCase);
            currentInstanceChangeTracker.RegisterComparer(() => MiddleName, StringComparer.CurrentCultureIgnoreCase);
            changeTracker.PropertyChanged += OnChangesTracked;
            ChangeTracker = changeTracker;
            BusyMediator = new BusyMediator();
            FailureMediator = new FailureMediator();
            NotificationMediator = new NotificationMediator();
            //We need to have copy of relationship list for every relative
            Relationships = cacheService.GetItems<RelativeRelationship>().ToArray();
            CollectionViewSource.GetDefaultView(Relationships).Filter = FilterRelationshipByGender;
            ActivateChildContentCommand = new DelegateCommand<object>(ActivateChildContent);
            reloadPatientDataCommandWrapper = new CommandWrapper { Command = new DelegateCommand(async () => await LoadPatientInfoAsync(patientIdBeingLoaded)) };
            reloadDataSourceCommandWrapper = new CommandWrapper { Command = new DelegateCommand(async () => await EnsureDataSourceLoaded()) };
            requestLoadSimilarPatientNotification = new ActionRequiringNotificationViewModel();
            requestLoadSimilarPatientNotification.Actions.Add(new CommandWrapper { Command = new DelegateCommand(LoadSimilarPatientAsync), CommandName = "Загрузить данные" });
            requestNavigationToRelativeNotification = new ActionRequiringNotificationViewModel { Message = "У текущего пациента уже есть этот родственник. Открыть его данные?" };
            requestNavigationToRelativeNotification.Actions.Add(new CommandWrapper { Command = new DelegateCommand(RequestNavigationToRelative), CommandName = "Открыть" }); 
            currentOperation = new TaskCompletionSource<object>();
            currentOperation.SetResult(null);
        }

        private bool FilterRelationshipByGender(object o)
        {
            var relationship = (RelativeRelationship)o;
            return relationship.MustBeMale == null || relationship.MustBeMale == IsMale;
        }

        private void OnChangesTracked(object sender, PropertyChangedEventArgs e)
        {
            UpdateNameIsChanged();
        }

        public IdentityDocumentCollectionViewModel IdentityDocuments { get; private set; }

        public InsuranceDocumentCollectionViewModel InsuranceDocuments { get; private set; }

        public AddressCollectionViewModel Addresses { get; private set; }

        public DisabilityDocumentCollectionViewModel DisabilityDocuments { get; private set; }

        public SocialStatusCollectionViewModel SocialStatuses { get; private set; }

        #region Data source

        private TaskCompletionSource<bool> dataSourcesLoadingTaskSource;

        private async Task<bool> EnsureDataSourceLoaded()
        {
            if (dataSourcesLoadingTaskSource != null)
            {
                return await dataSourcesLoadingTaskSource.Task;
            }
            dataSourcesLoadingTaskSource = new TaskCompletionSource<bool>();
            log.InfoFormat("Loading data sources for patient info content...");
            BusyMediator.Activate("Загрузка общих данных...");
            try
            {
                //Ensure data is loaded from OKATO table
                var dataSourceLoadingTask = Task<DataSource>.Factory.StartNew(LoadDataSource);
                await Task.WhenAll(dataSourceLoadingTask,
                                   Task.Factory.StartNew(FillCache),
                                   addressSuggestionProvider.EnsureDataSourceLoadedAsync());
                var result = dataSourceLoadingTask.Result;
                Educations = result.Educations;
                HealthGroups = result.HealthGroups;
                Nationalities = result.Countries;
                MaritalStatuses = result.MaritalStatuses;
                log.InfoFormat("Data sources for patient info content are successfully loaded");
                dataSourcesLoadingTaskSource.SetResult(true);
                return true;
            }
            catch (Exception ex)
            {
                log.Error("Failed to load data sources for patient info content", ex);
                FailureMediator.Activate("Не удалось загрузить общие данные. Попробуйте еще раз или обратитесь в службу поддержки", reloadDataSourceCommandWrapper, ex);
                dataSourcesLoadingTaskSource.SetResult(false);
                dataSourcesLoadingTaskSource = null;
                return false;
            }
            finally
            {
                BusyMediator.Deactivate();
            }
        }

        private void FillCache()
        {
            //This is just to speed up loading process. Its ok if some data is not loaded at the below point
            cacheService.GetItems<Okato>();
            cacheService.GetItems<IdentityDocumentType>();
            cacheService.GetItems<AddressType>();
            cacheService.GetItems<DisabilityType>();
            cacheService.GetItems<SocialStatusType>();
        }

        private DataSource LoadDataSource()
        {
            return new DataSource
                   {
                       Countries = patientService.GetCountries(),
                       Educations = patientService.GetEducations(),
                       HealthGroups = patientService.GetHealthGroups(),
                       MaritalStatuses = patientService.GetMaritalStatuses(),
                       Relationships = patientService.GetRelationships()
                   };
        }

        private readonly CommandWrapper reloadDataSourceCommandWrapper;

        private IEnumerable<HealthGroup> healthGroups;

        public IEnumerable<HealthGroup> HealthGroups
        {
            get { return healthGroups; }
            set { SetProperty(ref healthGroups, value); }
        }

        private IEnumerable<Education> educations;

        public IEnumerable<Education> Educations
        {
            get { return educations; }
            set { SetProperty(ref educations, value); }
        }

        private IEnumerable<Country> nationalities;

        public IEnumerable<Country> Nationalities
        {
            get { return nationalities; }
            set { SetProperty(ref nationalities, value); }
        }

        private IEnumerable<MaritalStatus> maritalStatuses;

        public IEnumerable<MaritalStatus> MaritalStatuses
        {
            get { return maritalStatuses; }
            set { SetProperty(ref maritalStatuses, value); }
        }

        public IEnumerable<RelativeRelationship> Relationships { get; private set; }

        #endregion

        public BusyMediator BusyMediator { get; set; }

        public FailureMediator FailureMediator { get; private set; }

        public NotificationMediator NotificationMediator { get; private set; }

        private PersonName currentName;

        private Person currentPerson;

        private PersonEducation currentEducation;

        private PersonNationality currentNationality;

        private PersonMaritalStatus currentMaritalStatus;

        private PersonHealthGroup currentHealthGroup;

        private ICollection<PersonIdentityDocument> currentIdentityDocuments;

        private ICollection<InsuranceDocument> currentInsuranceDocuments;

        private ICollection<PersonAddress> currentAddresses;

        private ICollection<PersonDisability> currentDisabilityDocuments;

        private ICollection<PersonSocialStatus> currentSocialStatuses;

        private readonly int patientIdBeingLoaded;

        #region Relative related

        private PersonRelative personRelative;

        public PersonRelative PersonRelative
        {
            get
            {
                if (!IsRelative)
                {
                    return null;
                }
                return new PersonRelative
                       {
                           Id = personRelative.Id,
                           IsRepresentative = IsRepresentative,
                           PersonId = personRelative.PersonId,
                           RelativeId = currentPerson.Id,
                           RelativeRelationshipId = SelectedRelationship.Id
                       };
            }
            set
            {
                ChangeTracker.IsEnabled = false;
                personRelative = value;
                if (value == null)
                {
                    IsRelative = false;
                    SelectedRelationship = null;
                    IsRepresentative = false;
                }
                else
                {
                    IsRelative = true;
                    SelectedRelationship = Relationships.FirstOrDefault(x => x.Id == value.RelativeRelationshipId);
                    IsRepresentative = value.IsRepresentative;
                }
                ChangeTracker.IsEnabled = true;
            }
        }

        private bool isRelative;

        public bool IsRelative
        {
            get { return isRelative; }
            set { SetProperty(ref isRelative, value); }
        }

        private RelativeRelationship selectedRelationship;

        public RelativeRelationship SelectedRelationship
        {
            get { return selectedRelationship; }
            set { SetTrackedProperty(ref selectedRelationship, value); }
        }

        private bool isRepresentative;

        public bool IsRepresentative
        {
            get { return isRepresentative; }
            set { SetTrackedProperty(ref isRepresentative, value); }
        }

        #endregion

        #region Properties

        private string lastName;

        public string LastName
        {
            get { return lastName; }
            set
            {
                value = value.Trim();
                if (SetTrackedProperty(ref lastName, value))
                {
                    UpdateNameIsChanged();
                    TryPredictGenderByLastName();
                    CheckForDuplicatePersonAsync(CreateDuplicateCheckParameters());
                }
            }
        }

        private void TryPredictGenderByLastName()
        {
            var lastName = this.lastName ?? string.Empty;
            if (lastName.EndsWith("ов", StringComparison.CurrentCultureIgnoreCase)
                || lastName.EndsWith("ев", StringComparison.CurrentCultureIgnoreCase)
                || lastName.EndsWith("ин", StringComparison.CurrentCultureIgnoreCase))
            {
                IsMale = true;
            }
            else if (lastName.EndsWith("ова", StringComparison.CurrentCultureIgnoreCase)
                     || lastName.EndsWith("ева", StringComparison.CurrentCultureIgnoreCase)
                     || lastName.EndsWith("ина", StringComparison.CurrentCultureIgnoreCase))
            {
                IsMale = false;
            }
        }

        private string firstName;

        public string FirstName
        {
            get { return firstName; }
            set
            {
                value = value.Trim();
                if (SetTrackedProperty(ref firstName, value))
                {
                    UpdateNameIsChanged();
                    CheckForDuplicatePersonAsync(CreateDuplicateCheckParameters());
                }
            }
        }

        private string middleName;

        public string MiddleName
        {
            get { return middleName; }
            set
            {
                value = value.Trim();
                if (SetTrackedProperty(ref middleName, value))
                {
                    UpdateNameIsChanged();
                    TryPredictGenderByMiddleName();
                    CheckForDuplicatePersonAsync(CreateDuplicateCheckParameters());
                }
            }
        }

        private void TryPredictGenderByMiddleName()
        {
            var middleName = this.middleName ?? string.Empty;
            if (middleName.EndsWith("вич", StringComparison.CurrentCultureIgnoreCase))
            {
                IsMale = true;
            }
            else if (middleName.EndsWith("вна", StringComparison.CurrentCultureIgnoreCase))
            {
                IsMale = false;
            }
        }

        private DateTime? birthDate;

        public DateTime? BirthDate
        {
            get { return birthDate; }
            set
            {
                if (value.HasValue)
                {
                    value = value.Value.Date;
                }
                if (SetTrackedProperty(ref birthDate, value))
                {
                    if (!IsChild)
                    {
                        HealthGroupId = SpecialValues.NonExistingId;
                    }
                    OnPropertyChanged(() => IsChild);
                    CheckForDuplicatePersonAsync(CreateDuplicateCheckParameters());
                }
            }
        }

        public bool IsChild
        {
            get { return BirthDate != null && BirthDate.Value.AddYears(18) > DateTime.Today; }
        }

        private string snils;

        public string Snils
        {
            get { return snils; }
            set
            {
                if (SetTrackedProperty(ref snils, value))
                {
                    CheckForDuplicatePersonAsync(CreateDuplicateCheckParameters());
                }
            }
        }

        private string ambNumber;

        public string AmbNumber
        {
            get { return ambNumber; }
            set { SetProperty(ref ambNumber, value); }
        }

        private string medNumber;

        public string MedNumber
        {
            get { return medNumber; }
            set { SetTrackedProperty(ref medNumber, value); }
        }

        private bool isMale;

        public bool IsMale
        {
            get { return isMale; }
            set
            {
                SetTrackedProperty(ref isMale, value);
                CollectionViewSource.GetDefaultView(Relationships).Refresh();
            }
        }

        private string phones;

        public string Phones
        {
            get { return phones; }
            set { SetTrackedProperty(ref phones, value); }
        }

        private string email;

        public string Email
        {
            get { return email; }
            set { SetTrackedProperty(ref email, value); }
        }

        private int nationalityId;

        public int NationalityId
        {
            get { return nationalityId; }
            set { SetTrackedProperty(ref nationalityId, value); }
        }

        private int educationId;

        public int EducationId
        {
            get { return educationId; }
            set { SetTrackedProperty(ref educationId, value); }
        }

        private int maritalStatusId;

        public int MaritalStatusId
        {
            get { return maritalStatusId; }
            set { SetTrackedProperty(ref maritalStatusId, value); }
        }

        private int healthGroupId;

        public int HealthGroupId
        {
            get { return healthGroupId; }
            set { SetTrackedProperty(ref healthGroupId, value); }
        }

        private bool isNameChanged;

        public bool IsNameChanged
        {
            get { return isNameChanged; }
            set
            {
                if (SetProperty(ref isNameChanged, value) && !value)
                {
                    IsIncorrectName = false;
                    IsNewName = false;
                    NewNameStartDate = null;
                }
            }
        }

        private void UpdateNameIsChanged()
        {
            IsNameChanged = currentPerson != null && !currentPerson.Id.IsNewOrNonExisting()
                            && (currentInstanceChangeTracker.PropertyHasChanges(() => LastName)
                                || currentInstanceChangeTracker.PropertyHasChanges(() => FirstName)
                                || currentInstanceChangeTracker.PropertyHasChanges(() => MiddleName));
        }

        private bool isIncorrectName;

        public bool IsIncorrectName
        {
            get { return isIncorrectName; }
            set
            {
                if (SetProperty(ref isIncorrectName, value) && value)
                {
                    IsNewName = false;
                    OnPropertyChanged(() => IsNewName);
                }
            }
        }

        private bool isNewName;

        public bool IsNewName
        {
            get { return isNewName; }
            set
            {
                if (SetProperty(ref isNewName, value) && value)
                {
                    IsIncorrectName = false;
                    OnPropertyChanged(() => IsNewName);
                }
            }
        }

        private DateTime? newNameStartDate;

        public DateTime? NewNameStartDate
        {
            get { return newNameStartDate; }
            set { SetProperty(ref newNameStartDate, value); }
        }

        public Person CurrentPerson
        {
            get { return currentPerson; }
        }

        private ImageSource photoSource;

        public ImageSource PhotoSource
        {
            get { return photoSource; }
            set { SetTrackedProperty(ref photoSource, value); }
        }

        private readonly ActionRequiringNotificationViewModel requestLoadSimilarPatientNotification;

        private readonly ActionRequiringNotificationViewModel requestNavigationToRelativeNotification;

        private Person similarPatient;

        private async void LoadSimilarPatientAsync()
        {
            NotificationMediator.Deactivate();
            if (similarPatient == null || similarPatient.Id.IsNewOrNonExisting())
            {
                return;
            }
            if (IsRelative)
            {
                if (similarPatient.Id == personRelative.PersonId)
                {
                    NotificationMediator.Activate("Пациент не может быть родственником самого себя", NotificationMediator.DefaultHideTime);
                    return;
                }
                var args = new CurrentPatientHasRelativeCheckEventArgs(similarPatient.Id);
                OnCurrentPatientHasRelativeCheckRequired(args);
                if (args.HasThisRelative)
                {
                    NotificationMediator.Activate(requestNavigationToRelativeNotification);
                }
                else
                {
                    await LoadPatientInfoAsync(similarPatient.Id);
                }
            }
            else
            {
                eventAggregator.GetEvent<SelectionChangedEvent<Person>>().Publish(similarPatient.Id);
            }
        }

        public event EventHandler<DataEventArgs<int>> RelativeNavigationRequested;

        protected virtual void OnRelativeNavigationRequested(int relativeId)
        {
            var handler = RelativeNavigationRequested;
            if (handler != null)
            {
                handler(this, new DataEventArgs<int>(relativeId));
            }
        }

        public event EventHandler RelativeRemoveRequested;

        protected virtual void OnRelativeRemoveRequested()
        {
            var handler = RelativeRemoveRequested;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        public event EventHandler<CurrentPatientHasRelativeCheckEventArgs> CurrentPatientHasRelativeCheckRequired;

        protected virtual void OnCurrentPatientHasRelativeCheckRequired(CurrentPatientHasRelativeCheckEventArgs e)
        {
            var handler = CurrentPatientHasRelativeCheckRequired;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private DuplicatePersonCheckParameters CreateDuplicateCheckParameters()
        {
            return new DuplicatePersonCheckParameters
            {
                Id = currentPerson == null ? SpecialValues.NewId : currentPerson.Id,
                LastName = LastName.Trim(),
                FirstName = FirstName.Trim(),
                MiddleName = MiddleName.Trim(),
                BirthDate = BirthDate,
                Snils = Snils
            };
        }

        private void RequestNavigationToRelative()
        {
            NotificationMediator.Deactivate();
            OnRelativeNavigationRequested(similarPatient.Id);
            if (currentPerson == null || currentPerson.Id.IsNewOrNonExisting())
            {
                OnRelativeRemoveRequested();
            }
            else
            {
                CancelChanges();
            }
        }

        private async void CheckForDuplicatePersonAsync(DuplicatePersonCheckParameters param)
        {
            if (currentPerson == null || currentPerson.Id.IsNewOrNonExisting() || !ChangeTracker.HasChanges)
            {
                return;
            }
            NotificationMediator.Deactivate();
            await Task.Delay(TimeSpan.FromSeconds(2.0));
            var currentParam = CreateDuplicateCheckParameters();
            if (!currentParam.Equals(param))
            {
                return;
            }
            var patient = await patientService.CheckIfSimilarPatientExistsAsync(param);
            currentParam = CreateDuplicateCheckParameters();
            if (!currentParam.Equals(param))
            {
                return;
            }
            similarPatient = patient;
            if (similarPatient == null || similarPatient.Id == currentPerson.Id)
            {
                return;
            }
            requestLoadSimilarPatientNotification.Message = similarPatient.Snils == Snils && !string.IsNullOrEmpty(Snils)
                ? "В базе данных уже есть другой пациент с таким СНИЛС. Возможно это тот же самый пациент. Загрузить его данные вместо текущих?" 
                : "В базе данных уже есть другой пациент с тикими Ф.И.О. и датой рождения. Загрузить его данные вместо текущих?";
            NotificationMediator.Activate(requestLoadSimilarPatientNotification);
        }

        #endregion

        public ICommand TakePhotoCommand { get; set; }

        private PhotoViewModel photoViewModel;

        private async void TakePhotoAsync()
        {
            if (photoViewModel == null)
            {
                photoViewModel = new PhotoViewModel();
            }
            var dialogResult = await dialogService.ShowDialogAsync(photoViewModel);
            if (dialogResult == true)
            {
                PhotoSource = photoViewModel.SnapshotTaken;
                photoViewModel.SnapshotBitmap = null;
                photoViewModel.SnapshotTaken = null;
            }
        }

        public ICommand ActivateChildContentCommand { get; private set; }

        private void ActivateChildContent(object activeViewModel)
        {
            ActiveChildContent = activeViewModel;
        }

        private object activeChildContent;

        public object ActiveChildContent
        {
            get { return activeChildContent; }
            set { SetProperty(ref activeChildContent, value); }
        }

        #region Actions

        public async Task SaveChangesAsync(Person relativeToPerson = null)
        {
            //If there are no changes then there is nothing to save
            if (currentPerson != null && currentPerson.Id != SpecialValues.NewId && !ChangeTracker.HasChanges)
            {
                return;
            }
            await currentOperation.Task;
            if (!Validate())
            {
                NotificationMediator.Activate("Некоторые поля незаполнены или заполнены неправильно", NotificationMediator.DefaultHideTime);
                return;
            }
            currentOperation = new TaskCompletionSource<object>();
            try
            {
                log.InfoFormat("Saving data for patient with Id = {0}", currentPerson == null || currentPerson.Id == SpecialValues.NewId ? "(New patient)" : currentPerson.Id.ToString());
                BusyMediator.Activate("Сохранение изменений...");
                var personRelative = PersonRelative;
                if (relativeToPerson != null && personRelative != null)
                {
                    personRelative.PersonId = relativeToPerson.Id;
                }
                //If validation was successfull then BirthDate.Value can't be null
                var saveData = new SavePatientInput
                               {
                                   CurrentName = currentName == null
                                                     ? null
                                                     : new PersonName
                                                       {
                                                           LastName = currentName.LastName,
                                                           FirstName = currentName.FirstName,
                                                           MiddleName = currentName.MiddleName,
                                                           BeginDateTime = currentName.BeginDateTime,
                                                           EndDateTime = currentName.EndDateTime,
                                                           Id = currentName.Id,
                                                           PersonId = currentName.PersonId
                                                       },
                                   NewName = new PersonName
                                             {
                                                 LastName = LastName,
                                                 FirstName = FirstName,
                                                 MiddleName = MiddleName
                                             },
                                   CurrentPerson = new Person
                                                   {
                                                       Id = currentPerson == null ? SpecialValues.NewId : currentPerson.Id,
                                                       BirthDate = BirthDate.Value.Date,
                                                       Snils = Snils,
                                                       MedNumber = MedNumber,
                                                       IsMale = IsMale,
                                                       Phones = Phones,
                                                       Email = Email,
                                                       PhotoId = currentPerson == null ? SpecialValues.NewId : currentPerson.PhotoId,
                                                       AmbNumberString = currentPerson == null ? string.Empty : currentPerson.AmbNumberString,
                                                       AmbNumberCreationDate = currentPerson == null ? null : currentPerson.AmbNumberCreationDate
                                                   },
                                   IsIncorrectName = IsIncorrectName,
                                   IsNewName = IsNewName || currentName == null,
                                   NewNameStartDate = (NewNameStartDate ?? SpecialValues.MinDate).Date,
                                   CurrentEducation = currentEducation,
                                   NewEducation = new PersonEducation { EducationId = EducationId },
                                   CurrentHealthGroup = currentHealthGroup,
                                   NewHealthGroup = new PersonHealthGroup { HealthGroupId = HealthGroupId },
                                   CurrentMaritalStatus = currentMaritalStatus,
                                   NewMaritalStatus = new PersonMaritalStatus { MaritalStatusId = MaritalStatusId },
                                   CurrentNationality = currentNationality,
                                   NewNationality = new PersonNationality { CountryId = NationalityId },
                                   CurrentIdentityDocuments = currentIdentityDocuments ?? new PersonIdentityDocument[0],
                                   NewIdentityDocuments = IdentityDocuments.Model,
                                   CurrentInsuranceDocuments = currentInsuranceDocuments ?? new InsuranceDocument[0],
                                   NewInsuranceDocuments = InsuranceDocuments.Model,
                                   CurrentAddresses = currentAddresses ?? new PersonAddress[0],
                                   NewAddresses = Addresses.Model,
                                   CurrentDisabilities = currentDisabilityDocuments ?? new PersonDisability[0],
                                   NewDisabilities = DisabilityDocuments.Model,
                                   CurrentSocialStatuses = currentSocialStatuses ?? new PersonSocialStatus[0],
                                   NewSocialStatuses = SocialStatuses.Model,
                                   Relative = personRelative,
                                   NewPhoto = ChangeTracker.PropertyHasChanges(() => PhotoSource) ? fileService.GetBinaryDataFromImage(new JpegBitmapEncoder(), PhotoSource) : null
                               };

                var result = await patientService.SavePatientAsync(saveData);
                currentPerson = result.Person;
                currentName = result.Name;
                currentNationality = result.Nationality;
                currentHealthGroup = result.HealthGroup;
                currentMaritalStatus = result.MaritalStatus;
                currentEducation = result.Education;
                IdentityDocuments.Model = currentIdentityDocuments = result.IdentityDocuments;
                InsuranceDocuments.Model = currentInsuranceDocuments = result.InsuranceDocuments;
                Addresses.Model = currentAddresses = result.Addresses;
                DisabilityDocuments.Model = currentDisabilityDocuments = result.DisabilityDocuments;
                SocialStatuses.Model = currentSocialStatuses = result.SocialStatuses;
                PersonRelative = result.Relative;
                CancelValidation();
                ChangeTracker.AcceptChanges();
                ChangeTracker.IsEnabled = true;
                UpdateNameIsChanged();
                NotificationMediator.Activate("Изменения сохранены", NotificationMediator.DefaultHideTime);
            }
            catch (OperationCanceledException)
            {
                //Nothing to do as it means that we somehow cancelled save operation
            }
            catch (Exception ex)
            {
                log.ErrorFormatEx(ex, "Failed to save patient info for patient with Id {0}", currentPerson == null || currentPerson.Id == SpecialValues.NewId
                                                                                                 ? "(New patient)"
                                                                                                 : currentPerson.Id.ToString());
                throw;
            }
            finally
            {
                currentOperation.SetResult(null);
                BusyMediator.Deactivate();
            }
        }

        public void CancelChanges()
        {
            FailureMediator.Deactivate();
            ChangeTracker.RestoreChanges();
            CancelValidation();
            UpdateNameIsChanged();
        }

        #endregion

        private readonly CommandWrapper reloadPatientDataCommandWrapper;

        private TaskCompletionSource<object> currentOperation;

        public async Task LoadPatientInfoAsync(int patientId)
        {
            var dataSourcesLoaded = await EnsureDataSourceLoaded();
            if (!dataSourcesLoaded)
            {
                return;
            }
            await currentOperation.Task;
            if (currentPerson != null && currentPerson.Id == patientId)
            {
                return;
            }
            ClearData();
            if (patientId == SpecialValues.NewId || patientId == SpecialValues.NonExistingId)
            {
                currentPerson = new Person { Id = SpecialValues.NewId, AmbNumberString = string.Empty };
                ChangeTracker.IsEnabled = true;
                return;
            }
            currentOperation = new TaskCompletionSource<object>();
            IDisposableQueryable<Person> patientQuery = null;
            try
            {
                BusyMediator.Activate(string.Format("Загрузка данных {0}...", IsRelative ? "родственника" : "пациента"));
                log.InfoFormat("Loading common info for {0} with Id {1}...", IsRelative ? "relative" : "patient", patientId);
                patientQuery = patientService.GetPatientQuery(patientId);
                var result = await patientQuery.Select(x => new
                                                            {
                                                                CurrentName = x.PersonNames.FirstOrDefault(y => y.EndDateTime == SpecialValues.MaxDate),
                                                                CurrentPerson = x,
                                                                CurrentHealthGroup = x.PersonHealthGroups.FirstOrDefault(y => y.EndDateTime == SpecialValues.MaxDate),
                                                                CurrentEducation = x.PersonEducations.FirstOrDefault(y => y.EndDateTime == SpecialValues.MaxDate),
                                                                CurrentMaritalStatus = x.PersonMaritalStatuses.FirstOrDefault(y => y.EndDateTime == SpecialValues.MaxDate),
                                                                CurrentNationality = x.PersonNationalities.FirstOrDefault(y => y.EndDateTime == SpecialValues.MaxDate),
                                                                CurrentIdentityDocuments = x.PersonIdentityDocuments,
                                                                CurrentInsuranceDocuments = x.InsuranceDocuments,
                                                                CurrentAddresses = x.PersonAddresses.Where(y => y.AddressType.Category.IndexOf(AddressTypeCategory.Registry.ToString()) != -1),
                                                                CurrentDisabilityDocuments = x.PersonDisabilities,
                                                                CurrentSocialStatuses = x.PersonSocialStatuses
                                                            })
                                               .FirstOrDefaultAsync();
                if (result.CurrentPerson.PhotoId != null)
                {
                    using (var documentQuery = documentService.GetDocumentById(result.CurrentPerson.PhotoId.Value))
                    {
                        var document = await documentQuery.FirstAsync();
                        PhotoSource = fileService.GetImageSourceFromBinaryData(document.FileData);
                    }
                }

                currentName = result.CurrentName;
                currentPerson = result.CurrentPerson;
                currentHealthGroup = result.CurrentHealthGroup;
                currentEducation = result.CurrentEducation;
                currentMaritalStatus = result.CurrentMaritalStatus;
                currentNationality = result.CurrentNationality;
                IdentityDocuments.Model = currentIdentityDocuments = result.CurrentIdentityDocuments;
                InsuranceDocuments.Model = currentInsuranceDocuments = result.CurrentInsuranceDocuments;
                Addresses.Model = currentAddresses = result.CurrentAddresses.ToArray();
                DisabilityDocuments.Model = currentDisabilityDocuments = result.CurrentDisabilityDocuments;
                SocialStatuses.Model = currentSocialStatuses = result.CurrentSocialStatuses;

                LastName = currentName == null ? PersonName.UnknownLastName : currentName.LastName;
                FirstName = currentName == null ? PersonName.UnknownFirstName : currentName.FirstName;
                MiddleName = currentName == null ? string.Empty : currentName.MiddleName;
                IsMale = currentPerson.IsMale;
                BirthDate = currentPerson.BirthDate;
                Snils = currentPerson.Snils;
                AmbNumber = CurrentPerson.AmbNumberString;
                MedNumber = currentPerson.MedNumber;
                Phones = currentPerson.Phones;
                Email = currentPerson.Email;

                HealthGroupId = currentHealthGroup == null ? SpecialValues.NonExistingId : currentHealthGroup.HealthGroupId;
                NationalityId = currentNationality == null ? SpecialValues.NonExistingId : currentNationality.CountryId;
                EducationId = currentEducation == null ? SpecialValues.NonExistingId : currentEducation.EducationId;
                MaritalStatusId = currentMaritalStatus == null ? SpecialValues.NonExistingId : currentMaritalStatus.MaritalStatusId;

                ChangeTracker.IsEnabled = true;
            }
            catch (OperationCanceledException)
            {
                //Do nothing. Cancelled operation means that user selected different patient before previous one was loaded
            }
            catch (Exception ex)
            {
                log.ErrorFormatEx(ex, "Failed to load common patient info for {0} with Id {1}", IsRelative ? "relative" : "patient", patientId);
                FailureMediator.Activate(string.Format("Не удалость загрузить данные {0}. Попробуйте еще раз или обратитесь в службу поддержки", IsRelative ? "родственника" : "пациента"),
                                         reloadPatientDataCommandWrapper,
                                         ex);
                throw;
            }
            finally
            {
                currentOperation.SetResult(null);
                BusyMediator.Deactivate();
                if (patientQuery != null)
                {
                    patientQuery.Dispose();
                }
            }
        }

        private void ClearData()
        {
            ChangeTracker.IsEnabled = false;
            currentPerson = null;
            currentEducation = null;
            currentHealthGroup = null;
            currentMaritalStatus = null;
            currentName = null;
            currentNationality = null;
            IdentityDocuments.Model = currentIdentityDocuments = null;
            InsuranceDocuments.Model = currentInsuranceDocuments = null;
            Addresses.Model = currentAddresses = null;
            DisabilityDocuments.Model = currentDisabilityDocuments = null;
            SocialStatuses.Model = currentSocialStatuses = null;
            lastName = string.Empty;
            firstName = string.Empty;
            middleName = string.Empty;
            birthDate = null;
            snils = string.Empty;
            medNumber = string.Empty;
            isMale = true;
            phones = string.Empty;
            email = string.Empty;
            nationalityId = SpecialValues.NonExistingId;
            maritalStatusId = SpecialValues.NonExistingId;
            educationId = SpecialValues.NonExistingId;
            healthGroupId = SpecialValues.NonExistingId;
            photoSource = null;
            OnPropertyChanged(string.Empty);
        }

        #region Implementation IDataErrorInfo

        public string this[string columnName]
        {
            get { return validator[columnName]; }
        }

        public string Error
        {
            get { return validator.Error; }
        }

        public bool Validate()
        {
            return validator.Validate()
                   & IdentityDocuments.Validate()
                   & InsuranceDocuments.Validate()
                   & Addresses.Validate()
                   & DisabilityDocuments.Validate()
                   & SocialStatuses.Validate();
        }

        public void CancelValidation()
        {
            validator.CancelValidation();
            IdentityDocuments.CancelValidation();
            InsuranceDocuments.CancelValidation();
            Addresses.CancelValidation();
            DisabilityDocuments.CancelValidation();
        }

        private class ValidationMediator : ValidationMediator<PatientInfoViewModel>
        {
            public ValidationMediator(PatientInfoViewModel associatedItem)
                : base(associatedItem)
            {
            }

            protected override void OnValidateProperty(string propertyName)
            {
                if (PropertyNameEquals(propertyName, x => x.LastName))
                {
                    ValidateLastName();
                }
                else if (PropertyNameEquals(propertyName, x => x.FirstName))
                {
                    ValidateFirstName();
                }
                else if (PropertyNameEquals(propertyName, x => x.BirthDate))
                {
                    ValidateBirthDate();
                }
                else if (PropertyNameEquals(propertyName, x => x.Snils))
                {
                    ValidateSnils();
                }
                else if (PropertyNameEquals(propertyName, x => x.MedNumber))
                {
                    ValidateMedNumber();
                }
                else if (PropertyNameEquals(propertyName, x => x.IsIncorrectName) || PropertyNameEquals(propertyName, x => x.IsNewName))
                {
                    ValidateIsNewOrIncorrectName();
                }
                else if (PropertyNameEquals(propertyName, x => x.NewNameStartDate))
                {
                    ValidateNewNameStartDate();
                }
                else if (PropertyNameEquals(propertyName, x => x.NationalityId))
                {
                    ValidateNationalityId();
                }
                else if (PropertyNameEquals(propertyName, x => x.HealthGroupId))
                {
                    ValidateHealthGroupId();
                }
                else if (PropertyNameEquals(propertyName, x => x.SelectedRelationship))
                {
                    ValidateSelectedRelationship();
                }
            }

            protected override void RaiseAssociatedObjectPropertyChanged()
            {
                AssociatedItem.OnPropertyChanged(string.Empty);
            }

            protected override void OnValidate()
            {
                ValidateLastName();
                ValidateFirstName();
                ValidateBirthDate();
                ValidateSnils();
                ValidateMedNumber();
                ValidateIsNewOrIncorrectName();
                ValidateNewNameStartDate();
                ValidateNationalityId();
                ValidateHealthGroupId();
                ValidateSelectedRelationship();
            }

            private void ValidateLastName()
            {
                SetError(x => x.LastName, string.IsNullOrWhiteSpace(AssociatedItem.LastName) ? "Фамилия не может быть пустой" : string.Empty);
            }

            private void ValidateFirstName()
            {
                SetError(x => x.FirstName, string.IsNullOrWhiteSpace(AssociatedItem.FirstName) ? "Имя не может быть пустым" : string.Empty);
            }

            private void ValidateBirthDate()
            {
                var error = string.Empty;
                if (AssociatedItem.BirthDate == null)
                {
                    error = "Не выбрана дата рождения";
                }
                else if (AssociatedItem.BirthDate > DateTime.Today)
                {
                    error = "Дата рождения не может быть в будущем";
                }
                SetError(x => x.BirthDate, error);
            }

            private void ValidateSnils()
            {
                SetError(x => x.Snils,
                         !string.IsNullOrEmpty(AssociatedItem.Snils) && AssociatedItem.Snils.Length != Person.FullSnilsLength
                             ? "СНИЛС должен либо быть пустым либо быть в формате 000-000-000 00"
                             : string.Empty);
            }

            private void ValidateMedNumber()
            {
                SetError(x => x.MedNumber,
                         !string.IsNullOrEmpty(AssociatedItem.MedNumber) && AssociatedItem.MedNumber.Length != Person.FullMedNumberLength
                             ? "ЕМН должен либо быть пустым либо содержать ровно шестнадцать цифр"
                             : string.Empty);
            }

            private void ValidateIsNewOrIncorrectName()
            {
                var error = AssociatedItem.IsNameChanged && !AssociatedItem.IsNewName && !AssociatedItem.IsIncorrectName ? "Выберите причину смены Ф.И.О." : string.Empty;
                SetError(x => x.IsNewName, error);
                SetError(x => x.IsIncorrectName, error);
            }

            private void ValidateNewNameStartDate()
            {
                var error = string.Empty;
                if (AssociatedItem.IsNewName)
                {
                    if (AssociatedItem.NewNameStartDate == null)
                    {
                        error = "Укажите дату, с которой вступили силу изменения Ф.И.О.";
                    }
                    else if (AssociatedItem.NewNameStartDate < AssociatedItem.BirthDate)
                    {
                        error = "Дата смены Ф.И.О. не может быть меньше даты рождения";
                    }
                    else if (AssociatedItem.NewNameStartDate < AssociatedItem.currentName.BeginDateTime)
                    {
                        error = string.Format("Дата смены Ф.И.О. не может быть меньше предыдущей даты смены Ф.И.О. ({0})",
                                              AssociatedItem.currentName.BeginDateTime.ToString(DateTimeFormats.ShortDateFormat));
                    }
                }
                SetError(x => x.NewNameStartDate, error);
            }

            private void ValidateNationalityId()
            {
                SetError(x => x.NationalityId, AssociatedItem.NationalityId == SpecialValues.NonExistingId ? "Укажите гражданство пациента" : string.Empty);
            }

            private void ValidateHealthGroupId()
            {
                SetError(x => x.HealthGroupId, AssociatedItem.IsChild && AssociatedItem.HealthGroupId == SpecialValues.NonExistingId ? "Группа здоровья обязательна для лиц до 18 лет" : string.Empty);
            }

            private void ValidateSelectedRelationship()
            {
                SetError(x => x.SelectedRelationship, AssociatedItem.IsRelative && AssociatedItem.SelectedRelationship == null ? "Не указана родственная связь" : string.Empty);
            }
        }

        #endregion

        private readonly IChangeTracker currentInstanceChangeTracker;

        public IChangeTracker ChangeTracker { get; private set; }

        public void Dispose()
        {
            currentInstanceChangeTracker.PropertyChanged -= OnChangesTracked;
            currentInstanceChangeTracker.Dispose();
            reloadDataSourceCommandWrapper.Dispose();
            reloadPatientDataCommandWrapper.Dispose();
        }

        private class DataSource
        {
            public IEnumerable<Country> Countries { get; set; }

            public IEnumerable<Education> Educations { get; set; }

            public IEnumerable<MaritalStatus> MaritalStatuses { get; set; }

            public IEnumerable<HealthGroup> HealthGroups { get; set; }

            public IEnumerable<RelativeRelationship> Relationships { get; set; }
        }
    }
}