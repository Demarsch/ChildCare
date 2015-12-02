using Core.Misc;
using Core.Wpf.Misc;
using PatientRecordsModule.Misc;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PatientRecordsModule.ViewModels.RecordTypesProtocolViewModels
{
    public class VisitProtocolViewModel : TrackableBindableBase, IRecordTypeProtocol
    {
        #region Constructors
        public VisitProtocolViewModel()
        {
            CurrentMode = ProtocolMode.View;

            ChangeTracker = new ChangeTrackerEx<VisitProtocolViewModel>(this);
            ChangeTracker.PropertyChanged += ChangeTracker_PropertyChanged;
        }

        void ChangeTracker_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
        }

        #endregion

        #region Properties
        #endregion

        #region Methods
        #endregion

        #region IRecordTypeProtocol implementation
        private ProtocolMode currentMode;
        public ProtocolMode CurrentMode
        {
            get { return currentMode; }
            set { SetProperty(ref currentMode, value); }
        }

        public void PrintProtocol()
        {
            return;
        }

        public int SaveProtocol(int recordId, int visitId)
        {
            return 0;
        }

        public void LoadProtocol(int assignmentId, int recordId, int visitId)
        {
            return;
        }

        #endregion
        
        public IChangeTracker ChangeTracker { get; set; }
    }
}
