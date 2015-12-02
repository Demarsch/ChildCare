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
using Core.Misc;
using System.ComponentModel;
using Core.Data.Services;
using Core.Data.Classes;

namespace PatientRecordsModule.ViewModels
{
    public class PersonRecordEditorViewModel : TrackableBindableBase, IDisposable, IDataErrorInfo, IChangeTrackerMediator
    {
        #region Fields
        private readonly IPatientRecordsService patientRecordsService;
        private readonly ILog logService;
        private readonly IRecordTypeEditorResolver recordTypeEditorResolver;
        private readonly IUserService userService;

        private readonly IEventAggregator eventAggregator;

        private readonly CommandWrapper reloadRecordBrigadeCommandWrapper;
        private readonly CommandWrapper reloadDataSourceCommandWrapper;
        private readonly CommandWrapper reloadRecordCommonDataCommandWrapper;
        private readonly CommandWrapper saveChangesCommandWrapper;
        private CancellationTokenSource currentOperationToken;

        private int recordTypeId = 0;
        private DateTime onDate = DateTime.MaxValue;
        private int personId = 0;
        #endregion

        #region Constructors
        public PersonRecordEditorViewModel(IPatientRecordsService patientRecordsService, IRecordTypeEditorResolver recordTypeEditorResolver, IUserService userService,
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
            if (eventAggregator == null)
            {
                throw new ArgumentNullException("eventAggregator");
            }
            if (recordTypeEditorResolver == null)
            {
                throw new ArgumentNullException("recordTypeEditorResolver");
            }
            if (userService == null)
            {
                throw new ArgumentNullException("userService");
            }
            this.userService = userService;
            this.recordTypeEditorResolver = recordTypeEditorResolver;
            this.patientRecordsService = patientRecordsService;
            this.logService = logSevice;
            this.eventAggregator = eventAggregator;

            this.documentsViewer = documentsViewer;
            this.documentsViewer.PropertyChanged += documentsViewer_PropertyChanged;

            ChangeTracker = new ChangeTrackerEx<PersonRecordEditorViewModel>(this);
            ChangeTracker.PropertyChanged += OnChangesTracked;

            reloadRecordBrigadeCommandWrapper = new CommandWrapper() { Command = new DelegateCommand(() => LoadBrigadeAsync(recordTypeId, RecordId, onDate)) };
            reloadRecordCommonDataCommandWrapper = new CommandWrapper() { Command = new DelegateCommand(() => LoadProtocolCommonData(VisitId, AssignmentId, RecordId)) };
            reloadDataSourceCommandWrapper = new CommandWrapper { Command = new DelegateCommand(() => LoadDataSources(onDate)), CommandName = "Повторить" };
            saveChangesCommandWrapper = new CommandWrapper { Command = new DelegateCommand(() => SaveCommonData()), CommandName = "Повторить" };

            BusyMediator = new BusyMediator();
            FailureMediator = new FailureMediator();
            Brigade = new ObservableCollectionEx<BrigadeViewModel>();
            Urgentlies = new ObservableCollectionEx<CommonIdName>();
            RecordPeriods = new ObservableCollectionEx<CommonIdName>();
            ParentVisits = new ObservableCollectionEx<CommonIdName>();
            Rooms = new ObservableCollectionEx<CommonIdName>();

            printProtocolCommand = new DelegateCommand(PrintProtocol);
            saveProtocolCommand = new DelegateCommand(SaveProtocol, CanSaveChanges);
            showInEditModeCommand = new DelegateCommand(ShowProtocolInEditMode);
            showInViewModeCommand = new DelegateCommand(ShowProtocolInViewMode);
            setCurrentDateTimeEndCommand = new DelegateCommand(SetCurrentDateTimeEnd);
            setCurrentDateTimeBeginCommand = new DelegateCommand(SetCurrentDateTimeBegin);
            SubscribeToEvents();

            currentInstanceChangeTracker = new ChangeTrackerEx<PersonRecordEditorViewModel>(this);    
        
        }

        private readonly IChangeTracker currentInstanceChangeTracker;
        public IChangeTracker ChangeTracker { get; private set; }

