using Core.Data;
using Core.Wpf.Events;
using Core.Wpf.Misc;
using log4net;
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
using System.Threading.Tasks;
using System.Windows.Input;

namespace PatientRecordsModule.ViewModels
{
    public class PersonRecordEditorViewModel : BindableBase, IDisposable
    {
        #region Fields
        private readonly IPatientRecordsService patientRecordsService;

        private readonly ILog logService;

        private readonly IEventAggregator eventAggregator;
        #endregion

        #region Constructors
        public PersonRecordEditorViewModel(IPatientRecordsService patientRecordsService, ILog logSevice, IEventAggregator eventAggregator)
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

            printProtocolCommand = new DelegateCommand(PrintProtocol);
            saveProtocolCommand = new DelegateCommand(SaveProtocol);
            showInEditModeCommand = new DelegateCommand(ShowProtocolInEditMode);
            showInViewModeCommand = new DelegateCommand(ShowProtocolInViewMode);
            SubscribeToEvents();
        }
        #endregion

        #region Properties
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
                SetProperty(ref assignmentId, value);
                ProtocolEditor = new DefaultProtocolViewModel();
            }
        }

        private int recordId;
        public int RecordId
        {
            get { return recordId; }
            set
            {
                SetProperty(ref recordId, value);
                ProtocolEditor = new DefaultProtocolViewModel();
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
        #endregion
    }
}
