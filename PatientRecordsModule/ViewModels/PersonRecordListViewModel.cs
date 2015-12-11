using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Core;
using Prism.Mvvm;
using Core.Wpf.Mvvm;
using Shared.PatientRecords.Services;
using Shared.PatientRecords.DTO;
using Core.Data.Misc;
using Prism.Regions;
using Shared.PatientRecords.Misc;
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
using System.Data.Entity;
using Core.Wpf.Services;
using Core.Services;
using Core.Data.Services;

namespace Shared.PatientRecords.ViewModels
{
    public class PersonRecordListViewModel : BindableBase, IDisposable
    {
        #region Fields

        private readonly IPatientRecordsService patientRecordsService;
        private readonly IEventAggregator eventAggregator;
        private readonly IDialogServiceAsync dialogService;
        private readonly ILog logService;
        private readonly IUserService userService;
        private readonly CommandWrapper reloadPatientVisitsCommandWrapper;
        private readonly CommandWrapper addNewVisitInListVisitCommandWrapper;
        //private readonly ChangeTrackerEx<PersonRecordListViewModel> changeTracker;
        

        private CancellationTokenSource currentOperationToken;
        private int? visitId;
        private int? recordId;
        #endregion

        #region  Constructors
        public PersonRecordListViewModel(IPatientRecordsService patientRecordsService, ILog logService, IDialogServiceAsync dialogService, IUserService userService, IEventAggregator eventAggregator)
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
            if (dialogService == null)
            {
                throw new ArgumentNullException("dialogService");
            }
            if (userService == null)
            {
                throw new ArgumentNullException("userService");
            }
            this.dialogService = dialogService;
            this.userService = userService;
            this.eventAggregator = eventAggregator;
            this.patientRecordsService = patientRecordsService;
            this.logService = logService;
            //changeTracker = new ChangeTrackerEx<PersonRecordListViewModel>(this);
            BusyMediator = new BusyMediator();
            FailureMediator = new FailureMediator();
            reloadPatientVisitsCommandWrapper = new CommandWrapper
            {
                Command = new DelegateCommand(() => LoadRootItemsAsync(PersonId)),
                CommandName = "Повторить",
            };
            addNewVisitInListVisitCommandWrapper = new CommandWrapper { Command = new DelegateCommand(() => AddNewVisitToList(visitId)) };
            RootItems = new ObservableCollectionEx<object>();
            this.PersonId = SpecialValues.NonExistingId;
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

        private object selectedItem;
        public object SelectedItem
        {
            get { return selectedItem; }
            set { SetProperty(ref selectedItem, value); }
        }

        public BusyMediator BusyMediator { get; set; }

        public FailureMediator FailureMediator { get; private set; }
        #endregion

        #region Commands


        private void AddAssignmentToPatientRecords(int assignmentId, int? visitId)
        {

        }

        #endregion

        #region Methods

        private async void AddNewVisitToList(int? visitId)
        {
            FailureMediator.Deactivate();
            this.visitId = visitId;
            if (currentOperationToken != null)
            {
                currentOperationToken.Cancel();
                currentOperationToken.Dispose();
            }
            currentOperationToken = new CancellationTokenSource();
            var token = currentOperationToken.Token;
            logService.InfoFormat("Additing new visit in records list with Id = {0} for person with Id = {1}", visitId, personId);
            BusyMediator.Activate("Добавление нового случая в список пациенту...");
            var saveSuccesfull = false;
            var visitQuery = patientRecordsService.GetVisit(visitId.Value);
            try
            {
                var visitDTO = await visitQuery.Select(x => new VisitDTO()
                {
                    Id = x.Id,
                    BeginDateTime = x.BeginDateTime,
                    EndDateTime = x.EndDateTime,
                    ActualDateTime = x.BeginDateTime,
                    FinSource = x.FinancingSource.Name,
                    Name = x.VisitTemplate.ShortName,
                    IsCompleted = x.IsCompleted
                }).FirstOrDefaultAsync(token);
                saveSuccesfull = true;
                this.visitId = 0;
                RootItems.Add(new PersonHierarchicalVisitsViewModel(visitDTO, patientRecordsService, eventAggregator, logService));
            }
            catch (OperationCanceledException)
            {
                //Nothing to do as it means that we somehow cancelled save operation
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to add new visit in records list with Id = {0} for person with Id = {1}", visitId, personId);
                FailureMediator.Activate("Не удалось добавить новый случай в список пациенту. Попробуйте еще раз или обратитесь в службу поддержки", addNewVisitInListVisitCommandWrapper, ex);
            }
            finally
            {
                BusyMediator.Deactivate();
                if (saveSuccesfull)
                {

                }
            }
        }

        public void Dispose()
        {
            addNewVisitInListVisitCommandWrapper.Dispose();
            reloadPatientVisitsCommandWrapper.Dispose();
        }

        public async void LoadRootItemsAsync(int personId)
        {
            RootItems.Clear();
            this.PersonId = personId;
            if (personId == SpecialValues.NewId || personId == SpecialValues.NonExistingId)
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
                //changeTracker.IsEnabled = true;
                loadingIsCompleted = true;
            }
            catch (OperationCanceledException)
            {
                //Do nothing. Cancelled operation means that user selected different patient before previous one was loaded
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to load person visits for patient with Id {0}", personId);
                FailureMediator.Activate("Не удалость загрузить случаи пациента. Попробуйте еще раз или обратитесь в службу поддержки", reloadPatientVisitsCommandWrapper, ex);
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
                    FinancingSourceName = x.FinancingSource.Name,
                    RecordTypeName = x.RecordType.Name,
                    RoomName = (x.Room.Number != string.Empty ? x.Room.Number + " - " : string.Empty) + x.Room.Name
                })
                .ToArray()
                .Select(x => new PersonHierarchicalAssignmentsViewModel(x, patientRecordsService, eventAggregator, logService));
            var visitsViewModels = patientRecordsService.GetPersonVisitsQuery(PersonId, false)
                .Select(x => new VisitDTO()
                {
                    Id = x.Id,
                    BeginDateTime = x.BeginDateTime,
                    EndDateTime = x.EndDateTime,
                    ActualDateTime = x.BeginDateTime,
                    FinSource = x.FinancingSource.Name,
                    Name = x.VisitTemplate.Name,
                    IsCompleted = x.IsCompleted,
                })
                .ToArray()
                .Select(x => new PersonHierarchicalVisitsViewModel(x, patientRecordsService, eventAggregator, logService));
            resList.AddRange(assignmentsViewModels);
            resList.AddRange(visitsViewModels);
            return resList.ToList();
        }

        #endregion
    }
}
