using Core;
using GalaSoft.MvvmLight;
using System;

namespace Commission
{
    [PropertyChanged.ImplementPropertyChanged]
    public class CommissionManagementViewModel : ViewModelBase
    {
        public CommissionManagementViewModel(CommissionNavigatorViewModel navigator, CommissionPersonFlowViewModel personFlow, CommissionDecisionViewModel decision)
        {
            Navigator = navigator;
            PersonFlow = personFlow;
            Decision = decision;
            Navigator.Navigated += Navigator_Navigated;
        }

        void Navigator_Navigated(object sender, dynamic e)
        {
            PersonFlow.Navigate(e);
            Decision.Navigate(e);
        }

        public CommissionNavigatorViewModel Navigator { get; set; }
        public CommissionPersonFlowViewModel PersonFlow { get; set; }
        public CommissionDecisionViewModel Decision { get; set; }
        
        
    }
}