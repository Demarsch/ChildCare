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

namespace Shared.PatientRecords.ViewModels
{
    public class PersonRecordListViewModel : BindableBase, IConfirmNavigationRequest, IDisposable
    {
        #region Fields

        private readonly IPatientRecordsService patientRecordsService;
        private readonly IEventAggregator eventAggregator;
        private readonly IDialogService dialogService;
        private readonly ILog logService;
        private readonly CommandWrapper reloadPatientVisitsCommandWrapper;
        private readonly CommandWrapper deleteVisitCommandWrapper;
        private readonly CommandWrapper addNewVisitInListVisitCommandWrapper;
        private readonly CommandWrapper completeVisitCommandWrapper;
        private readonly CommandWrapper completeRecordCommandWrapper;
        private readonly CommandWrapper inProgressRecordCommandWrapper;
        //private readonly ChangeTrackerEx<PersonRecordListViewModel> changeTracker;
        private readonly DelegateCommand<int?> createNewVisitCommand;
        private readonly DelegateCommand<int?> editVisitCommand;
        private readonly DelegateCommand<int?> deleteVisitCommand;
        private readonly DelegateCommand<int?> completeVisitCommand;
        private readonly DelegateCommand<int?> completeRecordCommand;
        private readonly DelegateCommand<int?> inProgressRecordCommand;
        private readonly DelegateCommand<int?> returnToActiveVisitCommand;

        private readonly Func<VisitEditorViewModel> visitEditorViewModelFactory;
        private readonly Func<VisitCloseViewModel> visitCloseViewModelFactory;

        private CancellationTokenSource currentOperationToken;
        private int? visitId;
        private int? recordId;
        #endregion

        #region  Constructors
        public PersonRecordListViewModel(IPatientRecordsService patientRecordsService, ILog logService, IDialogService dialogService, IEventAggregator eventAggregator, Func<VisitEditorViewModel> visitEditorViewModelFactory, Func<VisitCloseViewModel> visitCloseViewModelFactory)
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
            if (visitEditorViewModelFactory == null)
            {
                throw new ArgumentNullException("newVisitCreatingViewModelFactory");
            }
            if (visitCloseViewModelFactory == null)
            {
                throw new ArgumentNullException("visitCloseViewModelFactory");
            }
            if (dialogService == null)
            {
                throw new ArgumentNullException("dialogService");
            }
            this.dialogService = dialogService;
            this.visitCloseViewModelFactory = visitCloseViewModelFactory;
            this.visitEditorViewModelFactory = visitEditorViewModelFactory;
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
            deleteVisitCommandWrapper = new CommandWrapper { Command = deleteVisitCommand };
            completeVisitCommandWrapper = new CommandWrapper { Command = completeVisitCommand };
            completeRecordCommandWrapper = new CommandWrapper { Command = completeRecordCommand };
            inProgressRecordCommandWrapper = new CommandWrapper { Command = inProgressRecordCommand };
            addNewVisitInListVisitCommandWrapper = new CommandWrapper { Command = new DelegateCommand(() => AddNewVisitToList(visitId)) };
            createNewVisitCommand = new DelegateCommand<int?>(CreateNewVisit);
            editVisitCommand = new DelegateCommand<int?>(EditVisit);
            deleteVisitCommand = new DelegateCommand<int?>(DeleteVisitAsync);
            completeVisitCommand = new DelegateCommand<int?>(CompleteVisitAsync);
            completeRecordCommand = new DelegateCommand<int?>(CompleteRecordAsync);
            inProgressRecordCommand = new DelegateCommand<int?>(InProgressRecord);
            returnToActiveVisitCommand = new DelegateCommand<int?>(ReturnToActiveVisit);
            VisitEditorInteractionRequest = new InteractionRequest<VisitEditorViewModel>();
            VisitCloseInteractionRequest = new InteractionRequest<VisitCloseViewModel>();
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

        private object selectedItem;
        public object SelectedItem
        {
            get { return selectedItem; }
            set { SetProperty(ref selectedItem, value); }
        }

        public BusyMediator BusyMediator { get; set; }

        public FailureMediator FailureMediator { get; private set; }

        public InteractionRequest<VisitEditorViewModel> VisitEditorInteractionRequest { get; private set; }

