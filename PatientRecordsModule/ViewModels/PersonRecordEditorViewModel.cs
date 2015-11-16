using Core.Data;
using Core.Wpf.Events;
using Core.Wpf.Misc;
using log4net;
using PatientRecordsModule.Services;
using PatientRecordsModule.ViewModels.RecordTypesProtocolViewModels;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                if (SetProperty(ref visitId, value))
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

        private TrackableBindableBase protocolEditor;
        public TrackableBindableBase ProtocolEditor
        {
            get { return protocolEditor; }
            set { SetProperty(ref protocolEditor, value); }
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
        #endregion
    }
}
