using Core.Wpf.Misc;
using log4net;
using PatientRecordsModule.Misc;
using PatientRecordsModule.Services;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PatientRecordsModule.ViewModels.RecordTypesProtocolViewModels
{
    public class DefaultProtocolViewModel : TrackableBindableBase, IRecordTypeProtocol
    {
        private readonly IDiagnosService diagnosService;
        private readonly IRecordService recordService;
        private readonly ILog logService;

        #region Constructors
        public DefaultProtocolViewModel(IDiagnosService diagnosService, IRecordService recordService, ILog logService)
        {
            if (logService == null)
            {
                throw new ArgumentNullException("logService");
            }
            if (diagnosService == null)
            {
                throw new ArgumentNullException("diagnosService");
            }
            if (recordService == null)
            {
                throw new ArgumentNullException("recordService");
            }
            this.diagnosService = diagnosService;
            this.recordService = recordService;
            this.logService = logService;

            CurrentMode = ProtocolMode.View;
            this.diagnosesEditor = new DiagnosesCollectionViewModel(diagnosService, recordService, logService);
            this.diagnosesEditor.PropertyChanged += diagnoses_PropertyChanged;         
        }

        void diagnoses_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {

        }

        #endregion

        #region Properties
        private string discription;
        public string Discription
        {
            get { return discription; }
            set { SetProperty(ref discription, value); }
        }

        private string result;
        public string Result
        {
            get { return result; }
            set { SetProperty(ref result, value); }
        }

        public bool IsEditMode
        {
            get { return CurrentMode == ProtocolMode.Edit; }
        }

        public bool IsViewMode
        {
            get { return CurrentMode == ProtocolMode.View; }
        }

        private DiagnosesCollectionViewModel diagnosesEditor;
        public DiagnosesCollectionViewModel DiagnosesEditor
        {
            get { return diagnosesEditor; }
            set { SetProperty(ref diagnosesEditor, value); }
        }
        #endregion

        #region Methods

        #endregion

        #region IRecordTypeProtocol implementation
        private ProtocolMode currentMode;
        public ProtocolMode CurrentMode
        {
            get { return currentMode; }
            set
            {
                SetProperty(ref currentMode, value);
                OnPropertyChanged(() => IsEditMode);
                OnPropertyChanged(() => IsViewMode);
            }
        }

        public void PrintProtocol()
        {
            int i = 0;
        }

        public bool SaveProtocol()
        {
            return true;
        }

        public void LoadProtocol(int assignmentId, int recordId, int visitId)
        {
            if (assignmentId > 0)
                LoadAssignmentData(assignmentId);
            else if (recordId > 0)
            {
                LoadRecordData(recordId);
                DiagnosesEditor.Load(recordId); 
            }
            else if (visitId > 0)
                LoadVisitData(visitId);
        }

        private void LoadVisitData(int visitId)
        {
            return;
        }

        private void LoadRecordData(int recordId)
        {
            return;
        }

        private void LoadAssignmentData(int assignmentId)
        {
            return;
        }

        #endregion
    }
}
