using Core.Wpf.Misc;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientRecordsModule.ViewModels.RecordTypesProtocolViewModels
{
    public class DefaultProtocolViewModel : TrackableBindableBase
    {
        #region Constructors
        public DefaultProtocolViewModel()
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
        
        #endregion
    }
}
