using Core.Data.Misc;
using Core.Data.Services;
using Core.Misc;
using Core.Services;
using Core.Wpf.Misc;
using Core.Wpf.Services;
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
    public class DefaultProtocolViewModel : TrackableBindableBase, IRecordTypeProtocol, IChangeTrackerMediator
    {
        private readonly IDiagnosService diagnosService;
        private readonly IRecordService recordService;
        private readonly ILog logService;

        #region Constructors
        public DefaultProtocolViewModel(IDiagnosService diagnosService, IRecordService recordService, ILog logService,
                                        ICacheService cacheService, IDialogServiceAsync dialogService, IDialogService messageService, IUserService userService)
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
            DiagnosesEditor = new DiagnosesCollectionViewModel(diagnosService, recordService, cacheService, dialogService, messageService, logService, userService);  
     
            currentInstanceChangeTracker = new ChangeTrackerEx<DefaultProtocolViewModel>(this);
            var changeTracker = new CompositeChangeTracker(currentInstanceChangeTracker, DiagnosesEditor.ChangeTracker);
            changeTracker.PropertyChanged += OnChangesTracked;
            ChangeTracker = changeTracker;
        }
        #endregion

        private readonly IChangeTracker currentInstanceChangeTracker;

        public IChangeTracker ChangeTracker { get; set; }

        public void Dispose()
        {
            currentInstanceChangeTracker.PropertyChanged -= OnChangesTracked;
        }

        private void OnChangesTracked(object sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.PropertyName) || string.CompareOrdinal(e.PropertyName, "HasChanges") == 0)
            {
                
            }
        }            

        #region Properties

        private string discription;
        public string Discription
        {
            get { return discription; }
            set { SetTrackedProperty(ref discription, value); }
        }

        private string result;
        public string Result
        {
            get { return result; }
            set { SetTrackedProperty(ref result, value); }
        }

        public bool IsEditMode
        {
            get { return CurrentMode == ProtocolMode.Edit; }
        }

        public bool IsViewMode
        {
            get { return CurrentMode == ProtocolMode.View; }
        }

        public DiagnosesCollectionViewModel DiagnosesEditor { get; private set; }

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

        public bool SaveProtocol(int recordId, int visitId)
        {
            bool saveIsSuccessful = DiagnosesEditor.Save(recordId);

            if (saveIsSuccessful)
            {
                ChangeTracker.AcceptChanges();
                ChangeTracker.IsEnabled = true;
            }
            
            return saveIsSuccessful;
        }

        public void LoadProtocol(int assignmentId, int recordId, int visitId)
        {
            ChangeTracker.IsEnabled = false;
            if (assignmentId > 0)
            {
                LoadAssignmentData(assignmentId);
            }
            else if (recordId > 0)
            {
                LoadRecordData(recordId);                
            }
            else if (visitId > 0)
                LoadVisitData(visitId);

            DiagnosesEditor.Load(OptionValues.DiagnosSpecialistExamination, recordId);

            ChangeTracker.IsEnabled = true;
        }

        private void LoadVisitData(int visitId)
        {
            Discription = string.Empty;
            Result = string.Empty;
            return;
        }

        private void LoadRecordData(int recordId)
        {
            Discription = string.Empty;
            Result = string.Empty;
            return;
        }

        private void LoadAssignmentData(int assignmentId)
        {
            Discription = string.Empty;
            Result = string.Empty;
            return;
        }

        #endregion
    }
}
