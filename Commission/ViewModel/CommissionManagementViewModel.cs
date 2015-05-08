using Core;
using GalaSoft.MvvmLight;
using System;

namespace Commission
{
    [PropertyChanged.ImplementPropertyChanged]
    public class CommissionManagementViewModel : ViewModelBase
    {
        public CommissionManagementViewModel(CommissionNavigatorViewModel navigator, CommissionPersonFlowViewModel personFlow)
        {
            Navigator = navigator;
            PersonFlow = personFlow;

            Navigator.Navigated += Navigator_Navigated;
        }

        void Navigator_Navigated(object sender, dynamic e)
        {
            PersonFlow.Navigate(e);
        }

        public CommissionNavigatorViewModel Navigator { get; set; }
        public CommissionPersonFlowViewModel PersonFlow { get; set; }

    }
}