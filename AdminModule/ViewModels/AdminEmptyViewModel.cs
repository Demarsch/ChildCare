using System;
using System.Data.Entity;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Data;
using log4net;
using Prism.Mvvm;
using Prism.Commands;
using Prism.Interactivity;
using Prism.Events;
using Prism.Common;
using Prism.Regions;
using Core.Data;
using Core.Data.Misc;
using Core.Data.Services;
using Core.Extensions;
using Core.Misc;
using Core.Services;
using Core.Wpf.Events;
using Core.Wpf.Misc;
using Core.Wpf.Mvvm;

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
