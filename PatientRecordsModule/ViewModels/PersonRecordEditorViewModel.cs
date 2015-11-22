using Core.Data;
using Core.Data.Misc;
using Core.Wpf.Events;
using Core.Wpf.Misc;
using Core.Wpf.Mvvm;
using log4net;
using PatientRecordsModule.DTO;
using PatientRecordsModule.Misc;
using PatientRecordsModule.Services;
using PatientRecordsModule.ViewModels.RecordTypesProtocolViewModels;
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

namespace PatientRecordsModule.ViewModels
{
    public class PersonRecordEditorViewModel : BindableBase, IDisposable
    {
        #region Fields
        private readonly IPatientRecordsService patientRecordsService;
        private readonly ILog logService;

        private readonly IEventAggregator eventAggregator;

        private readonly CommandWrapper reloadRecordBrigadeCommandWrapper;
        private CancellationTokenSource currentOperationToken;
       
        #endregion

        #region Constructors
        public PersonRecordEditorViewModel(IPatientRecordsService patientRecordsService,
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
            this.patientRecordsService = patientRecordsService;
            this.logService = logSevice;
            this.eventAggregator = eventAggregator;

            this.documentsViewer = documentsViewer;
            this.documentsViewer.PropertyChanged += documentsViewer_PropertyChanged;

            reloadRecordBrigadeCommandWrapper = new CommandWrapper() { Command = new DelegateCommand(() => LoadBrigadeAsync(RecordId)) };

            BusyMediator = new BusyMediator();
            FailureMediator = new FailureMediator();
            Brigade = new ObservableCollectionEx<BrigadeDTO>();
            
            printProtocolCommand = new DelegateCommand(PrintProtocol);
            saveProtocolCommand = new DelegateCommand(SaveProtocol);
            showInEditModeCommand = new DelegateCommand(ShowProtocolInEditMode);
            showInViewModeCommand = new DelegateCommand(ShowProtocolInViewMode);
            SubscribeToEvents();
        }

        #endregion

        #region Properties
        
        public ObservableCollectionEx<BrigadeDTO> Brigade { get; set; }

        private int visitId;
        public int VisitId
        {
            get { return visitId; }
            set
            {
                SetProperty(ref visitId, value);
                ProtocolEditor = new DefaultProtocolViewModel();
            }
        }

        private int assignmentId;
        public int AssignmentId
        {
            get { return assignmentId; }
            set
            {
                if (SetProperty(ref assignmentId, value))
                {
                    ProtocolEditor = new DefaultProtocolViewModel();
                }
            }
        }

        private int recordId;
        public int RecordId
        {
            get { return recordId; }
            set
            {
                if (SetProperty(ref recordId, value))
                {
                    ProtocolEditor = new DefaultProtocolViewModel();            
                }
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

                ProtocolEditor.PropertyChanged += ProtocolEditor_PropertyChanged;
                if (RecordId > 0)
                    LoadBrigadeAsync(RecordId);
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

        private async void LoadBrigadeAsync(int recordId)
        {
            FailureMediator.Deactivate();
            if (recordId < 1) return;
            var loadingIsCompleted = false;
            currentOperationToken = new CancellationTokenSource();
            var token = currentOperationToken.Token;
            BusyMediator.Activate(string.Empty);
            logService.InfoFormat("Loading brigade for record with Id ={0}", recordId);
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
                    x.PersonStaffId
                }).ToListAsync(token);
                var recordTypeMembersTask = recordTypeRolePermission.Select(x => new
                {
                    RoleName = x.RecordTypeRole.Name,
                    RoleId = x.RecordTypeMemberRoleId,
                    x.IsRequired,
                    PermissionName = x.Permission.Name,
                    PermissionId = x.PermissionId
                }).ToListAsync(token);
                await Task.WhenAll(recordMembersTask, recordTypeMembersTask);
                Brigade.AddRange(recordTypeMembersTask.Result.Select(x => new BrigadeDTO()
                {
                    IsRequired = x.IsRequired,
                    RoleName = x.RoleName,
                    RoleId = x.RoleId,
                    PermissionId = x.PermissionId
                }
                ));
                loadingIsCompleted = true;
            }
            catch (OperationCanceledException)
            {
                //Do nothing. Cancelled operation means that user selected different patient before previous one was loaded
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to load brigade for record with Id ={0}", recordId);
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

            await DocumentsViewer.LoadDocuments(assignmentId, recordId);
                        
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
