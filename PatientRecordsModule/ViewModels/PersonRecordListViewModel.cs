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
        private int? visitId;
        private int? recordId;
        private int? assignmentId;
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
            this.assignmentId = assignmentId;
            if (currentOperationToken != null)
            {
                currentOperationToken.Cancel();
                currentOperationToken.Dispose();
            }
            currentOperationToken = new CancellationTokenSource();
            var token = currentOperationToken.Token;
            logService.InfoFormat("Additing new assignment in records list with Id = {0} for person with Id = {1}", assignmentId, personId);
            BusyMediator.Activate("Добавление нового назначения в список пациенту...");
            var saveSuccesfull = false;
            var assignmentQuery = patientRecordsService.GetAssignment(assignmentId.Value);
            try
            {
                var assignmentDTO = await assignmentQuery.Select(x => new AssignmentDTO()
                {
                    Id = x.Id,
                    ActualDateTime = x.AssignDateTime,
                    FinancingSourceName = x.FinancingSource.Name,
                    RecordTypeName = x.RecordType.ShortName != string.Empty ? x.RecordType.ShortName : x.RecordType.Name,
                    RoomName = (x.Room.Number != string.Empty ? x.Room.Number + " - " : string.Empty) + x.Room.Name,
                }).FirstOrDefaultAsync(token);
                saveSuccesfull = true;
                this.assignmentId = 0;
                var listToParent = await patientRecordsService.GetParentItems(new PersonItem() { Id = assignmentDTO.Id, Type = ItemType.Assignment });
                if (listToParent.Count > 0)
                    InsertItemInRootItems(listToParent.ToList(), new PersonHierarchicalAssignmentsViewModel(assignmentDTO, patientRecordsService, eventAggregator, logService), RootItems);
            }
            catch (OperationCanceledException)
            {
                //Nothing to do as it means that we somehow cancelled save operation
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to add new assignment in records list with Id = {0} for person with Id = {1}", assignmentId, personId);
                FailureMediator.Activate("Не удалось добавить новое назначение в список пациенту. Попробуйте еще раз или обратитесь в службу поддержки", addNewAssignmentInListVisitCommandWrapper, ex);
            }
            finally
            {
                BusyMediator.Deactivate();
                if (saveSuccesfull)
                {

                }
            }
        }

        public async void AddNewVisitToList(int? visitId)
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
                var listToParent = await patientRecordsService.GetParentItems(new PersonItem() { Id = visitDTO.Id, Type = ItemType.Visit });
                if (listToParent.Count > 0)
                    InsertItemInRootItems(listToParent.ToList(), new PersonHierarchicalVisitsViewModel(visitDTO, patientRecordsService, eventAggregator, logService), RootItems);
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

        public async void AddNewRecordToList(int? recordId)
        {
            FailureMediator.Deactivate();
            this.recordId = recordId;
            if (currentOperationToken != null)
            {
                currentOperationToken.Cancel();
                currentOperationToken.Dispose();
            }
            currentOperationToken = new CancellationTokenSource();
            var token = currentOperationToken.Token;
            logService.InfoFormat("Additing new visit in records list with Id = {0} for person with Id = {1}", recordId, personId);
            BusyMediator.Activate("Добавление нового случая в список пациенту...");
            var saveSuccesfull = false;
            var visitQuery = patientRecordsService.GetRecord(recordId.Value);
            try
            {
                var recordDTO = await visitQuery.Select(x => new RecordDTO()
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
                saveSuccesfull = true;
                this.recordId = 0;
                var listToParent = await patientRecordsService.GetParentItems(new PersonItem() { Id = recordDTO.Id, Type = ItemType.Record });
                if (listToParent.Count > 0)
                    InsertItemInRootItems(listToParent.ToList(), new PersonHierarchicalRecordsViewModel(recordDTO, patientRecordsService, eventAggregator, logService), RootItems);
            }
            catch (OperationCanceledException)
            {
                //Nothing to do as it means that we somehow cancelled save operation
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to add new visit in records list with Id = {0} for person with Id = {1}", recordId, personId);
                FailureMediator.Activate("Не удалось добавить новый случай в список пациенту. Попробуйте еще раз или обратитесь в службу поддержки", addNewRecordInListVisitCommandWrapper, ex);
            }
            finally
            {
                BusyMediator.Deactivate();
                if (saveSuccesfull)
                {

                }
            }
        }

        private void InsertItemInRootItems(List<PersonItem> listToParent, IHierarchicalItem item, ObservableCollectionEx<IHierarchicalItem> curLevelItems, int curParentIndex = 0)
        {
            IHierarchicalItem curItem = null;
            var isAdded = false;
            if (curLevelItems.Count > 0)
            {
                for (int i = 0; i < curLevelItems.Count; i++)
                {
                    curItem = curLevelItems[i];
                    if (curParentIndex == listToParent.Count - 1)
                    {
                        if (curItem.ActualDateTime >= item.ActualDateTime)
                        {
                            curLevelItems.Insert(i, item);
                            item.IsSelected = true;
                            isAdded = true;
                            return;
                        }
                    }
                    else
                        if (curItem.Item.Equals(listToParent[curParentIndex]))
                        {
                            curItem.IsExpanded = true;
                            InsertItemInRootItems(listToParent, item, curItem.Childs, ++curParentIndex);
                            isAdded = true;
                        }
                }
                if ((curParentIndex == listToParent.Count - 1) && !isAdded)
                {
                    curLevelItems.Add(item);
                    item.IsSelected = true;
                    isAdded = true;
                    return;
                }
            }
            else
            {
                curLevelItems.Add(item);
                item.IsSelected = true;
                //if (curParentIndex != listToParent.Count - 1)
                //    InsertItemInRootItems(listToParent, item, curItem.Childs, ++curParentIndex);
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
    }
}
