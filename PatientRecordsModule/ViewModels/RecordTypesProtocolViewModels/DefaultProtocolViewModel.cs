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
    public class DefaultProtocolViewModel : TrackableBindableBase, IRecordTypeProtocol
    {
        #region Constructors
        public DefaultProtocolViewModel()
        {
            CurrentMode = ProtocolMode.View;
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

        #endregion

    }
}
