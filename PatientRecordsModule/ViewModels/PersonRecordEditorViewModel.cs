using Core.Data;
using Core.Data.Misc;
using Core.Wpf.Events;
using Core.Wpf.Misc;
using Core.Wpf.Mvvm;
using log4net;
using PatientRecordsModule.DTO;
using PatientRecordsModule.Misc;
using PatientRecordsModule.Services;
using PatientRecordsModule.ViewModels;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Data.Entity;
using Core.Extensions;
using Core.Wpf.Services;
using PatientRecordsModule.ViewModels.RecordTypesProtocolViewModels;

namespace PatientRecordsModule.ViewModels
{
    public class PersonRecordEditorViewModel : BindableBase, IDisposable
    {
        #region Fields
        private readonly IPatientRecordsService patientRecordsService;
        private readonly ILog logService;
        private readonly IDiagnosService diagnosService;
        private readonly IRecordTypeEditorResolver recordTypeEditorResolver;
        private readonly IRecordService recordService;

        private readonly IEventAggregator eventAggregator;

        private readonly CommandWrapper reloadRecordBrigadeCommandWrapper;
        private readonly CommandWrapper reloadDataSourceCommandWrapper;
        private CancellationTokenSource currentOperationToken;

        private TaskCompletionSource<bool> dataSourcesLoadingTaskSource;

        private int recordTypeId = 0;
        #endregion

        #region Constructors
        public PersonRecordEditorViewModel(IPatientRecordsService patientRecordsService, IRecordService recordService, IDiagnosService diagnosService, IRecordTypeEditorResolver recordTypeEditorResolver,
                                           ILog logSevice, IEventAggregator eventAggregator, RecordDocumentsCollectionViewModel documentsViewer)
        {
            if (patientRecordsService == null)
            {
                throw new ArgumentNullException("patientRecordsService");
            }
            if (logSevice == null)
            {
                throw new ArgumentNullException("log");
            }
            if (diagnosService == null)
            {
                throw new ArgumentNullException("diagnosService");
            }
            if (recordService == null)
            {
                throw new ArgumentNullException("recordService");
            }
            if (eventAggregator == null)
            {
                throw new ArgumentNullException("eventAggregator");
            }
            if (recordTypeEditorResolver == null)
            {
                throw new ArgumentNullException("recordTypeEditorResolver");
            }
            this.recordTypeEditorResolver = recordTypeEditorResolver;
            this.patientRecordsService = patientRecordsService;
            this.diagnosService = diagnosService;
            this.recordService = recordService;
            this.logService = logSevice;
            this.eventAggregator = eventAggregator;

            this.documentsViewer = documentsViewer;
            this.documentsViewer.PropertyChanged += documentsViewer_PropertyChanged;

            reloadRecordBrigadeCommandWrapper = new CommandWrapper() { Command = new DelegateCommand(() => LoadBrigadeAsync(recordTypeId, RecordId)) };
            reloadDataSourceCommandWrapper = new CommandWrapper { Command = new DelegateCommand(() => EnsureDataSourceLoaded()) };

            BusyMediator = new BusyMediator();
            FailureMediator = new FailureMediator();
            Brigade = new ObservableCollectionEx<BrigadeViewModel>();
            Urgentlies = new ObservableCollectionEx<CommonIdName>();
            RecordPeriods = new ObservableCollectionEx<CommonIdName>();

            printProtocolCommand = new DelegateCommand(PrintProtocol);
            saveProtocolCommand = new DelegateCommand(SaveProtocol);
            showInEditModeCommand = new DelegateCommand(ShowProtocolInEditMode);
            showInViewModeCommand = new DelegateCommand(ShowProtocolInViewMode);
            setCurrentDateTimeEndCommand = new DelegateCommand(SetCurrentDateTimeEnd);
            setCurrentDateTimeBeginCommand = new DelegateCommand(SetCurrentDateTimeBegin);
            SubscribeToEvents();
        }

        #endregion

        #region Properties

        public ObservableCollectionEx<BrigadeViewModel> Brigade { get; set; }

