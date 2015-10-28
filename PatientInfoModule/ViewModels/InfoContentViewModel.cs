using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
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
            patientId = SpecialId.NonExisting;
            BusyMediator = new BusyMediator();
            CriticalFailureMediator = new CriticalFailureMediator();
            reloadPatientDataCommandWrapper = new CommandWrapper
                                              {
                                                  Command = new DelegateCommand(() => SelectPatientAsync(patientId)),
                                                  CommandName = "Повторить",
                                              };
            createNewPatientCommand = new DelegateCommand(CreatetNewPatient);
            saveChangesCommand = new DelegateCommand(SaveChanges, CanSaveChanges);
            cancelChangesCommand = new DelegateCommand(CancelChanges, CanCancelChanges);
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

        private int patientId;

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

        public ICommand CreateNewPatientCommand
        {
            get { return createNewPatientCommand; }
        }

        private void CreatetNewPatient()
        {
            eventAggregator.GetEvent<SelectionEvent<Person>>().Publish(SpecialId.New);
        }

        public ICommand SaveChangesCommand
        {
            get { return saveChangesCommand; }
        }

        private void SaveChanges()
        {
            if (!IsValid)
            {
                return;
            }
            MessageBox.Show("Imitating save changes");
        }

        private bool CanSaveChanges()
        {
            if (patientId == SpecialId.NonExisting)
            {
                return false;
            }
            if (patientId == SpecialId.New)
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
            if (patientId == SpecialId.NonExisting || patientId == SpecialId.New)
            {
                return false;
            }
            return changeTracker.HasChanges;
        }

        #endregion

        private readonly CommandWrapper reloadPatientDataCommandWrapper;

        private CancellationTokenSource currentLoadingToken;

        public async void SelectPatientAsync(int patientId)
        {
            ClearData();
            this.patientId = patientId;
            saveChangesCommand.RaiseCanExecuteChanged();
            cancelChangesCommand.RaiseCanExecuteChanged();
            if (patientId == SpecialId.New || patientId == SpecialId.NonExisting)
            {
                return;
            }
            if (currentLoadingToken != null)
            {
                currentLoadingToken.Cancel();
                currentLoadingToken.Dispose();
            }
            var loadingIsCompleted = false;
            currentLoadingToken = new CancellationTokenSource();
            var token = currentLoadingToken.Token;
            BusyMediator.Activate("Загрузка данных пациента...");
            log.InfoFormat("Loading patient info for patient with Id {0}...", patientId);
            IDisposableQueryable<Person> patientQuery = null;
            try
            {
                patientQuery = patientService.GetPatientQuery(patientId);
                var loadPatientTask = patientQuery.Select(x => new
                                                               {
                                                                   ActualName = x.PersonNames.FirstOrDefault(y => y.ChangeNameReason == null), 
                                                                   x.BirthDate,
                                                                   x.Snils,
                                                                   x.MedNumber,
                                                                   x.IsMale,
                                                                   x.Phones,
                                                                   x.Email
                                                               })
                                                  .FirstOrDefaultAsync(token);
                await Task.WhenAll(loadPatientTask, Task.Delay(AppConfiguration.PendingOperationDelay, token));
                var result = loadPatientTask.Result;
                LastName = result.ActualName == null ? PersonName.UnknownLastName : result.ActualName.LastName;
                FirstName = result.ActualName == null ? PersonName.UnknownFirstName : result.ActualName.FirstName;
                MiddleName = result.ActualName == null ? string.Empty : result.ActualName.MiddleName;
                IsMale = result.IsMale;
                BirthDate = result.BirthDate;
                Snils = result.Snils;
                MedNumber = result.MedNumber;
                Phones = result.Phones;
                Email = result.Email;

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
            var targetPatientId = (int?)navigationContext.Parameters[ParameterNames.PatientId] ?? SpecialId.NonExisting;
            if (targetPatientId != patientId)
            {
                SelectPatientAsync(targetPatientId);
            }
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
                                     ?  !NewNameStartDate.HasValue 
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
