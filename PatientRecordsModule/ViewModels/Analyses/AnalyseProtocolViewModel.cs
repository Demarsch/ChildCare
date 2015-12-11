using Core.Data;
using Core.Data.Misc;
using Core.Data.Services;
using Core.Misc;
using Core.Services;
using Core.Wpf.Misc;
using Core.Wpf.Services;
using log4net;
using Shared.PatientRecords.Misc;
using Shared.PatientRecords.Services;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Shared.PatientRecords.ViewModels
{
    public class AnalyseProtocolViewModel : TrackableBindableBase, IRecordTypeProtocol, IChangeTrackerMediator, IDataErrorInfo
    {
        private readonly IPatientRecordsService recordService;
        private readonly ILog logService;

        #region Constructors
        public AnalyseProtocolViewModel(IPatientRecordsService recordService, ILog logService, IDialogServiceAsync dialogService, IDialogService messageService, IUserService userService)
        {
            if (logService == null)
            {
                throw new ArgumentNullException("logService");
            }
            if (recordService == null)
            {
                throw new ArgumentNullException("recordService");
            }
            if (recordService == null)
            {
                throw new ArgumentNullException("recordService");
            }
            this.recordService = recordService;
            this.logService = logService;

            CurrentMode = ProtocolMode.View;

            currentInstanceChangeTracker = new ChangeTrackerEx<AnalyseProtocolViewModel>(this);
            var changeTracker = new CompositeChangeTracker(currentInstanceChangeTracker);
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

        private string description;
        public string Description
        {
            get { return description; }
            set { SetTrackedProperty(ref description, value); }
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

        public int SaveProtocol(int recordId, int visitId)
        {
            if (recordId == SpecialValues.NewId || !IsValid)
                return SpecialValues.NonExistingId;

            var defaultProtocol = recordService.GetRecord(recordId).First().DefaultProtocols.FirstOrDefault();
            if (defaultProtocol == null)
                defaultProtocol = new DefaultProtocol() { RecordId = recordId };

            defaultProtocol.Description = Description;
            defaultProtocol.Conclusion = Result;
            int saveProtocolId = recordService.SaveDefaultProtocol(defaultProtocol);

            if (!SpecialValues.IsNewOrNonExisting(saveProtocolId) && DiagnosesEditor.Save(recordId))
            {
                ChangeTracker.AcceptChanges();
                ChangeTracker.IsEnabled = true;
                return saveProtocolId;
            }
            return SpecialValues.NonExistingId;
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
            ChangeTracker.IsEnabled = true;
        }

        public string CanComplete()
        {
            string resStr = string.Empty;
            return resStr;
        }

        private void LoadVisitData(int visitId)
        {
            ClearProtocolData();
            return;
        }

        private void LoadRecordData(int recordId)
        {
            var protocol = recordService.GetRecord(recordId).First().DefaultProtocols.FirstOrDefault();
            if (protocol != null)
            {
                Description = protocol.Description;
                Result = protocol.Conclusion;
            }
            else
                ClearProtocolData();
            return;
        }

        private void LoadAssignmentData(int assignmentId)
        {
            ClearProtocolData();
            return;
        }

        private void ClearProtocolData()
        {
            Description = string.Empty;
            Result = string.Empty;
        }

        #endregion

        #region Implement IDataError

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
                    case "Description":
                        result = string.IsNullOrEmpty(Description.Trim()) ? "Не заполнено поле 'Описание'" : string.Empty;
                        break;
                    case "Result":
                        result = string.IsNullOrEmpty(Result.Trim()) ? "Не заполнено поле 'Заключение'" : string.Empty;
                        break;
                }
                if (string.IsNullOrEmpty(result))
                    invalidProperties.Remove(columnName);
                else
                    invalidProperties.Add(columnName);
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