        private int visitId;
        public int VisitId
        {
            get { return visitId; }
            set
            {
                SetProperty(ref visitId, value);
            }
        }

        private int assignmentId;
        public int AssignmentId
        {
            get { return assignmentId; }
            set
            {
                SetProperty(ref assignmentId, value);
            }
        }

        private int recordId;
        public int RecordId
        {
            get { return recordId; }
            set
            {
                SetProperty(ref recordId, value);
            }
        }

        private IRecordTypeProtocol protocolEditor;
        public IRecordTypeProtocol ProtocolEditor
        {
            get { return protocolEditor; }
            set
            {
                if (ProtocolEditor != null)
                    ProtocolEditor.PropertyChanged -= ProtocolEditor_PropertyChanged;
                SetProperty(ref protocolEditor, value);
                if (ProtocolEditor != null)
                {
                    ProtocolEditor.PropertyChanged += ProtocolEditor_PropertyChanged;
                    ProtocolEditor.LoadProtocol(AssignmentId, RecordId, VisitId);
                    LoadProtocolCommonData(VisitId, AssignmentId, RecordId);
                }
                OnPropertyChanged(() => IsViewModeInCurrentProtocolEditor);
                OnPropertyChanged(() => IsEditModeInCurrentProtocolEditor);
            }
        }

        public bool IsViewModeInCurrentProtocolEditor
        {
            get
            {
                return (ProtocolEditor != null && ProtocolEditor.CurrentMode == ProtocolMode.View);
            }
        }

        public bool IsEditModeInCurrentProtocolEditor
        {
            get
            {
                return (ProtocolEditor != null && ProtocolEditor.CurrentMode == ProtocolMode.Edit);
            }
        }

        public ObservableCollectionEx<CommonIdName> Urgentlies { get; set; }

        public ObservableCollectionEx<CommonIdName> RecordPeriods { get; set; }

        private int selectedPeriodId;
        public int SelectedPeriodId
        {
            get { return selectedPeriodId; }
            set { SetProperty(ref selectedPeriodId, value); }
        }

        private int selectedUrgentlyId;
        public int SelectedUrgentlyId
        {
            get { return selectedUrgentlyId; }
            set { SetProperty(ref selectedUrgentlyId, value); }
        }

        private DateTime? beginDateTime;
        public DateTime? BeginDateTime
        {
            get { return beginDateTime; }
            set { SetProperty(ref beginDateTime, value); }
        }

        private DateTime? endDateTime;
        public DateTime? EndDateTime
        {
            get { return endDateTime; }
            set { SetProperty(ref endDateTime, value); }
        }

        public BusyMediator BusyMediator { get; set; }

        public FailureMediator FailureMediator { get; private set; }

        #region Properties RecordDocuments

        private RecordDocumentsCollectionViewModel documentsViewer;
        public RecordDocumentsCollectionViewModel DocumentsViewer
        {
            get { return documentsViewer; }
            set { SetProperty(ref documentsViewer, value); }
        }

        private bool allowDocuments;
        public bool AllowDocuments
        {
            get { return DocumentsViewer.AllowDocuments; }
            set { SetProperty(ref allowDocuments, value); }
        }

        private bool allowDICOM;
        public bool AllowDICOM
        {
            get { return DocumentsViewer.AllowDICOM; }
            set { SetProperty(ref allowDICOM, value); }
        }

        private bool canAttachDICOM;
        public bool CanAttachDICOM
        {
            get { return DocumentsViewer.CanAttachDICOM; }
            set { SetProperty(ref canAttachDICOM, value); }
        }

        private bool canDetachDICOM;
        public bool CanDetachDICOM
        {
            get { return DocumentsViewer.CanDetachDICOM; }
            set { SetProperty(ref canDetachDICOM, value); }
        }
        #endregion

        #endregion

        #region Methods

