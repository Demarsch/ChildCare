using System;

namespace PatientInfoModule.Misc
{
    public class CurrentPatientHasRelativeCheckEventArgs : EventArgs
    {
        public CurrentPatientHasRelativeCheckEventArgs(int relativeId)
        {
            RelativeId = relativeId;
        }

        public int RelativeId { get; private set; }

        public bool HasThisRelative { get; set; }
    }
}
