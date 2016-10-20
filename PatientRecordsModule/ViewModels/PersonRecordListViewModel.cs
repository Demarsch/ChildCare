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
using Shared.PatientRecords.ViewModels.PersonHierarchicalItemViewModels;
using Shell.Shared;

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
        private readonly IHierarchicalRepository hierarchicalItemViewModelRepository;
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
        public PersonRecordListViewModel(
            IPatientRecordsService patientRecordsService, ILog logService, IDialogServiceAsync dialogService, IUserService userService, IEventAggregator eventAggregator, 
            IHierarchicalRepository childItemViewModelRepository)
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
            if (childItemViewModelRepository == null)
            {
                throw new ArgumentNullException("childItemViewModelRepository");
            }
            this.hierarchicalItemViewModelRepository = childItemViewModelRepository;
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

        private void InsertItemInTree(List<PersonRecItem> listToParent, ObservableCollectionEx<IHierarchicalItem> curLevelItems, int curParentIndex = 0)
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
                    if (listToParent.Count - 1 == curParentIndex)
                    {
                        curTreeItem.IsSelected = true;
                    }
                    else
                    {
                        curTreeItem.IsExpanded = true;
                        InsertItemInTree(listToParent, curTreeItem.Childs, ++curParentIndex);
                    }
                    isExpanded = true;
                    return;
                }
            }
            if (!isExpanded)
            {
                var itemToAdd = hierarchicalItemViewModelRepository.GetHierarchicalItem(curItem);
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
                        itemToAdd.IsExpanded = true;
                        InsertItemInTree(listToParent, itemToAdd.Childs, ++curParentIndex);
                    }
                }
            }
        }

        public async void DeleteAssignmentFromList(int? assignmentId)
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
            logService.InfoFormat("Deleting assignment from records list with Id = {0} for person with Id = {1}", this.assignmentId, personId);
            BusyMediator.Activate("Удаление назначения из списока пациента...");
            var saveSuccesfull = false;
            var assignmentQuery = patientRecordsService.GetAssignment(this.assignmentId);
            try
            {
                var assignDateTime = await assignmentQuery.Select(x => x.AssignDateTime).FirstOrDefaultAsync();
                var listToParent = await patientRecordsService.GetParentItems(new PersonRecItem() { Id = this.assignmentId, Type = ItemType.Assignment, ActualDatetime = assignDateTime });
                if (listToParent.Count > 0)
                    DeleteItemFromTree(listToParent.ToList(), RootItems);
                this.assignmentId = 0;
            }
            catch (OperationCanceledException)
            {
                //Nothing to do as it means that we somehow cancelled save operation
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to delete assignment from records list with Id = {0} for person with Id = {1}", this.assignmentId, personId);
                FailureMediator.Activate("Не удалось удалить новое назначение из список пациенту. Попробуйте еще раз или обратитесь в службу поддержки", exception: ex, canBeDeactivated: true);
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

        public async void DeleteVisitFromList(int? visitId)
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
            logService.InfoFormat("Delete visit from records list with Id = {0} for person with Id = {1}", this.visitId, personId);
            BusyMediator.Activate("Удаление случая из списока пациента...");
            var saveSuccesfull = false;
            var visitQuery = patientRecordsService.GetVisit(this.visitId);
            try
            {
                var beginDateTime = await visitQuery.Select(x => x.BeginDateTime).FirstOrDefaultAsync();
                var listToParent = await patientRecordsService.GetParentItems(new PersonRecItem() { Id = this.visitId, Type = ItemType.Visit, ActualDatetime = beginDateTime });
                if (listToParent.Count > 0)
                    DeleteItemFromTree(listToParent.ToList(), RootItems);
                this.visitId = 0;
            }
            catch (OperationCanceledException)
            {
                //Nothing to do as it means that we somehow cancelled save operation
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to delete visit from records list with Id = {0} for person with Id = {1}", this.visitId, personId);
                FailureMediator.Activate("Не удалось удалить случай из списока пациентов. Попробуйте еще раз или обратитесь в службу поддержки", exception: ex, canBeDeactivated: true);
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

        public async void DeleteRecordFromList(int? recordId)
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
            logService.InfoFormat("Delete record from records list with Id = {0} for person with Id = {1}", this.recordId, personId);
            BusyMediator.Activate("Удаление услуги из списока пациента...");
            var saveSuccesfull = false;
            var recordQuery = patientRecordsService.GetRecord(this.recordId);
            try
            {
                var actualDateTime = await recordQuery.Select(x => x.ActualDateTime).FirstOrDefaultAsync();
                var listToParent = await patientRecordsService.GetParentItems(new PersonRecItem() { Id = this.recordId, Type = ItemType.Record, ActualDatetime = actualDateTime });
                if (listToParent.Count > 0)
                    DeleteItemFromTree(listToParent.ToList(), RootItems);
                this.recordId = 0;
            }
            catch (OperationCanceledException)
            {
                //Nothing to do as it means that we somehow cancelled save operation
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to delete record from records list with Id = {0} for person with Id = {1}", this.recordId, personId);
                FailureMediator.Activate("Не удалось удалить услугу из списока пациента. Попробуйте еще раз или обратитесь в службу поддержки", exception: ex, canBeDeactivated: true);
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

        private void DeleteItemFromTree(List<PersonRecItem> listToParent, ObservableCollectionEx<IHierarchicalItem> curLevelItems, int curParentIndex = 0)
        {
            IHierarchicalItem curTreeItem = null;
            PersonRecItem curItem = listToParent[curParentIndex];
            for (int i = 0; i < curLevelItems.Count; i++)
            {
                curTreeItem = curLevelItems[i];
                if (curTreeItem.Item.Equals(curItem))
                {
                    if (listToParent.Count - 1 == curParentIndex)
                    {
                        curLevelItems.Remove(curTreeItem);
                        return;
                    }
                    else
                    {
                        curTreeItem.IsExpanded = true;
                        DeleteItemFromTree(listToParent, curTreeItem.Childs, ++curParentIndex);
                    }
                }
            }
        }

        public void Dispose()
        {
            addNewVisitInListVisitCommandWrapper.Dispose();
            reloadPatientVisitsCommandWrapper.Dispose();
        }

        public void SelectItem(int assignmentId = 0, int recordId = 0, int visitId = 0)
        {
            IHierarchicalItem sItem = null;
            if (assignmentId != 0)
                sItem = RootItems.FirstOrDefault(x => x.Item.Id == assignmentId && x.Item.Type == ItemType.Assignment);
            else if (recordId != 0)
                sItem = RootItems.FirstOrDefault(x => x.Item.Id == recordId && x.Item.Type == ItemType.Record);
            else if (visitId != 0)
                sItem = RootItems.FirstOrDefault(x => x.Item.Id == visitId && x.Item.Type == ItemType.Visit);

            if (sItem != null)
                sItem.IsSelected = true;
        }

        public async Task LoadRootItemsAsync(int personId, int assignmentId = 0, int recordId = 0, int visitId = 0)
        {
            RootItems.Clear();
            this.PersonId = personId;
            if (personId == SpecialValues.NewId || personId == SpecialValues.NonExistingId)
            {
                return;
            }
            var loadingIsCompleted = false;
            currentOperationToken = new CancellationTokenSource();
            var token = currentOperationToken.Token;
            BusyMediator.Activate(string.Empty);
            logService.InfoFormat("Loading root items for person with Id {0}...", personId);
            IDisposableQueryable<Assignment> assignmentsQuery = null;
            IDisposableQueryable<Record> recordsQuery = null;
            IDisposableQueryable<Visit> visitsQuery = null;
            try
            {
                assignmentsQuery = patientRecordsService.GetPersonRootAssignmentsQuery(personId);
                recordsQuery = patientRecordsService.GetPersonRecordsQuery(personId);
                visitsQuery = patientRecordsService.GetPersonVisitsQuery(personId, false);
                var loadAssignmentsTask = assignmentsQuery.Select(x => new PersonRecItem
                {
                    Id = x.Id,
                    ActualDatetime = x.AssignDateTime,
                    Type = ItemType.Assignment
                }).ToListAsync(token);
                var loadRecordsTask = recordsQuery.Select(x => new PersonRecItem
                {
                    Id = x.Id,
                    ActualDatetime = x.ActualDateTime,
                    Type = ItemType.Record
                }).ToListAsync(token);
                var loadVisitsTask = visitsQuery.Select(x => new PersonRecItem
                {
                    Id = x.Id,
                    ActualDatetime = x.BeginDateTime,
                    Type = ItemType.Visit
                }).ToListAsync(token);
                await Task.WhenAll(loadAssignmentsTask, loadRecordsTask, loadVisitsTask);
                var resChilds = loadAssignmentsTask.Result.Union(loadRecordsTask.Result).Union(loadVisitsTask.Result).OrderBy(x => x.ActualDatetime);
                RootItems.AddRange(resChilds.Select(x => hierarchicalItemViewModelRepository.GetHierarchicalItem(x)));
                loadingIsCompleted = true;
            }
            catch (OperationCanceledException)
            {
                //Do nothing. Cancelled operation means that user selected different patient before previous one was loaded
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to load root items for person with Id {0}", personId);
                FailureMediator.Activate("Не удалость загрузить данные по услугам, случаям и назначениям пациента. Попробуйте еще раз или обратитесь в службу поддержки", reloadPatientVisitsCommandWrapper, ex);
                loadingIsCompleted = true;
            }
            finally
            {
                CommandManager.InvalidateRequerySuggested();
                if (loadingIsCompleted)
                {
                    BusyMediator.Deactivate();
                }
                if (assignmentsQuery != null)
                {
                    assignmentsQuery.Dispose();
                }
                if (recordsQuery != null)
                {
                    recordsQuery.Dispose();
                }
                if (visitsQuery != null)
                {
                    visitsQuery.Dispose();
                }
            }
        }       
        
        #endregion

    }
}