        private async Task<bool> EnsureDataSourceLoaded()
        {
            if (dataSourcesLoadingTaskSource != null)
            {
                return await dataSourcesLoadingTaskSource.Task;
            }
            dataSourcesLoadingTaskSource = new TaskCompletionSource<bool>();
            Urgentlies.Clear();
            RecordPeriods.Clear();
            BusyMediator.Activate("Загрузка общих данных в редактор...");
            logService.Info("Loading data sources for common record editor...");
            IDisposableQueryable<Urgently> urgentliesQuery = null;
            DateTime curDate = DateTime.Now;
            try
            {
                urgentliesQuery = patientRecordsService.GetActualUrgentlies(curDate);

                var urgentlies = await urgentliesQuery.Select(x => new CommonIdName()
                {
                    Id = x.Id,
                    Name = x.Name
                }).ToListAsync();
                Urgentlies.AddRange(urgentlies);

                logService.InfoFormat("Data sources for common record editor are successfully loaded");
                dataSourcesLoadingTaskSource.SetResult(true);
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to load data sources for common record editor");
                FailureMediator.Activate("Не удалость загрузить данные для общих данных редактора. Попробуйте еще раз или обратитесь в службу поддержки", reloadDataSourceCommandWrapper, ex);
                dataSourcesLoadingTaskSource.SetResult(false);
            }
            finally
            {
                if (urgentliesQuery != null)
                {
                    urgentliesQuery.Dispose();
                }
                BusyMediator.Deactivate();
            }
            return await dataSourcesLoadingTaskSource.Task;
        }


        private async void LoadBrigadeAsync(int recordTypeId, int recordId)
        {
            FailureMediator.Deactivate();
            Brigade.Clear();
            if (recordId < 1 && recordTypeId < 1) return;
            this.recordTypeId = recordTypeId;
            var loadingIsCompleted = false;
            currentOperationToken = new CancellationTokenSource();
            var token = currentOperationToken.Token;
            BusyMediator.Activate(string.Empty);
            logService.InfoFormat("Loading brigade for record with Id ={0} and RecordType with Id ={1}", recordId, recordTypeId);
            IDisposableQueryable<Record> record = null;
            IDisposableQueryable<RecordMember> recordMembers = null;
            IDisposableQueryable<RecordTypeRolePermission> recordTypeRolePermission = null;
            try
            {
                record = patientRecordsService.GetRecord(recordId);
                var recordTypeResult = await record.Select(x => new { x.RecordTypeId, x.ActualDateTime }).FirstOrDefaultAsync(token);
                recordMembers = patientRecordsService.GetRecordMembers(recordId);
                recordTypeRolePermission = patientRecordsService.GetRecordTypeMembers(recordTypeResult.RecordTypeId, recordTypeResult.ActualDateTime);
                var recordMembersTask = recordMembers.Select(x => new
                {
                    PersonName = x.PersonStaff.Person.ShortName,
                    PersonId = x.PersonStaff.PersonId,
                    StaffName = x.PersonStaff.Staff.ShortName,
                    x.RecordTypeRolePermissionId,
                    x.PersonStaffId
                }).ToListAsync(token);
                var recordTypeMembersTask = recordTypeRolePermission.Select(x => new
                {
                    RoleName = x.RecordTypeRole.Name,
                    RoleId = x.RecordTypeMemberRoleId,
                    x.IsRequired,
                    PermissionName = x.Permission.Name,
                    PermissionId = x.PermissionId,
                    RecordTypeRolePermissionId = x.Id
                }).ToListAsync(token);
                await Task.WhenAll(recordMembersTask, recordTypeMembersTask);
                var recordMembersresult = recordMembersTask.Result;
                Brigade.AddRange(recordTypeMembersTask.Result.Select(x => new BrigadeViewModel(patientRecordsService, logService, new BrigadeDTO()
                {
                    IsRequired = x.IsRequired,
                    RoleName = x.RoleName,
                    RoleId = x.RoleId,
                    PermissionId = x.PermissionId,
                    RecordTypeRolePermissionId = x.RecordTypeRolePermissionId,
                    PersonName = recordMembersresult.Any(y => y.RecordTypeRolePermissionId == x.RecordTypeRolePermissionId) ?
                        recordMembersresult.FirstOrDefault(y => y.RecordTypeRolePermissionId == x.RecordTypeRolePermissionId).PersonName : string.Empty,
                    StaffName = recordMembersresult.Any(y => y.RecordTypeRolePermissionId == x.RecordTypeRolePermissionId) ?
                        recordMembersresult.FirstOrDefault(y => y.RecordTypeRolePermissionId == x.RecordTypeRolePermissionId).StaffName : string.Empty,
                    PersonStaffId = recordMembersresult.Any(y => y.RecordTypeRolePermissionId == x.RecordTypeRolePermissionId) ?
                        recordMembersresult.FirstOrDefault(y => y.RecordTypeRolePermissionId == x.RecordTypeRolePermissionId).PersonStaffId : 0,
                    RecordTypeId = recordTypeResult.RecordTypeId,
                    OnDate = recordTypeResult.ActualDateTime
                })).ToList());
                loadingIsCompleted = true;
                this.recordTypeId = 0;
            }
            catch (OperationCanceledException)
            {
                //Do nothing. Cancelled operation means that user selected different patient before previous one was loaded
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to load brigade for record with Id ={0} and RecordType with Id ={1}", recordId, recordTypeId);
                FailureMediator.Activate("Не удалось загрузить данные о бригаде. Попробуйте еще раз или обратитесь в службу поддержки", reloadRecordBrigadeCommandWrapper, ex);
                loadingIsCompleted = true;
            }
            finally
            {
                CommandManager.InvalidateRequerySuggested();
                if (loadingIsCompleted)
                {
                    BusyMediator.Deactivate();
                }
                if (record != null)
                {
                    record.Dispose();
                }
            }
        }

