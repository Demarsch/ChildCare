using Core;
using GalaSoft.MvvmLight;
using System;

namespace Commission
{
    [PropertyChanged.ImplementPropertyChanged]
    public class CommissionMainViewModel : ViewModelBase
    {
        public CommissionMainViewModel(CommissionManagementViewModel Management, CommissionWorkViewModel Work)
        {
            this.Management = Management;
            this.Work = Work;
        }

        public CommissionManagementViewModel Management { get; set; }
        public CommissionWorkViewModel Work { get; set; }
        
        
    }
}