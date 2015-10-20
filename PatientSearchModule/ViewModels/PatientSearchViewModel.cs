using System;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Core.Data;
using Core.Extensions;
using Core.Misc;
using Core.Services;
using Core.Wpf.Events;
using Core.Wpf.Misc;
using Core.Wpf.Mvvm;
using log4net;
using PatientSearchModule.Misc;
using PatientSearchModule.Model;
using PatientSearchModule.Services;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace PatientSearchModule.ViewModels
{
    public class PatientSearchViewModel : BindableBase
    {
        private readonly IPatientSearchService patientSearchService;

        private readonly ILog log;

        private readonly ICacheService cacheService;

        private readonly IUserInputNormalizer userInputNormalizer;

        private readonly IEventAggregator eventAggregator;

        public PatientSearchViewModel(IPatientSearchService patientSearchService,
                                      ILog log,
                                      ICacheService cacheService,
                                      IUserInputNormalizer userInputNormalizer,
                                      IEventAggregator eventAggregator)
        {
            if (patientSearchService == null)
            {
                throw new ArgumentNullException("patientSearchService");
            }
            if (log == null)
            {
                throw new ArgumentNullException("log");
            }
            if (cacheService == null)
            {
                throw new ArgumentNullException("cacheService");
            }
            if (userInputNormalizer == null)
            {
                throw new ArgumentNullException("userInputNormalizer");
            }
            if (eventAggregator == null)
            {
                throw new ArgumentNullException("eventAggregator");
            }
            this.userInputNormalizer = userInputNormalizer;
            this.eventAggregator = eventAggregator;
            this.cacheService = cacheService;
            this.userInputNormalizer = userInputNormalizer;
            this.log = log;
            this.patientSearchService = patientSearchService;
            Header = "Поиск Пациента";
            BusyMediator = new BusyMediator();
            CriticalFailureMediator = new CriticalFailureMediator();
            Patients = new ObservableCollectionEx<FoundPatientViewModel>();
            SearchPatientsCommand = new DelegateCommand<bool?>(SearchPatients);
            searchPatientsCommandWrapper = new CommandWrapper
                                           {
                                               Command = SearchPatientsCommand,
                                               CommandName = "Повторный поиск",
                                               CommandParameter = false
                                           };
        }

        public ObservableCollectionEx<FoundPatientViewModel> Patients { get; private set; }

        private string searchText;

        public string SearchText
        {
            get { return searchText; }
            set
            {
                if (SetProperty(ref searchText, value))
                {
                    SearchPatients(true);
                }
            }
        }

        private CancellationTokenSource currentSearchToken;

        private object header;

        public object Header
        {
            get { return header; }
            set { SetProperty(ref header, value); }
        }

        public BusyMediator BusyMediator { get; private set; }

        public CriticalFailureMediator CriticalFailureMediator { get; private set; }

        private readonly CommandWrapper searchPatientsCommandWrapper;

        public ICommand SearchPatientsCommand { get; private set; }

        private FoundPatientViewModel selectedPatient;

        public FoundPatientViewModel SelectedPatient
        {
            get { return selectedPatient; }
            set
            {
                SetProperty(ref selectedPatient, value);
                if (value != null)
                {
                    eventAggregator.GetEvent<SelectionEvent<Person>>().Publish(value.Id);
                }
            }
        }

        private void SearchPatients(bool? useDelay)
        {
            if (currentSearchToken != null)
            {
                currentSearchToken.Cancel();
                currentSearchToken.Dispose();
            }
            currentSearchToken = new CancellationTokenSource();
            var userInput = userInputNormalizer.NormalizeUserInput(searchText ?? string.Empty);
            if (userInput.Length < AppConfiguration.UserInputSearchThreshold)
            {
                BusyMediator.Deactivate();
                Patients.Clear();
                return;
            }
            BusyMediator.Activate("Идет поиск пациентов...");
            if (useDelay == true)
            {
                Task.Delay(AppConfiguration.UserInputDelay, currentSearchToken.Token)
                    .ContinueWith(x => SearchPatientsAsync(userInput, currentSearchToken.Token),
                                  currentSearchToken.Token,
                                  TaskContinuationOptions.OnlyOnRanToCompletion,
                                  TaskScheduler.FromCurrentSynchronizationContext());
            }
            else
            {
                SearchPatientsAsync(userInput, currentSearchToken.Token);
            }
        }

        private async void SearchPatientsAsync(string userInput, CancellationToken token)
        {
            var searchIsCompleted = false;
            CriticalFailureMediator.Deactivate();
            log.InfoFormat("Searching patients by user input \"{0}\"", userInput);
            PatientSearchQuery query = null;
            try
            {
                query = patientSearchService.GetPatientSearchQuery(userInput);
                var runQueryTask = query
                    .PatientsQuery
                    .Select(x => new
                                 {
                                     x.Id,
                                     x.BirthDate,
                                     x.GenderId,
                                     x.Snils,
                                     x.MedNumber,
                                     CurrentName = x.PersonNames.FirstOrDefault(y => y.ChangeNameReason == null),
                                     PreviousName = x.PersonNames.Where(y => y.ChangeNameReason != null)
                                                     .OrderByDescending(y => y.BeginDateTime)
                                                     .FirstOrDefault(),
                                     IdentityDocument = x.PersonIdentityDocuments
                                                         .OrderByDescending(y => y.BeginDate)
                                                         .FirstOrDefault()
                                 })
                    .ToArrayAsync(token);
                await Task.WhenAll(runQueryTask, Task.Delay(TimeSpan.FromSeconds(0.5), token));
                if (runQueryTask.IsCanceled)
                {
                    return;
                }
                var result = await runQueryTask.Result
                                               .Select(x => new FoundPatientViewModel
                                                            {
                                                                Id = x.Id,
                                                                BirthDate = x.BirthDate,
                                                                CurrentName = x.CurrentName,
                                                                PreviousName = x.PreviousName,
                                                                Gender = cacheService.GetItemById<Gender>(x.GenderId),
                                                                IdentityDocument = cacheService.AutoWire(x.IdentityDocument, y => y.IdentityDocumentType),
                                                                MedNumber = x.MedNumber,
                                                                Snils = Person.DelimitizeSnils(x.Snils)
                                                            })
                                               .ToArrayAsync(token);
                log.InfoFormat("Found {0} patients", result.Length);
                foreach (var patient in result)
                {
                    patient.WordsToHighlight = query.ParsedTokens;
                }
                Patients.Replace(result);
                searchIsCompleted = true;
            }
            catch (OperationCanceledException)
            {
                searchIsCompleted = false;
            }
            catch (Exception ex)
            {
                log.Error("Failed to find patients", ex);
                searchIsCompleted = true;
                CriticalFailureMediator.Activate("Не удалось загрузить пациентов. Попробуйте еще раз или перезапустите приложение", searchPatientsCommandWrapper, ex);
            }
            finally
            {
                if (query != null)
                {
                    query.Dispose();
                }
                if (searchIsCompleted)
                {
                    BusyMediator.Deactivate();
                }
            }
        }
    }
}