        public InteractionRequest<VisitCloseViewModel> VisitCloseInteractionRequest { get; private set; }
        #endregion

        #region Commands
        public ICommand CreateNewVisitCommand { get { return createNewVisitCommand; } }
        private void CreateNewVisit(int? selectedTemplate)
        {
            var newVisitCreatingViewModel = visitEditorViewModelFactory();
            newVisitCreatingViewModel.IntializeCreation(PersonId, selectedTemplate, null, DateTime.Now, "Создать новый случай");
            VisitEditorInteractionRequest.Raise(newVisitCreatingViewModel, (vm) => { AddNewVisitToList(vm.VisitId); });
        }

        public ICommand EditVisitCommand { get { return editVisitCommand; } }
        private void EditVisit(int? visitId)
        {
            if (visitId < 1)
            {
                FailureMediator.Activate("Данная случай не найден", true);
                return;
            }
            var newVisitCreatingViewModel = visitEditorViewModelFactory();
            newVisitCreatingViewModel.IntializeCreation(PersonId, null, visitId, DateTime.Now, "Редактировать случай");
            VisitEditorInteractionRequest.Raise(newVisitCreatingViewModel, (vm) => { /*UpdateVisit(vm.VisitId);*/ });
        }

        public ICommand CompleteVisitCommand { get { return completeVisitCommand; } }
        private void CompleteVisitAsync(int? visitId)
        {
            if (visitId < 1)
            {
                dialogService.ShowError("Данная случай не найден");
                return;
            }
            var visitCloseViewModel = visitCloseViewModelFactory();
            visitCloseViewModel.IntializeCreation(visitId.Value, "Завершить случай");
            VisitCloseInteractionRequest.Raise(visitCloseViewModel, (vm) => {/* UpdateVisit(vm.VisitId);*/ });
        }

        public ICommand CompleteRecordCommand { get { return completeRecordCommand; } }
        private async void CompleteRecordAsync(int? recordId)
        {
            if (!recordId.HasValue)
            {
                dialogService.ShowError("Данная запись не найдена");
                return;
            }
            FailureMediator.Deactivate();
            this.recordId = recordId;
            if (currentOperationToken != null)
            {
                currentOperationToken.Cancel();
                currentOperationToken.Dispose();
            }
            currentOperationToken = new CancellationTokenSource();
            var token = currentOperationToken.Token;
            logService.InfoFormat("Complete record with Id = {0}", recordId);
            BusyMediator.Activate("Закрытие услуги...");
            var saveSuccesfull = false;
            var errors = string.Empty;
            IDisposableQueryable<Record> recordQuery = patientRecordsService.GetRecord(recordId.Value);
            try
            {
                var recordTask = recordQuery.Select(x => new
                {
                    x.EndDateTime
                }).FirstOrDefaultAsync();
                var isBrigadeComletedTask = patientRecordsService.IsBrigadeCompleted(recordId.Value);
                await Task.WhenAll(recordTask, isBrigadeComletedTask);

                if (!recordTask.Result.EndDateTime.HasValue)
                {
                    if (errors != string.Empty)
                        errors += "\r\n";
                    errors += "дату окончания услуги";
                }

                if (!isBrigadeComletedTask.Result)
                {
                    if (errors != string.Empty)
                        errors += "\r\n";
                    errors += "всех членов бригады, которые являются обязательными";
                }
                //ToDo: check Protocol.CanComplete() 

                if (!string.IsNullOrEmpty(errors))
                {
                    dialogService.ShowError("Чтобы закрыть услугу необходимо указать " + errors);
                }
                else
                {
                    patientRecordsService.CompleteRecordAsync(recordId.Value, token);
                    saveSuccesfull = true;

                    this.recordId = 0;
                }
                //UpdateVisit(visitId);
            }
            catch (OperationCanceledException)
            {
                //Nothing to do as it means that we somehow cancelled save operation
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to complete record with Id = {0}", recordId);
                FailureMediator.Activate("Не удалось закрыть услугу. Попробуйте еще раз или обратитесь в службу поддержки", completeVisitCommandWrapper, ex, true);
            }
            finally
            {
                BusyMediator.Deactivate();
                if (saveSuccesfull)
                {

                }
            }
        }

