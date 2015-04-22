using GalaSoft.MvvmLight;

namespace Core
{
    public class BasicViewModel : ObservableObject
    {
        private string failReason;
        
        public string FailReason
        {
            get { return failReason; }
            set
            {
                var isFailed = IsFailed;
                if (Set("FailReason", ref failReason, value) && isFailed != IsFailed)
                    RaisePropertyChanged("IsFailed");
            }
        }

        public bool IsFailed { get { return !string.IsNullOrEmpty(failReason); } }

        private object busyStatus;

        public object BusyStatus
        {
            get { return busyStatus; }
            set
            {
                var isBusy = IsBusy;
                if (Set("BusyStatus", ref busyStatus, value) && isBusy != IsBusy)
                    RaisePropertyChanged("IsBusy");
            }
        }

        public bool IsBusy { get { return busyStatus != null; } }
    }
}
