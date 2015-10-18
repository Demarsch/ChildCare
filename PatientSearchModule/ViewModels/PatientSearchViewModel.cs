using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Core.Data;
using Core.Extensions;
using Core.Services;
using Core.Wpf.Mvvm;
using log4net;
using PatientSearchModule.Misc;
using PatientSearchModule.Services;
using Prism.Commands;
using Prism.Mvvm;

namespace PatientSearchModule.ViewModels
{
    public class PatientSearchViewModel : BindableBase
    {
        private readonly IPatientSearchService patientSearchService;

        private readonly ILog log;

        private readonly ICacheService cacheService;

        public PatientSearchViewModel(IPatientSearchService patientSearchService, ILog log, ICacheService cacheService)
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
            this.cacheService = cacheService;
            this.log = log;
            this.patientSearchService = patientSearchService;
            Header = "Поиск Пациента";
            BusyMediator = new BusyMediator();
            CriticalFailureMediator = new CriticalFailureMediator();
            Patients = new ObservableCollectionEx<FoundPatientViewModel>();
            SearchPatientsCommand = new DelegateCommand(SearchPatients);
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
                    SearchPatients();
                }
            }
        }

        private object header;

        public object Header
        {
            get { return header; }
            set { SetProperty(ref header, value); }
        }

        public BusyMediator BusyMediator { get; private set; }

        public CriticalFailureMediator CriticalFailureMediator { get; private set; }

        public ICommand SearchPatientsCommand { get; private set; }

        private async void SearchPatients()
        {
            Patients.Clear();
            var currentSearchText = SearchText;
            if (string.IsNullOrWhiteSpace(currentSearchText))
            {
                return;
            }
            var searchIsCompleted = false;
            CriticalFailureMediator.Deactivate();
            BusyMediator.Activate("Идет поиск пациентов...");
            log.InfoFormat("Searching patients by user input \"{0}\"", currentSearchText);
            PatientSearchQuery query = null;
            try
            {
                query = patientSearchService.GetPatientSearchQuery(currentSearchText);
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
                    .ToArrayAsync();
                await Task.WhenAll(runQueryTask, Task.Delay(TimeSpan.FromSeconds(0.5)));
                var result = await runQueryTask.Result
                                               .Select(x => new FoundPatientViewModel
                                                            {
                                                                BirthDate = x.BirthDate,
                                                                CurrentName = x.CurrentName,
                                                                PreviousName = x.PreviousName,
                                                                Gender = cacheService.GetItemById<Gender>(x.GenderId),
                                                                IdentityDocument = cacheService.AutoWire(x.IdentityDocument, y => y.IdentityDocumentType),
                                                                MedNumber = x.MedNumber,
                                                                Snils = x.Snils
                                                            })
                                               .ToArrayAsync();
                searchIsCompleted = currentSearchText == SearchText;
                if (searchIsCompleted)
                {
                    log.InfoFormat("Found {0} patients", result.Length);
                    foreach (var patient in result)
                    {
                        patient.WordsToHighlight = query.ParsedTokens;
                    }
                    Patients.Replace(result);
                }
            }
            catch (Exception ex)
            {
                log.Error("Failed to find patients", ex);
                searchIsCompleted = currentSearchText == SearchText;
                CriticalFailureMediator.Activate("Не удалось загрузить пациентов. Попробуйте еще раз или перезапустите приложение", SearchPatientsCommand);
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