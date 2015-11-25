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
    public class VisitProtocolViewModel :BindableBase, IRecordTypeProtocol
    {
        #region Constructors
        public VisitProtocolViewModel()
        {
            CurrentMode = ProtocolMode.View;
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

        public bool SaveProtocol()
        {
            return true;
        }

        public void LoadProtocol(int recordTypeId, int assignmentId, int recordId)
        {
            return;
        }

        #endregion
    }
}
