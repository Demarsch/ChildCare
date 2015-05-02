using System;
using System.ComponentModel;
using GalaSoft.MvvmLight;

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
            patientSearchViewModel.PropertyChanged += PatientSearchViewModelOnPropertyChanged;
        }

        private void PatientSearchViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (string.IsNullOrEmpty(propertyChangedEventArgs.PropertyName) || propertyChangedEventArgs.PropertyName == "CurrentPatient")
                ScheduleViewModel.CurrentPatient = PatientSearchViewModel.CurrentPatient;
        }

        public PatientSearchViewModel PatientSearchViewModel { get; private set; }

        public ScheduleViewModel ScheduleViewModel { get; private set; }
    }
}
