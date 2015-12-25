using Core.Data;
using Core.Data.Misc;
using Core.Wpf.Events;
using Core.Wpf.Services;
using log4net;
using Shared.PatientRecords.Services;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using Shell.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Prism.Commands;
using Core.Wpf.Misc;
using Prism.Interactivity.InteractionRequest;
using Core.Wpf.Mvvm;
using System.Threading;
using System.Data.Entity;
using Core.Extensions;
using Shared.PatientRecords.Misc;

namespace Shared.PatientRecords.ViewModels
{
    public class PersonRecordsViewModel : BindableBase, IConfirmNavigationRequest, IDisposable
    {
        #region Fields

        private readonly IPatientRecordsService patientRecordsService;
        private readonly IEventAggregator eventAggregator;
        private readonly IDialogService messageService;
        private readonly IDialogServiceAsync dialogServiceAsync;
        private readonly ILog logService;

        private readonly DelegateCommand<int?> createNewVisitCommand;
        private readonly DelegateCommand<int?> editVisitCommand;
        private readonly DelegateCommand<int?> deleteVisitCommand;
        private readonly DelegateCommand<int?> completeVisitCommand;
        private readonly DelegateCommand createRecordCommand;
        private readonly DelegateCommand<int?> completeRecordCommand;
        private readonly DelegateCommand<int?> inProgressRecordCommand;
        private readonly DelegateCommand<int?> returnToActiveVisitCommand;
        private readonly DelegateCommand<object> сreateAnalyseCommand;

        private readonly CommandWrapper deleteVisitCommandWrapper;
        private readonly CommandWrapper completeVisitCommandWrapper;
        private readonly CommandWrapper completeRecordCommandWrapper;
        private readonly CommandWrapper inProgressRecordCommandWrapper;

        private readonly Func<VisitEditorViewModel> visitEditorViewModelFactory;
        private readonly Func<VisitCloseViewModel> visitCloseViewModelFactory;
        private readonly Func<AnalyseCreateViewModel> analyseCreateViewModelFactory;
        private readonly Func<RecordCreateViewModel> recordCreateViewModelFactory;

        private readonly PersonRecordEditorViewModel personRecordEditorViewModel;
        private readonly PersonRecordListViewModel personRecordListViewModel;

        private CancellationTokenSource currentOperationToken;
        private int? visitId;
        private int? recordId;
        #endregion

        #region  Constructors
        public PersonRecordsViewModel(IPatientRecordsService patientRecordsService, IDialogService messageService, IDialogServiceAsync dialogServiceAsync, IEventAggregator eventAggregator, ILog logService,
            PersonRecordEditorViewModel personRecordEditorViewModel, PersonRecordListViewModel personRecordListViewModel,
            Func<VisitEditorViewModel> visitEditorViewModelFactory, Func<VisitCloseViewModel> visitCloseViewModelFactory, Func<AnalyseCreateViewModel> analyseCreateViewModelFactory, Func<RecordCreateViewModel> recordCreateViewModelFactory)
        {
            if (patientRecordsService == null)
            {
                throw new ArgumentNullException("patientRecordsService");
            }
            if (eventAggregator == null)
            {
                throw new ArgumentNullException("patientRecordsService");
            }
            if (logService == null)
            {
                throw new ArgumentNullException("logService");
            }
            if (visitEditorViewModelFactory == null)
            {
                throw new ArgumentNullException("visitEditorViewModelFactory");
            }
            if (visitCloseViewModelFactory == null)
            {
                throw new ArgumentNullException("visitCloseViewModelFactory");
            }
            if (analyseCreateViewModelFactory == null)
            {
                throw new ArgumentNullException("analyseCreateViewModelFactory");
            }
            if (messageService == null)
            {
                throw new ArgumentNullException("messageService");
            }
            if (dialogServiceAsync == null)
            {
                throw new ArgumentNullException("dialogServiceAsync");
            }
            if (personRecordEditorViewModel == null)
            {
                throw new ArgumentNullException("personRecordEditorViewModel");
            }
            if (personRecordListViewModel == null)
            {
                throw new ArgumentNullException("personRecordListViewModel");
            } if (recordCreateViewModelFactory == null)
            {
                throw new ArgumentNullException("recordCreateViewModelFactory");
            }
            this.recordCreateViewModelFactory = recordCreateViewModelFactory;
            this.personRecordEditorViewModel = personRecordEditorViewModel;
            this.personRecordEditorViewModel.PropertyChanged += personRecordEditorViewModel_PropertyChanged;
            this.personRecordListViewModel = personRecordListViewModel;
            this.dialogServiceAsync = dialogServiceAsync;
            this.messageService = messageService;
            this.logService = logService;
            this.eventAggregator = eventAggregator;
            this.patientRecordsService = patientRecordsService;
            this.visitCloseViewModelFactory = visitCloseViewModelFactory;
            this.visitEditorViewModelFactory = visitEditorViewModelFactory;
            this.analyseCreateViewModelFactory = analyseCreateViewModelFactory;
            personId = SpecialValues.NonExistingId;

            createNewVisitCommand = new DelegateCommand<int?>(CreateNewVisit);
            editVisitCommand = new DelegateCommand<int?>(EditVisit);
            deleteVisitCommand = new DelegateCommand<int?>(DeleteVisitAsync);
            completeVisitCommand = new DelegateCommand<int?>(CompleteVisitAsync);
            createRecordCommand = new DelegateCommand(CreateRecord);
            completeRecordCommand = new DelegateCommand<int?>(CompleteRecordAsync);
            inProgressRecordCommand = new DelegateCommand<int?>(InProgressRecord);
            returnToActiveVisitCommand = new DelegateCommand<int?>(ReturnToActiveVisit);
            сreateAnalyseCommand = new DelegateCommand<object>(СreateAnalyse);

            deleteVisitCommandWrapper = new CommandWrapper { Command = deleteVisitCommand };
            completeVisitCommandWrapper = new CommandWrapper { Command = completeVisitCommand };
            completeRecordCommandWrapper = new CommandWrapper { Command = completeRecordCommand };
            inProgressRecordCommandWrapper = new CommandWrapper { Command = inProgressRecordCommand };

            VisitEditorInteractionRequest = new InteractionRequest<VisitEditorViewModel>();
            VisitCloseInteractionRequest = new InteractionRequest<VisitCloseViewModel>();

            FailureMediator = new FailureMediator();
            BusyMediator = new BusyMediator();
            NotificationMediator = new NotificationMediator();
            SubscribeToEvents();
        }
        #endregion

