using System;
using System.Data.Entity;
using System.Linq;
using System.Windows.Input;
using Core.Data;
using Core.Misc;
using Core.Wpf.Mvvm;
using log4net;
using PatientSearchModule.Services;
using Prism.Commands;
using Prism.Mvvm;

namespace PatientSearchModule.ViewModels
{
    public class PatientSearchViewModel : BindableBase
    {
        private readonly IPatientSearchService patientSearchService;

        private readonly ILog log;

        public PatientSearchViewModel(IPatientSearchService patientSearchService, ILog log)
        {
            if (patientSearchService == null)
            {
                throw new ArgumentNullException("patientSearchService");
            }
            if (log == null)
            {
                throw new ArgumentNullException("log");
            }
            this.log = log;
            this.patientSearchService = patientSearchService;
            Header = "Поиск Пациента";
            BusyMediator = new BusyMediator();
            CriticalFailureMediator = new CriticalFailureMediator();
            Patients = new ObservableCollectionEx<string>();
            SearchPatientsCommand = new DelegateCommand(SearchPatients);
        }

        public ObservableCollectionEx<string> Patients { get; private set; }

        private string searchText;

        public string SearchText
        {
            get { return searchText; }
            set { SetProperty(ref searchText, value); }
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
            var currentSearchText = SearchText;
            var searchIsCompleted = false;
            CriticalFailureMediator.Deactivate();
            BusyMediator.Activate("Идет поиск пациентов...");
            log.InfoFormat("Searching patient by user input \"{0}\"", currentSearchText);
            IDisposableQueryable<Person> query = null;
            try
            {
                query = patientSearchService.PatientSearchQuery(currentSearchText);
                var result = await query.Select(x => x.FullName).ToArrayAsync();
                searchIsCompleted = currentSearchText == SearchText;
                if (searchIsCompleted)
                {
                    log.InfoFormat("Found {0} patients", result.Length);
                    Patients.Replace(result);
                }
            }
            catch (Exception ex)
            {
                log.Error("Failed to find patients", ex);
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