        public void Dispose()
        {
            UnsubscriveFromEvents();
        }

        private void SubscribeToEvents()
        {
            eventAggregator.GetEvent<SelectionEvent<Visit>>().Subscribe(OnVisitSelected);
            eventAggregator.GetEvent<SelectionEvent<Assignment>>().Subscribe(OnAssignmentSelected);
            eventAggregator.GetEvent<SelectionEvent<Record>>().Subscribe(OnRecordSelected);
        }

        private void OnRecordSelected(int recordId)
        {
            SetRVAIds(0, 0, recordId);
        }

        private void OnAssignmentSelected(int assignmentId)
        {
            SetRVAIds(0, assignmentId, 0);
        }

        private void OnVisitSelected(int visitId)
        {
            SetRVAIds(visitId, 0, 0);
        }

        private async void SetRVAIds(int visitId, int assignmentId, int recordId)
        {
            VisitId = visitId;
            AssignmentId = assignmentId;
            RecordId = recordId;
            LoadProtocolEditor(visitId, assignmentId, recordId);

            await DocumentsViewer.LoadDocuments(assignmentId, recordId);
        }

        private async void LoadProtocolCommonData(int visitId, int assignmentId, int recordId)
        {
            var dataSourcesLoaded = await EnsureDataSourceLoaded();
            RecordPeriods.Clear();
            //ChangeTracker.IsEnabled = false;
            if (!dataSourcesLoaded)
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
            BusyMediator.Activate("Загрузка общих данных протокола...");
            logService.InfoFormat("Loading common record data...");
            IDisposableQueryable<Record> recordQuery = patientRecordsService.GetRecord(recordId);
            IDisposableQueryable<Assignment> assignmentQuery = patientRecordsService.GetAssignment(assignmentId);
            IDisposableQueryable<RecordPeriod> recordPeriodsQuery = null;
            DateTime curDate = DateTime.Now;
            try
            {
                CommonRecordAssignmentDTO data = new CommonRecordAssignmentDTO();
                if (recordId > 0)
                {
                    data = await recordQuery.Select(x => new CommonRecordAssignmentDTO()
                        {
                            RecordTypeId = x.RecordTypeId,
                            ExecutionPlaceId = x.Visit.ExecutionPlaceId,
                            RecordPeriodId = x.RecordPeriodId,
                            BeginDateTime = x.BeginDateTime,
                            EndDateTime = x.EndDateTime,
                            UrgentlyId = x.UrgentlyId
                        }).FirstOrDefaultAsync(token);
                    LoadBrigadeAsync(data.RecordTypeId, recordId);
                }
                else if (assignmentId > 0)
                {
                    data = await assignmentQuery.Select(x => new CommonRecordAssignmentDTO()
                    {
                        RecordTypeId = x.RecordTypeId,
                        ExecutionPlaceId = x.ExecutionPlaceId,// x.VisitId.HasValue ? x.Visit.ExecutionPlaceId
                        BeginDateTime = x.AssignDateTime,
                        UrgentlyId = x.UrgentlyId
                    }).FirstOrDefaultAsync(token);
                    LoadBrigadeAsync(data.RecordTypeId, 0);
                }
                recordPeriodsQuery = patientRecordsService.GetActualRecordPeriods(data.ExecutionPlaceId, data.BeginDateTime);
                var recordPeriods = await recordPeriodsQuery.Select(x => new CommonIdName() { Id = x.Id, Name = x.Name }).ToArrayAsync();
                RecordPeriods.AddRange(recordPeriods);
                SelectedPeriodId = data.RecordPeriodId.ToInt();
                SelectedUrgentlyId = data.UrgentlyId;
                BeginDateTime = data.BeginDateTime;
                EndDateTime = data.EndDateTime;
                //ChangeTracker.IsEnabled = true;
                loadingIsCompleted = true;
            }
            catch (OperationCanceledException)
            {
                //Do nothing. Cancelled operation means that user selected different patient before previous one was loaded
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to load data from visit template with Id {0}");
                //FailureMediator.Activate("Не удалость загрузить из шаблона случая. Попробуйте еще раз или обратитесь в службу поддержки", reloadVisitTemplateDataFillingCommandWrapper, ex);
                loadingIsCompleted = true;
            }
            finally
            {
                CommandManager.InvalidateRequerySuggested();
                if (loadingIsCompleted)
                {
                    BusyMediator.Deactivate();
                }
                if (recordQuery != null)
                {
                    recordQuery.Dispose();
                }
                if (assignmentQuery != null)
                {
                    assignmentQuery.Dispose();
                }
                if (recordPeriodsQuery != null)
                {
                    recordPeriodsQuery.Dispose();
                }
            }
        }

