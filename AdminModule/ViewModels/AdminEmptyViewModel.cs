using System;
using System.Collections.Generic;
using System.Linq;
using Prism.Mvvm;
using Prism.Regions;

namespace AdminModule.ViewModels
{
    public class AdminEmptyViewModel : BindableBase, IConfirmNavigationRequest
    {
        public AdminEmptyViewModel()
        {

        }

        #region NavigationReguest

        public void ConfirmNavigationRequest(NavigationContext navigationContext, Action<bool> continuationCallback)
        {
            continuationCallback(true);
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
        }

        #endregion
    }
}
