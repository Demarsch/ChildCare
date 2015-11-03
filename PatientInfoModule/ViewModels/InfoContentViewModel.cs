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
using Core.Services;
using Core.Wpf.Events;
using Core.Wpf.Misc;
using Core.Wpf.Mvvm;
using log4net;
using PatientInfoModule.Data;
using PatientInfoModule.Misc;
using PatientInfoModule.Services;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;

namespace PatientInfoModule.ViewModels
{
    public class InfoContentViewModel : BindableBase, INavigationAware, IDataErrorInfo
    {
        private const int FullSnilsLength = 14;

        private const int FullMedNumberLength = 16;

        private readonly IPatientService patientService;

        private readonly ILog log;

        private readonly ICacheService cacheService;

        private readonly IEventAggregator eventAggregator;

        public InfoContentViewModel(IPatientService patientService, ILog log, ICacheService cacheService, IEventAggregator eventAggregator)
        {
            if (patientService == null)
            {
                throw new ArgumentNullException("patientService");
            }
            if (log == null)
            {
                throw new ArgumentNullException("log");
            }
            if (cacheService == null)
            {
                throw new ArgumentNullException("cacheService");
            }
            if (eventAggregator == null)
            {
                throw new ArgumentNullException("eventAggregator");
            }
            this.patientService = patientService;
            this.log = log;
            this.cacheService = cacheService;
            this.eventAggregator = eventAggregator;
            changeTracker = new ChangeTracker();
            changeTracker.RegisterComparer(() => LastName, StringComparer.CurrentCultureIgnoreCase);
            changeTracker.RegisterComparer(() => FirstName, StringComparer.CurrentCultureIgnoreCase);
            changeTracker.RegisterComparer(() => MiddleName, StringComparer.CurrentCultureIgnoreCase);
            changeTracker.PropertyChanged += OnChangesTracked;
            BusyMediator = new BusyMediator();
            CriticalFailureMediator = new CriticalFailureMediator();
            createNewPatientCommand = new DelegateCommand(CreatetNewPatient);
            saveChangesCommand = new DelegateCommand(SaveChangesAsync, CanSaveChanges);
            cancelChangesCommand = new DelegateCommand(CancelChanges, CanCancelChanges);
            reloadPatientDataCommandWrapper = new CommandWrapper
                                              {
                                                  Command = new DelegateCommand(() => SelectPatientAsync(patientIdBeingSelected)),
                                                  CommandName = "Повторить",
                                              };
            saveChangesCommandWrapper = new CommandWrapper
                                        {
                                            Command = saveChangesCommand,
                                            CommandName = "Повторить",
                                        };
        }

