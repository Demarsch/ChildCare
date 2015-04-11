using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using log4net;

namespace Registry
{
    public class MainWindowViewModel : ObservableObject
    {
        public MainWindowViewModel(PatientSearchViewModel patientSearchViewModel, ScheduleViewModel scheduleViewModel)
        {
            if (patientSearchViewModel == null)
                throw new ArgumentNullException("patientSearchViewModel");
            if (scheduleViewModel == null)
                throw new ArgumentNullException("scheduleViewModel");
            ScheduleViewModel = scheduleViewModel;
            PatientSearchViewModel = patientSearchViewModel;
        }

        public PatientSearchViewModel PatientSearchViewModel { get; private set; }

        public ScheduleViewModel ScheduleViewModel { get; private set; }
    }
}