        #region Properties

        private int personId;
        public int PersonId
        {
            get { return personId; }
            set { SetProperty(ref personId, value); }
        }

        private IPersonRecordEditor selectedItemEditor;
        public IPersonRecordEditor SelectedItemEditor
        {
            get { return selectedItemEditor; }
            set { SetProperty(ref selectedItemEditor, value); }
        }

        public BusyMediator BusyMediator { get; set; }

        public FailureMediator FailureMediator { get; private set; }

        public NotificationMediator NotificationMediator { get; private set; }

        public InteractionRequest<VisitEditorViewModel> VisitEditorInteractionRequest { get; private set; }

        public InteractionRequest<VisitCloseViewModel> VisitCloseInteractionRequest { get; private set; }

        //Properties form personRecordEditorViewModel for header
        public bool IsViewModeInCurrentProtocolEditor
        {
            get { return personRecordEditorViewModel.IsViewModeInCurrentProtocolEditor; }
        }
        public bool IsEditModeInCurrentProtocolEditor
        {
            get { return personRecordEditorViewModel.IsEditModeInCurrentProtocolEditor; }
        }
        public bool IsRecordCanBeCompleted
        {
            get { return personRecordEditorViewModel.IsRecordCanBeCompleted; }
        }
        public bool AllowDocuments
        {
            get { return personRecordEditorViewModel.AllowDocuments; }
        }
        public bool AllowDICOM
        {
            get { return personRecordEditorViewModel.AllowDICOM; }
        }
        public bool CanAttachDICOM
        {
            get { return personRecordEditorViewModel.CanAttachDICOM; }
        }
        public bool CanDetachDICOM
        {
            get { return personRecordEditorViewModel.CanDetachDICOM; }
        }
        /////////////////////////////////////////////////////////

        #endregion

        #region Methods
        public void Dispose()
        {
            UnsubscriveFromEvents();
            completeVisitCommandWrapper.Dispose();
            deleteVisitCommandWrapper.Dispose();
        }

        private void SubscribeToEvents()
        {
            eventAggregator.GetEvent<PubSubEvent<IPersonRecordEditor>>().Subscribe(OnEditorSelected);
            eventAggregator.GetEvent<SelectionChangedEvent<Person>>().Subscribe(OnPatientSelected);
        }

        private void OnPatientSelected(int personId)
        {
            this.PersonId = personId;
            personRecordListViewModel.LoadRootItemsAsync(this.PersonId);
        }

        private void OnEditorSelected(IPersonRecordEditor personRecordEditor)
        {
            SelectedItemEditor = personRecordEditor;
        }

        private void UnsubscriveFromEvents()
        {
            eventAggregator.GetEvent<SelectionChangedEvent<Person>>().Unsubscribe(OnPatientSelected);
        }

        void personRecordEditorViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsViewModeInCurrentProtocolEditor")
                OnPropertyChanged(() => IsViewModeInCurrentProtocolEditor);
            if (e.PropertyName == "IsEditModeInCurrentProtocolEditor")
                OnPropertyChanged(() => IsEditModeInCurrentProtocolEditor);
            if (e.PropertyName == "IsRecordCanBeCompleted")
                OnPropertyChanged(() => IsRecordCanBeCompleted);

            if (e.PropertyName == "AllowDocuments")
                OnPropertyChanged(() => AllowDocuments);
            if (e.PropertyName == "AllowDICOM")
                OnPropertyChanged(() => AllowDICOM);
            if (e.PropertyName == "CanAttachDICOM")
                OnPropertyChanged(() => CanAttachDICOM);
            if (e.PropertyName == "CanDetachDICOM")
                OnPropertyChanged(() => CanDetachDICOM);
        }
        #endregion

