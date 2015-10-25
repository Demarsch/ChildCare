using System;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Data;
using Core.Data.Misc;
using Core.Extensions;
using Core.Misc;
using Core.Services;
using Core.Wpf.Misc;
using Core.Wpf.Mvvm;
using log4net;
using PatientInfoModule.Misc;
using PatientInfoModule.Services;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;

namespace PatientInfoModule.ViewModels
{
    public class InfoContentViewModel : BindableBase, IConfirmNavigationRequest
    {
        private readonly IPatientService patientService;

        private readonly ILog log;

        private readonly ICacheService cacheService;

        public InfoContentViewModel(IPatientService patientService, ILog log, ICacheService cacheService)
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
            this.patientService = patientService;
            this.log = log;
            this.cacheService = cacheService;
            BusyMediator = new BusyMediator();
            CriticalFailureMediator = new CriticalFailureMediator();
            reloadPatientDataCommandWrapper = new CommandWrapper
                                              {
                                                  Command = new DelegateCommand(() => SelectPatientAsync(patientId)),
                                                  CommandName = "Повторить",
                                              };
        }

        public BusyMediator BusyMediator { get; set; }

        public CriticalFailureMediator CriticalFailureMediator { get; private set; }

        private int patientId;

        private string lastName;

        public string LastName
        {
            get { return lastName; }
            set
            {
                SetProperty(ref lastName, value.Trim());
                UpdateNameIsChanged();
            }
        }

        private string firstName;

        public string FirstName
        {
            get { return firstName; }
            set
            {
                SetProperty(ref firstName, value.Trim());
                UpdateNameIsChanged();
            }
        }

        private string middleName;

        public string MiddleName
        {
            get { return middleName; }
            set
            {
                SetProperty(ref middleName, value.Trim());
                UpdateNameIsChanged();
            }
        }

        private DateTime? birthDate;

        public DateTime? BirthDate
        {
            get { return birthDate; }
            set { SetProperty(ref birthDate, value); }
        }

        private Gender selectedGender;

        public Gender SelectedGender
        {
            get { return selectedGender; }
            set { SetProperty(ref selectedGender, value); }
        }

        private string snils;

        public string Snils
        {
            get { return snils; }
            set { SetProperty(ref snils, value); }
        }

        private string medNumber;

        public string MedNumber
        {
            get { return medNumber; }
            set { SetProperty(ref medNumber, value); }
        }

        private bool isMale;

        public bool IsMale
        {
            get { return isMale; }
            set { SetProperty(ref isMale, value); }
        }

        private string phones;

        public string Phones
        {
            get { return phones; }
            set { SetProperty(ref phones, value); }
        }

        private string email;

        public string Email
        {
            get { return email; }
            set { SetProperty(ref email, value); }
        }

        #region Name changing

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

        private bool isTrackingNameChanges;

        private string originalLastName;

        private string originalFirstName;

        private string originalMiddleName;

        private void StartTrackingNameChanges()
        {
            originalLastName = lastName;
            originalFirstName = firstName;
            originalMiddleName = middleName;
            isTrackingNameChanges = true;
        }

        private void StopTrackingNameChanges()
        {
            originalLastName = null;
            originalFirstName = null;
            originalMiddleName = null;
            isTrackingNameChanges = false;
        }

        private void UpdateNameIsChanged()
        {
            if (isTrackingNameChanges)
            {
                IsNameChanged = string.Compare(originalLastName, lastName, StringComparison.CurrentCultureIgnoreCase) != 0
                                || string.Compare(originalFirstName, firstName, StringComparison.CurrentCultureIgnoreCase) != 0
                                || string.Compare(originalMiddleName, middleName, StringComparison.CurrentCultureIgnoreCase) != 0;
            }
            else
            {
                IsNameChanged = false;
            }
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

        private readonly CommandWrapper reloadPatientDataCommandWrapper;

        private CancellationTokenSource currentLoadingToken;

        public async void SelectPatientAsync(int patientId)
        {
            ClearData();
            this.patientId = patientId;
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

                StartTrackingNameChanges();
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
            StopTrackingNameChanges();
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
            var patientId = (int?)navigationContext.Parameters[ParameterNames.PatientId] ?? SpecialId.NonExisting;
            SelectPatientAsync(patientId);
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            throw new NotImplementedException();
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            throw new NotImplementedException();
        }

        public void ConfirmNavigationRequest(NavigationContext navigationContext, Action<bool> continuationCallback)
        {
            throw new NotImplementedException();
        }
    }
}
