using GalaSoft.MvvmLight;

namespace Registry
{
    public class CurrentPatientAssignmentsViewModel : ObservableObject
    {
        public CurrentPatientAssignmentsViewModel()
        {
            showCurrentPatientAssignments = false;
        }

        private PersonViewModel currentPatient;

        public PersonViewModel CurrentPatient
        {
            get { return currentPatient; }
            set
            {
                Set("CurrentPatient", ref currentPatient, value);
                CanShowCurrentPatientAssignments = value != null;
            }
        }

        private bool showThatCurrentPatientHasNoAssignments;

        public bool ShowThatCurrentPatientHasNoAssignments
        {
            get { return showThatCurrentPatientHasNoAssignments; }
            set { Set("ShowThatCurrentPatientHasNoAssignments", ref showThatCurrentPatientHasNoAssignments, value); }
        }

        private bool canShowCurrentPatientAssignments;

        public bool CanShowCurrentPatientAssignments
        {
            get { return canShowCurrentPatientAssignments; }
            private set
            {
                Set("CanShowCurrentPatientAssignments", ref canShowCurrentPatientAssignments, value);
                if (!value)
                {
                    ShowCurrentPatientAssignments = false;
                }
            }
        }

        private bool showCurrentPatientAssignments;

        public bool ShowCurrentPatientAssignments
        {
            get { return showCurrentPatientAssignments; }
            set
            {
                Set("ShowCurrentPatientAssignments", ref showCurrentPatientAssignments, value);
                //TODO: this is just temporary
                ShowThatCurrentPatientHasNoAssignments = value;
                //if (!value)
                //{
                //    ShowThatCurrentPatientHasNoAssignments = false;
                //}
            }
        }
    }
}
