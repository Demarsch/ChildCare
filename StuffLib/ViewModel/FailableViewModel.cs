using GalaSoft.MvvmLight;

namespace Core
{
    public class FailableViewModel : ObservableObject
    {
        protected string failReason;
        
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
    }
}
