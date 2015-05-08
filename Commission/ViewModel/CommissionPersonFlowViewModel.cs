using Core;
using GalaSoft.MvvmLight;

namespace Commission
{
    [PropertyChanged.ImplementPropertyChanged]
    public class CommissionPersonFlowViewModel : ViewModelBase
    {
        public CommissionPersonFlowViewModel()
        {
        }

        public void Navigate(dynamic e)
        {
            Title = e.GetType().ToString() + ": " + e.Id.ToString();
        }

        public string Title { get; set; }
    }
}