        private void OnChangesTracked(object sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.PropertyName) || string.CompareOrdinal(e.PropertyName, "HasChanges") == 0)
            {
                saveChangesCommand.RaiseCanExecuteChanged();
                cancelChangesCommand.RaiseCanExecuteChanged();
            }
        }

        private readonly ChangeTracker changeTracker;

        public BusyMediator BusyMediator { get; set; }

        public CriticalFailureMediator CriticalFailureMediator { get; private set; }

        private PersonName currentName;

        private Person currentPerson;

        private int patientIdBeingSelected;

        private string lastName;

        public string LastName
        {
            get { return lastName; }
            set
            {
                value = value.Trim();
                changeTracker.Track(lastName, value);
                if (SetProperty(ref lastName, value))
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
                changeTracker.Track(firstName, value);
                if (SetProperty(ref firstName, value))
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
                changeTracker.Track(middleName, value);
                if (SetProperty(ref middleName, value))
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
                changeTracker.Track(birthDate, value);
                SetProperty(ref birthDate, value);
            }
        }

        private string snils;

        public string Snils
        {
            get { return snils; }
            set
            {
                changeTracker.Track(snils, value);
                SetProperty(ref snils, value);
            }
        }

        private string medNumber;

        public string MedNumber
        {
            get { return medNumber; }
            set
            {
                changeTracker.Track(medNumber, value);
                SetProperty(ref medNumber, value);
            }
        }

        private bool isMale;

        public bool IsMale
        {
            get { return isMale; }
            set
            {
                changeTracker.Track(isMale, value);
                SetProperty(ref isMale, value);
            }
        }

        private string phones;

        public string Phones
        {
            get { return phones; }
            set
            {
                changeTracker.Track(phones, value);
                SetProperty(ref phones, value);
            }
        }

        private string email;

        public string Email
        {
            get { return email; }
            set
            {
                changeTracker.Track(email, value);
                SetProperty(ref email, value);
            }
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
            IsNameChanged = changeTracker.PropertyHasChanges(() => LastName)
                            || changeTracker.PropertyHasChanges(() => FirstName)
                            || changeTracker.PropertyHasChanges(() => MiddleName);
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
            CriticalFailureMediator.Deactivate();
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
                                   NewName = new PersonName { BeginDateTime = SpecialValues.MinDate },
                                   CurrentPerson = currentPerson ?? new Person(),
                                   IsIncorrectName = IsIncorrectName,
                                   IsNewName = IsNewName || currentName == null,
                                   NewNameStartDate = (NewNameStartDate ?? SpecialValues.MinDate).Date,
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

                var savePatientTask = patientService.SavePatientAsync(saveData, token);
                await Task.WhenAll(Task.Delay(AppConfiguration.PendingOperationDelay, token), savePatientTask);
                var result = savePatientTask.Result;
                currentPerson = result.CurrentPerson;
                currentName = result.CurrentName;
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
                CriticalFailureMediator.Activate("Не удалось сохранить данные пациента. Попробуйте еще раз или обратитесь в службу поддержки", saveChangesCommandWrapper, ex);
            }
            finally
            {
                BusyMediator.Deactivate();
                if (saveSuccesfull)
                {
                    changeTracker.UntrackAll();
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
            return changeTracker.HasChanges;
        }

        public ICommand CancelChangesCommand
        {
            get { return cancelChangesCommand; }
        }

        private void CancelChanges()
        {
            CriticalFailureMediator.Deactivate();
            changeTracker.Untrack(ref lastName, () => LastName);
            changeTracker.Untrack(ref firstName, () => FirstName);
            changeTracker.Untrack(ref middleName, () => MiddleName);
            changeTracker.Untrack(ref birthDate, () => BirthDate);
            changeTracker.Untrack(ref isMale, () => IsMale);
            changeTracker.Untrack(ref snils, () => Snils);
            changeTracker.Untrack(ref medNumber, () => MedNumber);
            changeTracker.Untrack(ref phones, () => Phones);
            changeTracker.Untrack(ref email, () => Email);
            saveWasRequested = false;
            OnPropertyChanged(string.Empty);
            UpdateNameIsChanged();
        }

        private bool CanCancelChanges()
        {
            if (currentPerson == null || currentPerson.Id == SpecialValues.NewId)
            {
                return false;
            }
            return changeTracker.HasChanges;
        }

        #endregion

        private readonly CommandWrapper reloadPatientDataCommandWrapper;

        private CancellationTokenSource currentOperationToken;

        public async void SelectPatientAsync(int patientId)
        {
            if (currentPerson != null && currentPerson.Id == patientId)
            {
                return;
            }
            ClearData();
            saveChangesCommand.RaiseCanExecuteChanged();
            cancelChangesCommand.RaiseCanExecuteChanged();
            if (patientId == SpecialValues.NewId || patientId == SpecialValues.NonExistingId)
            {
                return;
            }
            patientIdBeingSelected = patientId;
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
                var loadPatientTask = patientQuery.Select(x => new
                                                               {
                                                                   CurrentName = x.PersonNames.FirstOrDefault(y => y.EndDateTime == null || y.EndDateTime == DateTime.MaxValue),
                                                                   CurrentPerson = x
                                                               })
                                                  .FirstOrDefaultAsync(token);
                await Task.WhenAll(loadPatientTask, Task.Delay(AppConfiguration.PendingOperationDelay, token));
                var result = loadPatientTask.Result;
                if (result == null)
                {
                    CriticalFailureMediator.Activate("Указанный пациент по какой-то причине отсутсвует в базе данных. Пожалуйста, обратитесь в службу поддержки");
                    return;
                }
                currentName = result.CurrentName;
                currentPerson = result.CurrentPerson;
                LastName = currentName == null ? PersonName.UnknownLastName : currentName.LastName;
                FirstName = currentName == null ? PersonName.UnknownFirstName : currentName.FirstName;
                MiddleName = currentName == null ? string.Empty : currentName.MiddleName;
                IsMale = currentPerson.IsMale;
                BirthDate = currentPerson.BirthDate;
                Snils = currentPerson.Snils;
                MedNumber = currentPerson.MedNumber;
                Phones = currentPerson.Phones;
                Email = currentPerson.Email;

                changeTracker.IsEnabled = true;
                loadingIsCompleted = true;
            }
            catch (OperationCanceledException)
            {
                //Do nothing. Cancelled operation means that user selected different patient before previous one was loaded
            }
            catch (Exception ex)
            {
                log.ErrorFormatEx(ex, "Failed to load common patient info for patient with Id {0}", patientId);
                CriticalFailureMediator.Activate("Не удалость загрузить данные пациента. Попробуйте еще раз или обратитесь в службу поддержки", reloadPatientDataCommandWrapper, ex);
                loadingIsCompleted = true;
            }
            finally
            {
                CommandManager.InvalidateRequerySuggested();
                if (loadingIsCompleted)
                {
                    saveChangesCommand.RaiseCanExecuteChanged();
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
            changeTracker.IsEnabled = false;
            currentPerson = new Person { Id = SpecialValues.NewId, AmbNumberString = string.Empty };
            LastName = string.Empty;
            FirstName = string.Empty;
            MiddleName = string.Empty;
            BirthDate = DateTime.Today.AddYears(-1);
            Snils = string.Empty;
            MedNumber = string.Empty;
            IsMale = true;
            Phones = string.Empty;
            Email = string.Empty;
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

        #region Inplementation IDataErrorInfo

        private bool saveWasRequested;

        private readonly HashSet<string> invalidProperties = new HashSet<string>();

        private bool IsValid
        {
            get
            {
                saveWasRequested = true;
                OnPropertyChanged(string.Empty);
                return invalidProperties.Count < 1;
            }
        }

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
    }
}