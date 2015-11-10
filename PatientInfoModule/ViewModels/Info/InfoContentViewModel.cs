using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Core.Data;
using Core.Data.Misc;
using Core.Extensions;
using Core.Misc;
using Core.Wpf.Events;
using Core.Wpf.Misc;
using Core.Wpf.Mvvm;
using log4net;
using PatientInfoModule.Data;
using PatientInfoModule.Misc;
using PatientInfoModule.Services;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;

namespace PatientInfoModule.ViewModels
{
    public class InfoContentViewModel : TrackableBindableBase, INavigationAware, IDataErrorInfo, IChangeTrackerMediator, IDisposable
    {
        private const int FullSnilsLength = 14;

        private const int FullMedNumberLength = 16;

        private readonly IPatientService patientService;

        private readonly ILog log;

        private readonly IEventAggregator eventAggregator;

        public InfoContentViewModel(IPatientService patientService,
                                    ILog log,
                                    IEventAggregator eventAggregator,
                                    IdentityDocumentCollectionViewModel identityDocumentCollectionViewModel)
        {
            if (patientService == null)
            {
                throw new ArgumentNullException("patientService");
            }
            if (log == null)
            {
                throw new ArgumentNullException("log");
            }
            if (eventAggregator == null)
            {
                throw new ArgumentNullException("eventAggregator");
            }
            if (identityDocumentCollectionViewModel == null)
            {
                throw new ArgumentNullException("identityDocumentCollectionViewModel");
            }
            IdentityDocuments = identityDocumentCollectionViewModel;
            this.patientService = patientService;
            this.log = log;
            this.eventAggregator = eventAggregator;
            patientIdBeingSelected = SpecialValues.NonExistingId;
            currentInstanceChangeTracker = new ChangeTrackerEx<InfoContentViewModel>(this);
            var changeTracker = new CompositeChangeTracker(currentInstanceChangeTracker, IdentityDocuments.ChangeTracker);
            currentInstanceChangeTracker.RegisterComparer(() => LastName, StringComparer.CurrentCultureIgnoreCase);
            currentInstanceChangeTracker.RegisterComparer(() => FirstName, StringComparer.CurrentCultureIgnoreCase);
            currentInstanceChangeTracker.RegisterComparer(() => MiddleName, StringComparer.CurrentCultureIgnoreCase);
            changeTracker.PropertyChanged += OnChangesTracked;
            ChangeTracker = changeTracker;
            BusyMediator = new BusyMediator();
            FailureMediator = new FailureMediator();
            ActivateChildContentCommand = new DelegateCommand<object>(ActivateChildContent);
            createNewPatientCommand = new DelegateCommand(CreatetNewPatient);
            saveChangesCommand = new DelegateCommand(SaveChangesAsync, CanSaveChanges);
            cancelChangesCommand = new DelegateCommand(CancelChanges, CanCancelChanges);
            reloadPatientDataCommandWrapper = new CommandWrapper { Command = new DelegateCommand(() => SelectPatientAsync(patientIdBeingSelected)) };
            saveChangesCommandWrapper = new CommandWrapper { Command = saveChangesCommand };
            reloadDataSourceCommandWrapper = new CommandWrapper { Command = new DelegateCommand(() => EnsureDataSourceLoaded()) };
        }

