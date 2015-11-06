using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Core;
using Prism.Mvvm;
using Core.Wpf.Mvvm;
using PatientRecordsModule.Services;
using PatientRecordsModule.DTO;
using Core.Data.Misc;
using Prism.Regions;
using PatientRecordsModule.Misc;
using System.Threading;
using log4net;
using Core.Data;
using Core.Misc;
using Core.Extensions;
using Core.Wpf.Misc;
using System.Windows.Input;
using Prism.Commands;
using Prism.Events;
using Core.Wpf.Events;
using Prism.Interactivity.InteractionRequest;

namespace PatientRecordsModule.ViewModels
{
    public class PersonRecordListViewModel : BindableBase, IConfirmNavigationRequest
    {
        #region Fields

        private readonly IPatientRecordsService patientRecordsService;
        private readonly IEventAggregator eventAggregator;
        private readonly ILog logService;
        private readonly CommandWrapper reloadPatientVisitsCommandWrapper;
        private readonly ChangeTracker changeTracker;
        private readonly DelegateCommand<int?> createNewVisitCommand;
        private readonly Func<NewVisitCreatingViewModel> newVisitCreatingViewModelFactory;

        private CancellationTokenSource currentLoadingToken;

        #endregion

        #region  Constructors
        public PersonRecordListViewModel(IPatientRecordsService patientRecordsService, ILog logService, IEventAggregator eventAggregator, Func<NewVisitCreatingViewModel> newVisitCreatingViewModelFactory)
        {
            if (patientRecordsService == null)
            {
                throw new ArgumentNullException("patientRecordsService");
            }
            if (logService == null)
            {
                throw new ArgumentNullException("logService");
            }
            if (eventAggregator == null)
            {
                throw new ArgumentNullException("eventAggregator");
            }
            if (newVisitCreatingViewModelFactory == null)
            {
                throw new ArgumentNullException("newVisitCreatingViewModelFactory");
            }
            this.newVisitCreatingViewModelFactory = newVisitCreatingViewModelFactory;
            this.eventAggregator = eventAggregator;
            this.patientRecordsService = patientRecordsService;
            this.logService = logService;
            changeTracker = new ChangeTracker();
            BusyMediator = new BusyMediator();
            CriticalFailureMediator = new CriticalFailureMediator();
            reloadPatientVisitsCommandWrapper = new CommandWrapper
            {
                Command = new DelegateCommand(() => LoadRootItemsAsync(PersonId)),
                CommandName = "Повторить",
            };
            createNewVisitCommand = new DelegateCommand<int?>(CreateNewVisit);
            NewVisitCreatingInteractionRequest = new InteractionRequest<NewVisitCreatingViewModel>();
            RootItems = new ObservableCollectionEx<object>();
            this.PersonId = SpecialValues.NonExistingId;
            SubscribeToEvents();
            //ToDo: If this row exist, all work
            //LoadRootItemsAsync(1);
        }
        #endregion

        #region Properties

        private int personId;
        public int PersonId
        {
            get { return personId; }
            set { SetProperty(ref personId, value); }
        }

        public ObservableCollectionEx<object> RootItems { get; set; }

        private string ambNumber;
        public string AmbNumber
        {
            get { return ambNumber; }
            set { SetProperty(ref ambNumber, value); }
        }

        public BusyMediator BusyMediator { get; set; }

        public CriticalFailureMediator CriticalFailureMediator { get; private set; }

        public InteractionRequest<NewVisitCreatingViewModel> NewVisitCreatingInteractionRequest { get; private set; }
        #endregion

        #region Commands
        public ICommand CreateNewVisitCommand { get { return createNewVisitCommand; } }
        private void CreateNewVisit(int? selectedTemplate)
        {
            var newVisitCreatingViewModel = newVisitCreatingViewModelFactory();
            newVisitCreatingViewModel.IntializeCreation(PersonId, selectedTemplate, null, DateTime.Now, "Создать новый случай");
            NewVisitCreatingInteractionRequest.Raise(newVisitCreatingViewModel, (vm) => {  });
        }
        #endregion

