using Core;
using GalaSoft.MvvmLight;

namespace Commission
{
    public class CommissionManagementViewModel : ObservableObject
    {
        private ISimpleLocator locator;
        public CommissionManagementViewModel(ISimpleLocator simpleLocator)
        {
            locator = simpleLocator;
        }


    }
}