        public ICommand InProgressRecordCommand { get { return inProgressRecordCommand; } }
        private void InProgressRecord(int? recordId)
        {
            if (!recordId.HasValue)
            {
                dialogService.ShowError("Данная запись не найдена");
                return;
            }
            FailureMediator.Deactivate();
            this.recordId = recordId;
            if (currentOperationToken != null)
            {
                currentOperationToken.Cancel();
                currentOperationToken.Dispose();
            }
            currentOperationToken = new CancellationTokenSource();
            var token = currentOperationToken.Token;
            logService.InfoFormat("InProgress record with Id = {0}", recordId);
            BusyMediator.Activate("Продолжить услугу...");
            var saveSuccesfull = false;
            var errors = string.Empty;
            try
            {
                patientRecordsService.InProgressRecordAsync(recordId.Value, token);
                saveSuccesfull = true;

                this.recordId = 0;
                //UpdateVisit(visitId);
            }
            catch (OperationCanceledException)
            {
                //Nothing to do as it means that we somehow cancelled save operation
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to inProgress record with Id = {0}", recordId);
                FailureMediator.Activate("Не удалось открыть и продолжить услугу. Попробуйте еще раз или обратитесь в службу поддержки", completeVisitCommandWrapper, ex, true);
            }
            finally
            {
                BusyMediator.Deactivate();
                if (saveSuccesfull)
                {

                }
            }
        }

        public ICommand ReturnToActiveVisitCommand { get { return returnToActiveVisitCommand; } }
        private void ReturnToActiveVisit(int? visitId)
        {
            if (visitId < 1) return;
            FailureMediator.Deactivate();
            this.visitId = visitId;
            if (currentOperationToken != null)
            {
                currentOperationToken.Cancel();
                currentOperationToken.Dispose();
            }
            currentOperationToken = new CancellationTokenSource();
            var token = currentOperationToken.Token;
            logService.InfoFormat("Uncompleting visit with Id = {0} for person with Id = {1}", visitId, personId);
            BusyMediator.Activate("Открытие случая...");
            var saveSuccesfull = false;
            try
            {
                patientRecordsService.ReturnToActiveVisitAsync(visitId.Value, token);
                saveSuccesfull = true;
                this.visitId = 0;
                //UpdateVisit(visitId);
            }
            catch (OperationCanceledException)
            {
                //Nothing to do as it means that we somehow cancelled save operation
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to uncomplete visit with Id = {0} for person with Id = {1}", visitId, personId);
                FailureMediator.Activate("Не удалось открыть случай. Попробуйте еще раз или обратитесь в службу поддержки", completeVisitCommandWrapper, ex);
            }
            finally
            {
                BusyMediator.Deactivate();
                if (saveSuccesfull)
                {

                }
            }
        }

        public ICommand DeleteVisitCommand { get { return deleteVisitCommand; } }
        private async void DeleteVisitAsync(int? visitId)
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
            logService.InfoFormat("Deleting visit with Id = {0} for person with Id = {1}", visitId, personId);
            BusyMediator.Activate("Удаление случая...");
            var saveSuccesfull = false;
            try
            {
                patientRecordsService.DeleteVisitAsync(visitId.Value, 1, token);
                saveSuccesfull = true;
                this.visitId = 0;
                //DeleteVisitFromList(visitId);
            }
            catch (OperationCanceledException)
            {
                //Nothing to do as it means that we somehow cancelled save operation
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to delete visit with Id = {0} for person with Id = {1}", visitId, personId);
                FailureMediator.Activate("Не удалось удалить случай. Попробуйте еще раз или обратитесь в службу поддержки", deleteVisitCommandWrapper, ex);
            }
            finally
            {
                BusyMediator.Deactivate();
                if (saveSuccesfull)
                {

                }
            }
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
                FailureMediator.Activate("Не удалось добавить новый случай в список пациенту. Попробуйте еще раз или обратитесь в службу поддержки", deleteVisitCommandWrapper, ex);
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
            UnsubscriveFromEvents();
            addNewVisitInListVisitCommandWrapper.Dispose();
            completeVisitCommandWrapper.Dispose();
            deleteVisitCommandWrapper.Dispose();
            reloadPatientVisitsCommandWrapper.Dispose();
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
