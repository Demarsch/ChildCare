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
        private readonly CommandWrapper addNewAssignmentInListVisitCommandWrapper;
        private readonly CommandWrapper addNewRecordInListVisitCommandWrapper;
        //private readonly ChangeTrackerEx<PersonRecordListViewModel> changeTracker;


        private CancellationTokenSource currentOperationToken;
        private int visitId;
        private int recordId;
        private int assignmentId;
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
            addNewAssignmentInListVisitCommandWrapper = new CommandWrapper { Command = new DelegateCommand(() => AddNewAssignmentToList(assignmentId)) };
            addNewRecordInListVisitCommandWrapper = new CommandWrapper { Command = new DelegateCommand(() => AddNewRecordToList(recordId)) };
            RootItems = new ObservableCollectionEx<IHierarchicalItem>();
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

        public ObservableCollectionEx<IHierarchicalItem> RootItems { get; set; }

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

        public async void AddNewAssignmentToList(int? assignmentId)
        {
            FailureMediator.Deactivate();
            this.assignmentId = assignmentId.ToInt();
            if (currentOperationToken != null)
            {
                currentOperationToken.Cancel();
                currentOperationToken.Dispose();
            }
            currentOperationToken = new CancellationTokenSource();
            var token = currentOperationToken.Token;
            logService.InfoFormat("Additing new assignment in records list with Id = {0} for person with Id = {1}", this.assignmentId, personId);
            BusyMediator.Activate("Добавление нового назначения в список пациенту...");
            var saveSuccesfull = false;
            var assignmentQuery = patientRecordsService.GetAssignment(this.assignmentId);
            try
            {
                var assignDateTime = await assignmentQuery.Select(x => x.AssignDateTime).FirstOrDefaultAsync();
                var listToParent = await patientRecordsService.GetParentItems(new PersonRecItem() { Id = this.assignmentId, Type = ItemType.Assignment, ActualDatetime = assignDateTime });
                if (listToParent.Count > 0)
                    InsertItemInTree(listToParent.ToList(), RootItems);
                this.assignmentId = 0;
            }
            catch (OperationCanceledException)
            {
                //Nothing to do as it means that we somehow cancelled save operation
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to add new assignment in records list with Id = {0} for person with Id = {1}", this.assignmentId, personId);
                FailureMediator.Activate("Не удалось добавить новое назначение в список пациенту. Попробуйте еще раз или обратитесь в службу поддержки", addNewAssignmentInListVisitCommandWrapper, ex);
            }
            finally
            {
                if (assignmentQuery != null)
                {
                    assignmentQuery.Dispose();
                }
                BusyMediator.Deactivate();
                if (saveSuccesfull)
                {

                }
            }
        }

        public async void AddNewVisitToList(int? visitId)
        {
            FailureMediator.Deactivate();
            this.visitId = visitId.ToInt();
            if (currentOperationToken != null)
            {
                currentOperationToken.Cancel();
                currentOperationToken.Dispose();
            }
            currentOperationToken = new CancellationTokenSource();
            var token = currentOperationToken.Token;
            logService.InfoFormat("Additing new visit in records list with Id = {0} for person with Id = {1}", this.visitId, personId);
            BusyMediator.Activate("Добавление нового случая в список пациенту...");
            var saveSuccesfull = false;
            var visitQuery = patientRecordsService.GetVisit(this.visitId);
            try
            {
                var beginDateTime = await visitQuery.Select(x => x.BeginDateTime).FirstOrDefaultAsync();
                var listToParent = await patientRecordsService.GetParentItems(new PersonRecItem() { Id = this.visitId, Type = ItemType.Visit, ActualDatetime = beginDateTime });
                if (listToParent.Count > 0)
                    InsertItemInTree(listToParent.ToList(), RootItems);
                this.visitId = 0;
            }
            catch (OperationCanceledException)
            {
                //Nothing to do as it means that we somehow cancelled save operation
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to add new visit in records list with Id = {0} for person with Id = {1}", this.visitId, personId);
                FailureMediator.Activate("Не удалось добавить новый случай в список пациенту. Попробуйте еще раз или обратитесь в службу поддержки", addNewVisitInListVisitCommandWrapper, ex);
            }
            finally
            {
                if (visitQuery != null)
                {
                    visitQuery.Dispose();
                }
                BusyMediator.Deactivate();
                if (saveSuccesfull)
                {

                }
            }
        }

        public async void AddNewRecordToList(int? recordId)
        {
            FailureMediator.Deactivate();
            this.recordId = recordId.ToInt();
            if (currentOperationToken != null)
            {
                currentOperationToken.Cancel();
                currentOperationToken.Dispose();
            }
            currentOperationToken = new CancellationTokenSource();
            var token = currentOperationToken.Token;
            logService.InfoFormat("Additing new record in records list with Id = {0} for person with Id = {1}", this.recordId, personId);
            BusyMediator.Activate("Добавление новой услуги в список пациенту...");
            var saveSuccesfull = false;
            var recordQuery = patientRecordsService.GetRecord(this.recordId);
            try
            {
                var actualDateTime = await recordQuery.Select(x => x.ActualDateTime).FirstOrDefaultAsync();
                var listToParent = await patientRecordsService.GetParentItems(new PersonRecItem() { Id = this.recordId, Type = ItemType.Record, ActualDatetime = actualDateTime });
                if (listToParent.Count > 0)
                    InsertItemInTree(listToParent.ToList(), RootItems);
                this.recordId = 0;
            }
            catch (OperationCanceledException)
            {
                //Nothing to do as it means that we somehow cancelled save operation
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to add new record in records list with Id = {0} for person with Id = {1}", this.recordId, personId);
                FailureMediator.Activate("Не удалось добавить новую услугу в список пациенту. Попробуйте еще раз или обратитесь в службу поддержки", addNewRecordInListVisitCommandWrapper, ex);
            }
            finally
            {
                if (recordQuery != null)
                {
                    recordQuery.Dispose();
                }
                BusyMediator.Deactivate();
                if (saveSuccesfull)
                {

                }
            }
        }


        private async void InsertItemInTree(List<PersonRecItem> listToParent, ObservableCollectionEx<IHierarchicalItem> curLevelItems, int curParentIndex = 0)
        {
            IHierarchicalItem curTreeItem = null;
            PersonRecItem curItem = listToParent[curParentIndex];
            int indexToAdd = 0;
            var isExpanded = false;
            for (int i = 0; i < curLevelItems.Count; i++)
            {
                curTreeItem = curLevelItems[i];
                if (curTreeItem.ActualDateTime >= curItem.ActualDatetime && indexToAdd == 0)
                    indexToAdd = i;
                if (curTreeItem.Item.Equals(curItem))
                {
                    curTreeItem.IsExpanded = true;
                    isExpanded = true;
                    InsertItemInTree(listToParent, curTreeItem.Childs, ++curParentIndex);
                    return;
                }
            }
            if (!isExpanded)
            {
                var itemToAdd = await GetHierarchicalItem(curItem);
                if (itemToAdd == null)
                {
                    throw new Exception(String.Format("Can't create a hierarchicalItem for item with type={0} and Id={1}", curItem.Type, curItem.Id));
                }
                else
                {
                    curLevelItems.Insert(indexToAdd, itemToAdd);
                    if (listToParent.Count - 1 == curParentIndex)
                    {
                        itemToAdd.IsSelected = true;
                    }
                    else
                    {
                        isExpanded = true;
                    }
                }
            }
        }

        private async Task<IHierarchicalItem> GetHierarchicalItem(PersonRecItem item)
        {
            logService.InfoFormat("Creating HierarchicalItem for item with type={0} and Id={1}", item.Type, item.Id);
            IHierarchicalItem hierarchicalItem = null;
            if (currentOperationToken != null)
            {
                currentOperationToken.Cancel();
                currentOperationToken.Dispose();
            }
            currentOperationToken = new CancellationTokenSource();
            var token = currentOperationToken.Token;
            IDisposableQueryable<Record> recordQuery = null;
            IDisposableQueryable<Visit> visitQuery = null;
            IDisposableQueryable<Assignment> assignmentQuery = null;
            try
            {
                switch (item.Type)
                {
                    case ItemType.Visit:
                        visitQuery = patientRecordsService.GetVisit(item.Id);
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

                        hierarchicalItem = new PersonHierarchicalVisitsViewModel(visitDTO, patientRecordsService, eventAggregator, logService);
                        break;
                    case ItemType.Record:
                        recordQuery = patientRecordsService.GetRecord(item.Id);
                        var recordDTO = await recordQuery.Select(x => new RecordDTO()
                        {
                            Id = x.Id,
                            ActualDateTime = x.ActualDateTime,
                            RecordTypeName = x.RecordType.Name,
                            BeginDateTime = x.BeginDateTime,
                            EndDateTime = x.EndDateTime,
                            IsCompleted = x.IsCompleted,
                            FinSourceName = x.Visit != null ? x.Visit.FinancingSource.ShortName : string.Empty,
                            RoomName = (x.Room.Number != string.Empty ? x.Room.Number + " - " : string.Empty) + x.Room.Name,
                        }).FirstOrDefaultAsync(token);

                        hierarchicalItem = new PersonHierarchicalRecordsViewModel(recordDTO, patientRecordsService, eventAggregator, logService);
                        break;
                    case ItemType.Assignment:
                        assignmentQuery = patientRecordsService.GetAssignment(item.Id);
                        var assignmentDTO = await assignmentQuery.Select(x => new AssignmentDTO()
                        {
                            Id = x.Id,
                            ActualDateTime = x.AssignDateTime,
                            FinancingSourceName = x.FinancingSource.Name,
                            RecordTypeName = x.RecordType.ShortName != string.Empty ? x.RecordType.ShortName : x.RecordType.Name,
                            RoomName = (x.Room.Number != string.Empty ? x.Room.Number + " - " : string.Empty) + x.Room.Name,
                        }).FirstOrDefaultAsync(token);

                        hierarchicalItem = new PersonHierarchicalAssignmentsViewModel(assignmentDTO, patientRecordsService, eventAggregator, logService);
                        break;
                    default:
                        break;
                }

            }
            catch (OperationCanceledException)
            {
                //Nothing to do as it means that we somehow cancelled save operation
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to add new visit in records list with Id = {0} for person with Id = {1}", recordId, personId);
            }
            finally
            {
                BusyMediator.Deactivate();
                if (recordQuery != null)
                {
                    recordQuery.Dispose();
                }
                if (visitQuery != null)
                {
                    visitQuery.Dispose();
                }
                if (assignmentQuery != null)
                {
                    assignmentQuery.Dispose();
                }
            }
            return hierarchicalItem;
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
                var task = Task<List<IHierarchicalItem>>.Factory.StartNew(LoadRootItems);
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

        private List<IHierarchicalItem> LoadRootItems()
        {
            List<IHierarchicalItem> resList = new List<IHierarchicalItem>();
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
            return resList.OrderBy(x => x.ActualDateTime).ToList();
        }

        #endregion

        public CommandWrapper addNewItemInTreeCommandWrapper { get; set; }
    }
}
