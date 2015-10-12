using GalaSoft.MvvmLight;

namespace Commission
{
    [PropertyChanged.ImplementPropertyChanged]
    public class CommissionPersonGridViewModel : ViewModelBase
    {
        public CommissionPersonGridViewModel()
        {
        }

        public void Load(dynamic e)
        {
            Title = e.GetType().ToString() + ": " + e.Id.ToString();
        }

        public string Title { get; set; }
    }
}