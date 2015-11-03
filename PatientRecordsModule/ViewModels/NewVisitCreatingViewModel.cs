using Core.Wpf.PopupWindowActionAware;
using Core.Wpf.Services;
using log4net;
using PatientRecordsModule.Services;
using Prism.Commands;
using Prism.Interactivity.InteractionRequest;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PatientRecordsModule.ViewModels
{
    public class NewVisitCreatingViewModel : Notification, INotifyPropertyChanged, IPopupWindowActionAware
    {
        #region Fields
        private readonly IPatientRecordsService patientRecordsService;
        private readonly ILog logService;
        #endregion

        #region Constructors
        public NewVisitCreatingViewModel(IPatientRecordsService patientRecordsService, ILog logService)
        {
            this.patientRecordsService = patientRecordsService;
            this.logService = logService;
            CreateVisitCommand = new DelegateCommand(CreateVisit);
            this.RaisePropertyChanged(String.Empty);
        }

        #endregion

        #region IPopupWindowActionAware implementation
        public System.Windows.Window HostWindow { get; set; }

        public INotification HostNotification { get; set; }
        #endregion

        #region Properties
        private int visitId;
        public int VisitId
        {
            get { return visitId; }
            set
            {
                visitId = value;
                RaisePropertyChanged("VisitId");
            }
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        // INotifyPropertyChange implementation
        protected virtual void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #region Commands
        public ICommand CreateVisitCommand { get; set; }
        private void CreateVisit()
        {
            VisitId = 1;
            HostWindow.Close();
        }
        #endregion
    }
}