        #region Methods

        public void Dispose()
        {
            UnsubscriveFromEvents();
        }

        private void SubscribeToEvents()
        {
            eventAggregator.GetEvent<SelectionEvent<Person>>().Subscribe(OnPatientSelected);
        }

        private void OnPatientSelected(int personId)
        {
            this.PersonId = personId;
            LoadRootItemsAsync(this.PersonId);
        }

        private void UnsubscriveFromEvents()
        {
            eventAggregator.GetEvent<SelectionEvent<Person>>().Unsubscribe(OnPatientSelected);
        }



        public async void LoadRootItemsAsync(int personId)
        {
            RootItems.Clear();
            this.PersonId = personId;
            if (personId == SpecialValues.NewId || personId == SpecialValues.NonExistingId)
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
            BusyMediator.Activate("Загрузка данных...");
            logService.InfoFormat("Loading patient visits for patient with Id {0}...", personId);
            IDisposableQueryable<Person> patientQuery = null;
            try
            {
                patientQuery = patientRecordsService.GetPersonQuery(personId);
                var task = Task<List<object>>.Factory.StartNew(LoadRootItems);
                await task;
                RootItems.AddRange(task.Result);
                AmbNumber = patientQuery.FirstOrDefault().AmbNumberString;
                changeTracker.IsEnabled = true;
                loadingIsCompleted = true;
            }
            catch (OperationCanceledException)
            {
                //Do nothing. Cancelled operation means that user selected different patient before previous one was loaded
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to load person visits for patient with Id {0}", personId);
                CriticalFailureMediator.Activate("Не удалость загрузить случаи пациента. Попробуйте еще раз или обратитесь в службу поддержки", reloadPatientVisitsCommandWrapper, ex);
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

        private List<object> LoadRootItems()
        {
            List<object> resList = new List<object>();
            var assignmentsViewModels = patientRecordsService.GetPersonRootAssignmentsQuery(PersonId)
                .Select(x => new AssignmentDTO()
                {
                    Id = x.Id,
                    ActualDateTime = x.AssignDateTime,
                    FinancingSourceName = x.FinancingSource.ShortName,
                    RecordTypeName = x.RecordType.Name,
                    RoomName = (x.Room.Number != string.Empty ? x.Room.Number + " - " : string.Empty) + x.Room.Name
                })
                .ToArray()
                .Select(x => new PersonHierarchicalAssignmentsViewModel(x, patientRecordsService, logService));
            var visitsViewModels = patientRecordsService.GetPersonVisitsQuery(PersonId)
                .Select(x => new VisitDTO()
                {
                    Id = x.Id,
                    BeginDateTime = x.BeginDateTime,
                    EndDateTime = x.EndDateTime,
                    ActualDateTime = x.BeginDateTime,
                    FinSource = x.FinancingSource.ShortName,
                    Name = x.VisitTemplate.ShortName,
                    IsCompleted = x.IsCompleted,
                })
                .ToArray()
                .Select(x => new PersonHierarchicalVisitsViewModel(x, patientRecordsService, logService));
            resList.AddRange(assignmentsViewModels);
            resList.AddRange(visitsViewModels);
            return resList.ToList();
        }

        #endregion

        #region IConfirmNavigationRequest implimentation

        public void ConfirmNavigationRequest(NavigationContext navigationContext, Action<bool> continuationCallback)
        {
            throw new NotImplementedException();
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            throw new NotImplementedException();
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            throw new NotImplementedException();
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            var targetPatientId = (int?)navigationContext.Parameters[ParameterNames.PersonId] ?? SpecialValues.NonExistingId;
            if (targetPatientId != personId)
            {
                LoadRootItemsAsync(targetPatientId);
            }
        }

        #endregion
    }
}