        private async void LoadProtocolEditor(int visitId, int assignmentId, int recordId)
        {
            var dataSourcesLoaded = await EnsureDataSourceLoaded();
            //ChangeTracker.IsEnabled = false;
            if (!dataSourcesLoaded)
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
            BusyMediator.Activate("Загрузка протокола...");
            logService.InfoFormat("Loading protocol editor...");
            IDisposableQueryable<Record> recordQuery = patientRecordsService.GetRecord(recordId);
            IDisposableQueryable<Assignment> assignmentQuery = patientRecordsService.GetAssignment(assignmentId);
            IDisposableQueryable<Visit> visitQuery = patientRecordsService.GetVisit(visitId);
            DateTime curDate = DateTime.Now;
            try
            {
                if (recordId > 0)
                {
                    var record = await recordQuery.FirstOrDefaultAsync(token);
                    ProtocolEditor = recordTypeEditorResolver.Resolve(record.RecordType.RecordTypeEditors.FirstOrDefault().Editor);
                }
                if (assignmentId > 0)
                {
                    var assignment = await assignmentQuery.FirstOrDefaultAsync(token);
                    ProtocolEditor = recordTypeEditorResolver.Resolve(assignment.RecordType.RecordTypeEditors.FirstOrDefault().Editor);
                }
                if (visitId > 0)
                {
                    var visit = await visitQuery.FirstOrDefaultAsync(token);
                    ProtocolEditor = recordTypeEditorResolver.Resolve("VisitProtocol");
                }
                //ChangeTracker.IsEnabled = true;
                loadingIsCompleted = true;
            }
            catch (OperationCanceledException)
            {
                //Do nothing. Cancelled operation means that user selected different patient before previous one was loaded
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to load data from visit template with Id {0}");
                //FailureMediator.Activate("Не удалость загрузить из шаблона случая. Попробуйте еще раз или обратитесь в службу поддержки", reloadVisitTemplateDataFillingCommandWrapper, ex);
                loadingIsCompleted = true;
            }
            finally
            {
                CommandManager.InvalidateRequerySuggested();
                if (loadingIsCompleted)
                {
                    BusyMediator.Deactivate();
                }
                if (recordQuery != null)
                {
                    recordQuery.Dispose();
                }
                if (assignmentQuery != null)
                {
                    assignmentQuery.Dispose();
                }
                if (visitQuery != null)
                {
                    visitQuery.Dispose();
                }
            }
        }