        #region Commands
        public ICommand CreateNewVisitCommand { get { return createNewVisitCommand; } }
        private void CreateNewVisit(int? selectedTemplate)
        {
            var newVisitCreatingViewModel = visitEditorViewModelFactory();
            newVisitCreatingViewModel.IntializeCreation(PersonId, selectedTemplate, null, DateTime.Now, "Создать новый случай");
            VisitEditorInteractionRequest.Raise(newVisitCreatingViewModel, (vm) =>
            {
                personRecordListViewModel.AddNewVisitToList(vm.VisitId);
                NotificationMediator.Activate("Случай успешно создан", NotificationMediator.DefaultHideTime);
            });
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
                messageService.ShowError("Данная случай не найден");
                return;
            }
            var visitCloseViewModel = visitCloseViewModelFactory();
            visitCloseViewModel.IntializeCreation(visitId.Value, "Завершить случай");
            VisitCloseInteractionRequest.Raise(visitCloseViewModel, (vm) => {/* UpdateVisit(vm.VisitId);*/ });
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
        private void DeleteVisitAsync(int? visitId)
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
                    personRecordListViewModel.DeleteVisitFromList(visitId);
                    NotificationMediator.Activate("Случай успешно удален", NotificationMediator.DefaultHideTime);
                };
            }
        }

        public ICommand CreateRecordCommand { get { return createRecordCommand; } }
        private async void CreateRecord()
        {
            if (SpecialValues.IsNewOrNonExisting(this.PersonId))
            {
                messageService.ShowError("Не выбран пациент");
                return;
            }
            var recordCreateViewModel = recordCreateViewModelFactory();
            recordCreateViewModel.Initialize(this.PersonId);
            var result = await dialogServiceAsync.ShowDialogAsync(recordCreateViewModel);
            if (recordCreateViewModel.AssignIsSuccessful)
            {
                NotificationMediator.Activate("Услуга успешно создана", NotificationMediator.DefaultHideTime);
                personRecordListViewModel.AddNewRecordToList(recordCreateViewModel.RecordId);
            }
        }

        public ICommand CompleteRecordCommand { get { return completeRecordCommand; } }
        private async void CompleteRecordAsync(int? recordId)
        {
            if (!recordId.HasValue)
            {
                messageService.ShowError("Данная запись не найдена");
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
                    messageService.ShowError("Чтобы закрыть услугу необходимо указать " + errors);
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
                messageService.ShowError("Данная запись не найдена");
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


        public ICommand CreateAnalyseCommand { get { return сreateAnalyseCommand; } }
        private async void СreateAnalyse(object parameter)
        {
            if (SpecialValues.IsNewOrNonExisting(this.PersonId))
            {
                messageService.ShowError("Не выбран пациент");
                return;
            }
            if (!patientRecordsService.GetRooms(DateTime.Now).Any(x => x.Options.Contains(OptionValues.LaboratoryRoom)))
            {
                messageService.ShowError("В МИС не найдена информация о кабинете для проведения лабораторных исследований");
                return;
            }
            var values = (object[])parameter;
            int assignmentId = (int)values[0];
            int recordId = (int)values[1];
            int visitId = (int)values[2];
            var analyseCreateViewModel = analyseCreateViewModelFactory();
            analyseCreateViewModel.Initialize(this.PersonId, assignmentId, recordId, visitId);
            var result = await dialogServiceAsync.ShowDialogAsync(analyseCreateViewModel);
            if (analyseCreateViewModel.AssignIsSuccessful)
            {
                foreach (var assignAnalise in analyseCreateViewModel.AssignedAnalyses)
                {
                    personRecordListViewModel.AddNewAssignmentToList(assignAnalise.Key);
                }
            }
        }

        public ICommand PrintProtocolCommand { get { return personRecordEditorViewModel.PrintProtocolCommand; } }

        public ICommand SaveProtocolCommand { get { return personRecordEditorViewModel.SaveProtocolCommand; } }

        public ICommand ShowInEditModeCommand { get { return personRecordEditorViewModel.ShowInEditModeCommand; } }

        public ICommand ShowInViewModeCommand { get { return personRecordEditorViewModel.ShowInViewModeCommand; } }

        public ICommand AttachDocumentCommand { get { return personRecordEditorViewModel.AttachDocumentCommand; } }
        public ICommand DetachDocumentCommand { get { return personRecordEditorViewModel.DetachDocumentCommand; } }
        public ICommand AttachDICOMCommand { get { return personRecordEditorViewModel.AttachDICOMCommand; } }
        public ICommand DetachDICOMCommand { get { return personRecordEditorViewModel.DetachDICOMCommand; } }
        #endregion

        #region IConfirmNavigationRequest implimentation

        public void ConfirmNavigationRequest(NavigationContext navigationContext, Action<bool> continuationCallback)
        {
            continuationCallback(true);
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {

        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            var targetPatientId = (int?)navigationContext.Parameters[ParameterNames.PatientId] ?? SpecialValues.NonExistingId;
            if (targetPatientId != personId)
                PersonId = targetPatientId;
        }

        #endregion
    }
}