        private void OnChangesTracked(object sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.PropertyName) || string.CompareOrdinal(e.PropertyName, "HasChanges") == 0)
            {
                UpdateChangeCommandsState();
            }
        }

        private void UpdateChangeCommandsState()
        {
            saveProtocolCommand.RaiseCanExecuteChanged();
        }

        private bool CanSaveChanges()
        {           
            return ChangeTracker != null && ChangeTracker.HasChanges;
        }

        #endregion

        #region Properties      

        public ObservableCollectionEx<BrigadeViewModel> Brigade { get; set; }

        private string recordTypeName;
        public string RecordTypeName
        {
            get { return recordTypeName; }
            set
            {
                SetProperty(ref recordTypeName, value);
            }
        }

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

        public ObservableCollectionEx<CommonIdName> ParentVisits { get; set; }

        public ObservableCollectionEx<CommonIdName> Rooms { get; set; }

        private int selectedPeriodId;
        public int SelectedPeriodId
        {
            get { return selectedPeriodId; }
            set { SetTrackedProperty(ref selectedPeriodId, value); }
        }

        private int selectedUrgentlyId;
        public int SelectedUrgentlyId
        {
            get { return selectedUrgentlyId; }
            set { SetTrackedProperty(ref selectedUrgentlyId, value); }
        }

        private DateTime? beginDateTime;
        public DateTime? BeginDateTime
        {
            get { return beginDateTime; }
            set { SetTrackedProperty(ref beginDateTime, value); }
        }

        private DateTime? endDateTime;
        public DateTime? EndDateTime
        {
            get { return endDateTime; }
            set { SetTrackedProperty(ref endDateTime, value); }
        }

        private int? roomId;
        public int? RoomId
        {
            get { return roomId; }
            set { SetTrackedProperty(ref roomId, value); }
        }

        private int? parentVisitId;
        public int? ParentVisitId
        {
            get { return parentVisitId; }
            set { SetTrackedProperty(ref parentVisitId, value); }
        }

        public bool IsVisibleVisits
        {
            get { return userService.HasPermission(PermissionType.ChangeRecordParentVisit) || ParentVisitId.ToInt() < 1; }
        }

        public bool IsVisibleRooms
        {
            get { return userService.HasPermission(PermissionType.ChangeRecordRoom) || RoomId.ToInt() < 1; }
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

        private void SetRVAIds(int visitId, int assignmentId, int recordId)
        {
            VisitId = visitId;
            AssignmentId = assignmentId;
            RecordId = recordId;
            recordTypeId = 0;
            LoadProtocolEditor(visitId, assignmentId, recordId);            
        }


        private async Task<bool> LoadDataSources(DateTime onDate)
        {
            Urgentlies.Clear();
            Rooms.Clear();
            BusyMediator.Activate("Загрузка общих данных в редактор...");
            logService.Info("Loading data sources for common record editor...");
            IDisposableQueryable<Urgently> urgentliesQuery = null;
            IDisposableQueryable<Room> roomsQuery = null;
            this.onDate = onDate;
            var dataloaded = false;
            try
            {
                urgentliesQuery = patientRecordsService.GetActualUrgentlies(onDate);

                var urgentlies = await urgentliesQuery.Select(x => new CommonIdName()
                {
                    Id = x.Id,
                    Name = x.Name
                }).ToListAsync();
                Urgentlies.AddRange(urgentlies);

                roomsQuery = patientRecordsService.GetRooms(onDate);
                var rooms = await roomsQuery.Select(x => new CommonIdName()
                {
                    Id = x.Id,
                    Name = x.Name
                }).ToListAsync();
                Rooms.AddRange(rooms);

                logService.InfoFormat("Data sources for common record editor are successfully loaded");
                dataloaded = true;
                this.onDate = DateTime.MaxValue;
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to load data sources for common record editor");
                FailureMediator.Activate("Не удалость загрузить данные для общих данных редактора. Попробуйте еще раз или обратитесь в службу поддержки", reloadDataSourceCommandWrapper, ex);
                dataloaded = false;
            }
            finally
            {
                if (urgentliesQuery != null)
                {
                    urgentliesQuery.Dispose();
                }
                if (roomsQuery != null)
                {
                    roomsQuery.Dispose();
                }
                BusyMediator.Deactivate();
            }
            return dataloaded;
        }

        private async void LoadBrigadeAsync(int recordTypeId, int recordId, DateTime onDate)
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
            IDisposableQueryable<RecordMember> recordMembers = null;
            IDisposableQueryable<RecordTypeRolePermission> recordTypeRolePermission = null;
            try
            {
                recordMembers = patientRecordsService.GetRecordMembers(recordId);
                recordTypeRolePermission = patientRecordsService.GetRecordTypeMembers(recordTypeId, onDate);
                var recordMembersTask = recordMembers.Select(x => new
                {
                    PersonName = x.PersonStaff.Person.ShortName,
                    PersonId = x.PersonStaff.PersonId,
                    StaffName = x.PersonStaff.Staff.ShortName,
                    x.RecordTypeRolePermissionId,
                    x.PersonStaffId,
                    RecordMemberId = x.Id,
                    x.RecordId
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
                    RecordId = recordMembersresult.Any(y => y.RecordTypeRolePermissionId == x.RecordTypeRolePermissionId) ?
                       recordMembersresult.FirstOrDefault(y => y.RecordTypeRolePermissionId == x.RecordTypeRolePermissionId).RecordId : 0,
                    RecordMemberId = recordMembersresult.Any(y => y.RecordTypeRolePermissionId == x.RecordTypeRolePermissionId) ?
                        recordMembersresult.FirstOrDefault(y => y.RecordTypeRolePermissionId == x.RecordTypeRolePermissionId).RecordMemberId : 0,
                    PersonName = recordMembersresult.Any(y => y.RecordTypeRolePermissionId == x.RecordTypeRolePermissionId) ?
                        recordMembersresult.FirstOrDefault(y => y.RecordTypeRolePermissionId == x.RecordTypeRolePermissionId).PersonName : string.Empty,
                    StaffName = recordMembersresult.Any(y => y.RecordTypeRolePermissionId == x.RecordTypeRolePermissionId) ?
                        recordMembersresult.FirstOrDefault(y => y.RecordTypeRolePermissionId == x.RecordTypeRolePermissionId).StaffName : string.Empty,
                    PersonStaffId = recordMembersresult.Any(y => y.RecordTypeRolePermissionId == x.RecordTypeRolePermissionId) ?
                        recordMembersresult.FirstOrDefault(y => y.RecordTypeRolePermissionId == x.RecordTypeRolePermissionId).PersonStaffId : 0,
                    RecordTypeId = recordTypeId,
                    OnDate = onDate
                })).ToList());
                loadingIsCompleted = true;
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
                if (recordMembers != null)
                {
                    recordMembers.Dispose();
                }
                if (recordTypeRolePermission != null)
            currentInstanceChangeTracker.PropertyChanged -= OnChangesTracked;
            reloadDataSourceCommandWrapper.Dispose();
            reloadRecordBrigadeCommandWrapper.Dispose();
                {
                    recordTypeRolePermission.Dispose();
                }
            }
        }

        private async void LoadProtocolCommonData(int visitId, int assignmentId, int recordId)
        {
            RecordPeriods.Clear();
            ParentVisits.Clear();
            var changeTracker = new CompositeChangeTracker(currentInstanceChangeTracker, ProtocolEditor.ChangeTracker);
            changeTracker.PropertyChanged += OnChangesTracked;
            ChangeTracker = changeTracker; 

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
            IDisposableQueryable<Visit> recordVisitsQuery = null;
            DateTime curDate = DateTime.Now;
            try
            {
                CommonRecordAssignmentDTO data = new CommonRecordAssignmentDTO();
                if (recordId > 0)
                {
                    data = await recordQuery.Select(x => new CommonRecordAssignmentDTO()
                        {
                            ParentVisitId = x.VisitId,
                            RecordTypeId = x.RecordTypeId,
                            PersonId = x.PersonId,
                            ExecutionPlaceId = x.Visit.ExecutionPlaceId,
                            RecordPeriodId = x.RecordPeriodId,
                            ActualDateTime = x.ActualDateTime,
                            BeginDateTime = x.BeginDateTime,
                            EndDateTime = x.EndDateTime,
                            UrgentlyId = x.UrgentlyId,
                            RoomId = x.RoomId,
                            RecordTypeName = x.RecordType.Name + (x.RecordType.Code != null && x.RecordType.Code != string.Empty ? " (" + x.RecordType.Code + ")" : string.Empty)
                        }).FirstOrDefaultAsync(token);
                    LoadBrigadeAsync(data.RecordTypeId, recordId, data.ActualDateTime);
                }
                else if (assignmentId > 0)
                {
                    data = await assignmentQuery.Select(x => new CommonRecordAssignmentDTO()
                    {
                        ParentVisitId = x.VisitId,
                        RecordTypeId = x.RecordTypeId,
                        PersonId = x.PersonId,
                        ExecutionPlaceId = x.ExecutionPlaceId,// x.VisitId.HasValue ? x.Visit.ExecutionPlaceId
                        BeginDateTime = x.AssignDateTime,
                        ActualDateTime = x.AssignDateTime,
                        UrgentlyId = x.UrgentlyId,
                        RoomId = x.RoomId,
                        RecordTypeName = x.RecordType.Name + (x.RecordType.Code != null && x.RecordType.Code != string.Empty ? " (" + x.RecordType.Code + ")" : string.Empty)
                    }).FirstOrDefaultAsync(token);
                    LoadBrigadeAsync(data.RecordTypeId, 0, data.ActualDateTime);
                }
                //Load recordPeriods
                recordPeriodsQuery = patientRecordsService.GetActualRecordPeriods(data.ExecutionPlaceId, data.BeginDateTime);
                var recordPeriods = await recordPeriodsQuery.Select(x => new CommonIdName() { Id = x.Id, Name = x.Name }).ToArrayAsync();
                RecordPeriods.AddRange(recordPeriods);
                //Load Person Visits
                recordVisitsQuery = patientRecordsService.GetPersonVisitsQuery(data.PersonId, !userService.HasPermission(PermissionType.ChangeRecordParentVisit));
                var visits = await recordVisitsQuery.Select(x => new { x.Id, x.BeginDateTime, x.EndDateTime, x.VisitTemplate.Name }).ToArrayAsync();
                ParentVisits.AddRange(visits.Select(x => new CommonIdName()
                {
                    Id = x.Id,
                    Name = x.Name + " (" + x.BeginDateTime.ToString("dd.MM.yyyy HH:mm") + " - " +
                        (x.EndDateTime.HasValue ? x.EndDateTime.Value.ToString("dd.MM.yyyy HH:mm") : "...") + ")"
                }));

                SelectedPeriodId = data.RecordPeriodId.ToInt();
                SelectedUrgentlyId = data.UrgentlyId;
                BeginDateTime = data.BeginDateTime;
                EndDateTime = data.EndDateTime;
                personId = data.PersonId;
                recordTypeId = data.RecordTypeId;
                ParentVisitId = data.ParentVisitId;
                OnPropertyChanged(() => IsVisibleVisits);
                RoomId = data.RoomId;
                OnPropertyChanged(() => IsVisibleRooms);
                RecordTypeName = data.RecordTypeName;
                ChangeTracker.IsEnabled = true;
                loadingIsCompleted = true;
            }
            catch (OperationCanceledException)
            {
                //Do nothing. Cancelled operation means that user selected different patient before previous one was loaded
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to load record common data");
                FailureMediator.Activate("Не удалость загрузить общие данные. Попробуйте еще раз или обратитесь в службу поддержки", reloadRecordCommonDataCommandWrapper, ex);
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
                if (recordVisitsQuery != null)
                {
                    recordVisitsQuery.Dispose();
                }
            }
        }

        private async void LoadProtocolEditor(int visitId, int assignmentId, int recordId)
        {         
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
                    var record = await recordQuery.Select(x => new { x.ActualDateTime, RecordTypeEditor = x.RecordType.RecordTypeEditors.Any() ? x.RecordType.RecordTypeEditors.FirstOrDefault().Editor : string.Empty }).FirstOrDefaultAsync(token);
                    var res = await LoadDataSources(record.ActualDateTime);
                    if (!res) return;
                    ProtocolEditor = recordTypeEditorResolver.Resolve(record.RecordTypeEditor);
                }
                if (assignmentId > 0)
                {
                    var assignmentEditor = await assignmentQuery.Select(x => x.RecordType.RecordTypeEditors.Any() ? x.RecordType.RecordTypeEditors.FirstOrDefault().Editor : string.Empty).FirstOrDefaultAsync(token);
                    var res = await LoadDataSources(curDate);
                    if (!res) return;
                    ProtocolEditor = recordTypeEditorResolver.Resolve(assignmentEditor);
                }
                if (visitId > 0)
                {
                    var visit = await visitQuery.FirstOrDefaultAsync(token);
                    ProtocolEditor = recordTypeEditorResolver.Resolve("VisitProtocol");
                }

                //Load Documents
                DocumentsViewer.LoadDocuments(assignmentId, recordId);                
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

        private async void SaveCommonData()
        {
            FailureMediator.Deactivate();
            if (!IsValid)
            {
                return;
            }
            if (currentOperationToken != null)
            {
                currentOperationToken.Cancel();
                currentOperationToken.Dispose();
            }
            currentOperationToken = new CancellationTokenSource();
            var token = currentOperationToken.Token;
            string recordIdString = RecordId < 1 ? RecordId.ToString() : "(new record)";
            logService.InfoFormat("Saving record common data Id = {0}", recordIdString);
            BusyMediator.Activate("Сохранение изменений...");
            var saveSuccesfull = false;
            try
            {
                var brigade = Brigade.Where(x => x.IsPersonMember).Select(x => x.GetRecordMember()).ToList();
                RecordId = await patientRecordsService.SaveRecordCommonDataAsync(RecordId, recordTypeId, personId, ParentVisitId.Value, RoomId.Value, SelectedPeriodId, SelectedUrgentlyId, BeginDateTime.Value, EndDateTime.Value, brigade, token);
                int protocolId = SpecialValues.NonExistingId;
                if (ProtocolEditor != null)
                    protocolId = ProtocolEditor.SaveProtocol(RecordId, VisitId);
                saveSuccesfull = !SpecialValues.IsNewOrNonExisting(protocolId);
            }
            catch (OperationCanceledException)
            {
                //Nothing to do as it means that we somehow cancelled save operation
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to save data while closing visit for visit with Id = {0}", recordIdString);
                FailureMediator.Activate("Не удалось сохранить данные. Попробуйте еще раз или обратитесь в службу поддержки", saveChangesCommandWrapper, ex, true);
            }
            finally
            {
                BusyMediator.Deactivate();
                if (saveSuccesfull)
                {
                    ChangeTracker.AcceptChanges();
                    ChangeTracker.IsEnabled = true;
                    //changeTracker.UntrackAll();
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
            // if (CanSaveChanges())
            SaveCommonData();
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

        public ICommand SaveRecordCommand { get; private set; }

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

        #region IDataErrorInfo implementation
        private bool saveWasRequested;

        private readonly HashSet<string> invalidProperties = new HashSet<string>();

        private bool IsValid
        {
            get
            {
                saveWasRequested = true;
                OnPropertyChanged(string.Empty);
                return invalidProperties.Count < 1;
            }
        }

        string IDataErrorInfo.this[string columnName]
        {
            get
            {
                if (!saveWasRequested)
                {
                    invalidProperties.Remove(columnName);
                    return string.Empty;
                }
                var result = string.Empty;
                switch (columnName)
                {
                    case "selectedPeriodId":
                        result = SelectedPeriodId == null ? "Не указан период выполнения" : string.Empty;
                        break;
                    case "selectedUrgentlyId":
                        result = SelectedUrgentlyId == null ? "Не указана форма оказания помощи" : string.Empty;
                        break;
                    case "parentVisitId":
                        result = ParentVisitId == null ? "Не указан родительский случай" : string.Empty;
                        break;
                    case "roomId":
                        result = RoomId == null ? "Не указан кабинет" : string.Empty;
                        break;
                }
                if (string.IsNullOrEmpty(result))
                {
                    invalidProperties.Remove(columnName);
                }
                else
                {
                    invalidProperties.Add(columnName);
                }
                return result;
            }
        }

        string IDataErrorInfo.Error
        {
            get { throw new NotImplementedException(); }
        }
        #endregion
    }
}