        private void UnsubscriveFromEvents()
        {
            eventAggregator.GetEvent<SelectionEvent<Visit>>().Subscribe(OnVisitSelected);
            eventAggregator.GetEvent<SelectionEvent<Assignment>>().Subscribe(OnAssignmentSelected);
            eventAggregator.GetEvent<SelectionEvent<Record>>().Subscribe(OnRecordSelected);
        }

        private void PrintProtocol()
        {
            if (ProtocolEditor != null)
                ProtocolEditor.PrintProtocol();
        }

        private void SaveProtocol()
        {
            if (ProtocolEditor != null)
                ProtocolEditor.SaveProtocol();
        }

        private void ShowProtocolInEditMode()
        {
            if (ProtocolEditor != null)
                protocolEditor.CurrentMode = ProtocolMode.Edit;
        }

        private void ShowProtocolInViewMode()
        {
            if (ProtocolEditor != null)
                protocolEditor.CurrentMode = ProtocolMode.View;
        }

        private void SetCurrentDateTimeEnd()
        {
            var dtNow = DateTime.Now;
            EndDateTime = new DateTime(dtNow.Year, dtNow.Month, dtNow.Day, dtNow.Hour, dtNow.Minute, 0);
        }

        private void SetCurrentDateTimeBegin()
        {
            var dtNow = DateTime.Now;
            BeginDateTime = new DateTime(dtNow.Year, dtNow.Month, dtNow.Day, dtNow.Hour, dtNow.Minute, 0);
        }

        void ProtocolEditor_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentMode")
            {
                OnPropertyChanged(() => IsViewModeInCurrentProtocolEditor);
                OnPropertyChanged(() => IsEditModeInCurrentProtocolEditor);
            }
        }



        #endregion

        #region Commands

        private DelegateCommand printProtocolCommand;
        public ICommand PrintProtocolCommand
        {
            get { return printProtocolCommand; }

        }

        private DelegateCommand saveProtocolCommand;
        public ICommand SaveProtocolCommand
        {
            get { return saveProtocolCommand; }

        }

        private DelegateCommand showInEditModeCommand;
        public ICommand ShowInEditModeCommand
        {
            get { return showInEditModeCommand; }

        }

        private DelegateCommand showInViewModeCommand;
        public ICommand ShowInViewModeCommand
        {
            get { return showInViewModeCommand; }

        }

        private DelegateCommand setCurrentDateTimeEndCommand;
        public ICommand SetCurrentDateTimeEndCommand
        {
            get { return setCurrentDateTimeEndCommand; }
        }

        private DelegateCommand setCurrentDateTimeBeginCommand;
        public ICommand SetCurrentDateTimeBeginCommand
        {
            get { return setCurrentDateTimeBeginCommand; }
        }

        #region RecordDocuments Commands

        public ICommand AttachDocumentCommand { get { return DocumentsViewer.AttachDocumentCommand; } }
        public ICommand DetachDocumentCommand { get { return DocumentsViewer.DetachDocumentCommand; } }
        public ICommand AttachDICOMCommand { get { return DocumentsViewer.AttachDICOMCommand; } }
        public ICommand DetachDICOMCommand { get { return DocumentsViewer.DetachDICOMCommand; } }

        #endregion

        #endregion

        #region Events

        void documentsViewer_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
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
    }
}