        public IdentityDocumentCollectionViewModel IdentityDocuments { get; private set; }

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
                var result = await Task<DataSource>.Factory.StartNew(LoadDataSource);
                Educations = result.Educations;
                HealthGroups = result.HealthGroups;
                Nationalities = result.Countries;
                MaritalStatuses = result.MaritalStatuses;
                log.InfoFormat("Data sources for patient info content are successfully loaded");
                dataSourcesLoadingTaskSource.SetResult(true);
            }
            catch (Exception ex)
            {
                log.Error("Failed to load data sources for patient info content", ex);
                FailureMediator.Activate("Не удалось загрузить общие данные. Попробуйте еще раз или обратитесь в службу поддержки", reloadDataSourceCommandWrapper, ex);
                dataSourcesLoadingTaskSource.SetResult(false);
            }
            finally
            {
                BusyMediator.Deactivate();
            }
            return await dataSourcesLoadingTaskSource.Task;
        }

        private DataSource LoadDataSource()
        {
            return new DataSource
                   {
                       Countries = patientService.GetCountries().ToArray(),
                       Educations = patientService.GetEducations().ToArray(),
                       HealthGroups = patientService.GetHealthGroups().ToArray(),
                       MaritalStatuses = patientService.GetMaritalStatuses().ToArray()
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

        #endregion

        private void OnChangesTracked(object sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.PropertyName) || string.CompareOrdinal(e.PropertyName, "HasChanges") == 0)
            {
                UpdateChangeCommandsState();
            }
        }

        private void UpdateChangeCommandsState()
        {
            saveChangesCommand.RaiseCanExecuteChanged();
            cancelChangesCommand.RaiseCanExecuteChanged();
        }

        public BusyMediator BusyMediator { get; set; }

        public FailureMediator FailureMediator { get; private set; }

        private PersonName currentName;

        private Person currentPerson;

        private PersonEducation currentEducation;

        private PersonNationality currentNationality;

        private PersonMaritalStatus currentMaritalStatus;

        private PersonHealthGroup currentHealthGroup;

        private int patientIdBeingSelected;

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
                }
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
                }
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
            set { SetTrackedProperty(ref snils, value); }
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
            set { SetTrackedProperty(ref isMale, value); }
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
            IsNameChanged = currentInstanceChangeTracker.PropertyHasChanges(() => LastName)
                            || currentInstanceChangeTracker.PropertyHasChanges(() => FirstName)
                            || currentInstanceChangeTracker.PropertyHasChanges(() => MiddleName);
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

        #endregion

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

        private readonly DelegateCommand createNewPatientCommand;

        private readonly DelegateCommand saveChangesCommand;

        private readonly DelegateCommand cancelChangesCommand;

        private readonly CommandWrapper saveChangesCommandWrapper;

        public ICommand CreateNewPatientCommand
        {
            get { return createNewPatientCommand; }
        }

        private void CreatetNewPatient()
        {
            eventAggregator.GetEvent<SelectionEvent<Person>>().Publish(SpecialValues.NewId);
        }

        public ICommand SaveChangesCommand
        {
            get { return saveChangesCommand; }
        }

        private async void SaveChangesAsync()
        {
            FailureMediator.Deactivate();
            if (!IsValid)
            {
                return;
            }
            if (currentOperationToken != null)
            {
                currentOperationToken.Cancel();
                currentOperationToken.Dispose();
            }
            currentOperationToken = new CancellationTokenSource();
            var token = currentOperationToken.Token;
            log.InfoFormat("Saving data for patient with Id = {0}", currentPerson == null || currentPerson.Id == SpecialValues.NewId ? "(New patient)" : currentPerson.Id.ToString());
            BusyMediator.Activate("Сохранение изменений...");
            var saveSuccesfull = false;
            try
            {
                //If validation was successfull then BirthDate.Value can't be null
                var saveData = new SavePatientInput
                               {
                                   CurrentName = currentName,
                                   NewName = new PersonName(),
                                   CurrentPerson = currentPerson ?? new Person(),
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
                                   NewNationality = new PersonNationality { CountryId = NationalityId }
                               };
                saveData.CurrentPerson.BirthDate = BirthDate.Value.Date;
                saveData.CurrentPerson.Snils = Snils;
                saveData.CurrentPerson.MedNumber = MedNumber;
                saveData.CurrentPerson.IsMale = IsMale;
                saveData.CurrentPerson.Phones = Phones;
                saveData.CurrentPerson.Email = Email;

                saveData.NewName.LastName = LastName;
                saveData.NewName.FirstName = FirstName;
                saveData.NewName.MiddleName = MiddleName;

                var result = await patientService.SavePatientAsync(saveData, token);
                currentPerson = result.Person;
                currentName = result.Name;
                currentNationality = result.Nationality;
                currentHealthGroup = result.HealthGroup;
                currentMaritalStatus = result.MaritalStatus;
                currentEducation = result.Education;
                saveSuccesfull = true;
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
                FailureMediator.Activate("Не удалось сохранить данные пациента. Попробуйте еще раз или обратитесь в службу поддержки", saveChangesCommandWrapper, ex);
            }
            finally
            {
                BusyMediator.Deactivate();
                if (saveSuccesfull)
                {
                    isValidationRequested = false;
                    ChangeTracker.AcceptChanges();
                    ChangeTracker.IsEnabled = true;
                    UpdateChangeCommandsState();
                    UpdateNameIsChanged();
                    eventAggregator.GetEvent<SelectionEvent<Person>>().Publish(currentPerson.Id);
                }
            }
        }

        private bool CanSaveChanges()
        {
            if (currentPerson == null)
            {
                return false;
            }
            if (currentPerson.Id == SpecialValues.NewId)
            {
                return true;
            }
            return ChangeTracker.HasChanges;
        }

        public ICommand CancelChangesCommand
        {
            get { return cancelChangesCommand; }
        }

        private void CancelChanges()
        {
            FailureMediator.Deactivate();
            ChangeTracker.RestoreChanges();
            isValidationRequested = false;
            OnPropertyChanged(string.Empty);
            UpdateNameIsChanged();
        }

        private bool CanCancelChanges()
        {
            if (currentPerson == null || currentPerson.Id == SpecialValues.NewId)
            {
                return false;
            }
            return ChangeTracker.HasChanges;
        }

        #endregion

        private readonly CommandWrapper reloadPatientDataCommandWrapper;

        private CancellationTokenSource currentOperationToken;

        public async void SelectPatientAsync(int patientId)
        {
            if (Interlocked.Exchange(ref patientIdBeingSelected, patientId) == patientId)
            {
                return;
            }
            if (currentPerson != null && currentPerson.Id == patientId)
            {
                return;
            }
            var dataSourcesLoaded = await EnsureDataSourceLoaded();
            if (!dataSourcesLoaded)
            {
                return;
            }
            ClearData();
            UpdateChangeCommandsState();
            if (patientId == SpecialValues.NewId || patientId == SpecialValues.NonExistingId)
            {
                return;
            }
            if (currentOperationToken != null)
            {
                currentOperationToken.Cancel();
                currentOperationToken.Dispose();
            }
            var loadingIsCompleted = false;
            currentOperationToken = new CancellationTokenSource();
            var token = currentOperationToken.Token;
            BusyMediator.Activate("Загрузка данных пациента...");
            log.InfoFormat("Loading patient info for patient with Id {0}...", patientId);
            IDisposableQueryable<Person> patientQuery = null;
            try
            {
                patientQuery = patientService.GetPatientQuery(patientId);
                var result = await patientQuery.Select(x => new
                                                            {
                                                                CurrentName = x.PersonNames.FirstOrDefault(y => y.EndDateTime == SpecialValues.MaxDate),
                                                                CurrentPerson = x,
                                                                CurrentHealthGroup = x.PersonHealthGroups.FirstOrDefault(y => y.EndDateTime == SpecialValues.MaxDate),
                                                                CurrentEducation = x.PersonEducations.FirstOrDefault(y => y.EndDateTime == SpecialValues.MaxDate),
                                                                CurrentMaritalStatus = x.PersonMaritalStatuses.FirstOrDefault(y => y.EndDateTime == SpecialValues.MaxDate),
                                                                CurrentNationality = x.PersonNationalities.FirstOrDefault(y => y.EndDateTime == SpecialValues.MaxDate)
                                                            })
                                               .FirstOrDefaultAsync(token);
                if (result == null)
                {
                    FailureMediator.Activate("Указанный пациент по какой-то причине отсутствует в базе данных. Пожалуйста, обратитесь в службу поддержки");
                    return;
                }
                currentName = result.CurrentName;
                currentPerson = result.CurrentPerson;
                currentHealthGroup = result.CurrentHealthGroup;
                currentEducation = result.CurrentEducation;
                currentMaritalStatus = result.CurrentMaritalStatus;
                currentNationality = result.CurrentNationality;

                LastName = currentName == null ? PersonName.UnknownLastName : currentName.LastName;
                FirstName = currentName == null ? PersonName.UnknownFirstName : currentName.FirstName;
                MiddleName = currentName == null ? string.Empty : currentName.MiddleName;
                IsMale = currentPerson.IsMale;
                BirthDate = currentPerson.BirthDate;
                Snils = currentPerson.Snils;
                MedNumber = currentPerson.MedNumber;
                Phones = currentPerson.Phones;
                Email = currentPerson.Email;

                HealthGroupId = currentHealthGroup == null ? SpecialValues.NonExistingId : currentHealthGroup.HealthGroupId;
                NationalityId = currentNationality == null ? SpecialValues.NonExistingId : currentNationality.CountryId;
                EducationId = currentEducation == null ? SpecialValues.NonExistingId : currentEducation.EducationId;
                MaritalStatusId = currentMaritalStatus == null ? SpecialValues.NonExistingId : currentMaritalStatus.MaritalStatusId;

                loadingIsCompleted = true;
            }
            catch (OperationCanceledException)
            {
                //Do nothing. Cancelled operation means that user selected different patient before previous one was loaded
            }
            catch (Exception ex)
            {
                log.ErrorFormatEx(ex, "Failed to load common patient info for patient with Id {0}", patientId);
                FailureMediator.Activate("Не удалость загрузить данные пациента. Попробуйте еще раз или обратитесь в службу поддержки", reloadPatientDataCommandWrapper, ex);
                loadingIsCompleted = true;
            }
            finally
            {
                CommandManager.InvalidateRequerySuggested();
                if (loadingIsCompleted)
                {
                    ChangeTracker.IsEnabled = true;
                    UpdateChangeCommandsState();
                    BusyMediator.Deactivate();
                }
                if (patientQuery != null)
                {
                    patientQuery.Dispose();
                }
            }
        }

        private void ClearData()
        {
            ChangeTracker.IsEnabled = false;
            currentPerson = new Person { Id = SpecialValues.NewId, AmbNumberString = string.Empty };
            currentEducation = null;
            currentHealthGroup = null;
            currentMaritalStatus = null;
            currentName = null;
            currentNationality = null;
            LastName = string.Empty;
            FirstName = string.Empty;
            MiddleName = string.Empty;
            BirthDate = null;
            Snils = string.Empty;
            MedNumber = string.Empty;
            IsMale = true;
            Phones = string.Empty;
            Email = string.Empty;
            NationalityId = SpecialValues.NonExistingId;
            MaritalStatusId = SpecialValues.NonExistingId;
            EducationId = SpecialValues.NonExistingId;
            HealthGroupId = SpecialValues.NonExistingId;
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            var targetPatientId = (int?)navigationContext.Parameters[ParameterNames.PatientId] ?? SpecialValues.NonExistingId;
            SelectPatientAsync(targetPatientId);
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            //We use only one view-model for patient info, thus we says that current view-model can accept navigation requests
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            //TODO: place here logic for current view being deactivated
        }

        #region Implementation IDataErrorInfo

        private bool isValidationRequested;

        private readonly HashSet<string> invalidProperties = new HashSet<string>();

        private bool IsValid
        {
            get
            {
                isValidationRequested = true;
                OnPropertyChanged(string.Empty);
                return invalidProperties.Count < 1;
            }
        }

        string IDataErrorInfo.this[string columnName]
        {
            get
            {
                if (!isValidationRequested)
                {
                    invalidProperties.Remove(columnName);
                    return string.Empty;
                }
                var result = string.Empty;
                switch (columnName)
                {
                    case "LastName":
                        result = string.IsNullOrWhiteSpace(LastName) ? "Фамилия не указана" : string.Empty;
                        break;
                    case "FirstName":
                        result = string.IsNullOrWhiteSpace(FirstName) ? "Имя не указано" : string.Empty;
                        break;
                    case "BirthDate":
                        result = !BirthDate.HasValue
                                     ? "Дата рождения не указана"
                                     : BirthDate.Value.Date > DateTime.Today
                                           ? "Дата рождения не может быть в будущем"
                                           : string.Empty;
                        break;
                    case "Snils":
                        result = string.IsNullOrEmpty(Snils) || Snils.Length == FullSnilsLength
                                     ? string.Empty
                                     : "СНИЛС должен либо быть пустым либо быть в формате 000-000-000 00";
                        break;
                    case "MedNumber":
                        result = string.IsNullOrEmpty(MedNumber) || MedNumber.Length == FullMedNumberLength
                                     ? string.Empty
                                     : "ЕМН должен либо быть пустым либо быть в формате 0000000000000000";
                        break;
                    case "IsIncorrectName":
                    case "IsNewName":
                        result = IsNameChanged && !IsNewName && !IsIncorrectName
                                     ? "Выберите причину смены Ф.И.О."
                                     : string.Empty;
                        break;
                    case "NewNameStartDate":
                        result = IsNewName
                                     ? !NewNameStartDate.HasValue
                                           ? "Укажите дату, с которой вступили силу изменения Ф.И.О."
                                           : NewNameStartDate.Value > DateTime.Today
                                                 ? "Дата смены Ф.И.О. не может быть в будущем"
                                                 : string.Empty
                                     : string.Empty;
                        break;
                    case "NationalityId":
                        result = NationalityId == SpecialValues.NonExistingId ? "Укажите гражданство пациента" : string.Empty;
                        break;
                    case "HealthGroupId":
                        result = IsChild && HealthGroupId == SpecialValues.NonExistingId ? "Группа здоровья обязательна для лиц до 18 лет" : string.Empty;
                        break;
                }
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

        private readonly IChangeTracker currentInstanceChangeTracker;

        public IChangeTracker ChangeTracker { get; private set; }

        public void Dispose()
        {
            currentInstanceChangeTracker.PropertyChanged -= OnChangesTracked;
        }

        private class DataSource
        {
            public IEnumerable<Country> Countries { get; set; }

            public IEnumerable<Education> Educations { get; set; }

            public IEnumerable<MaritalStatus> MaritalStatuses { get; set; }

            public IEnumerable<HealthGroup> HealthGroups { get; set; }
        }
    }
}