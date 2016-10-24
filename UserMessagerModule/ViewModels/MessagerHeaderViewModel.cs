using Core.Wpf.Services;
using Prism;
using Prism.Mvvm;
using Prism.Regions;
using Shell.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using Core.Wpf.Extensions;
using System.Threading;
using Prism.Commands;

namespace UserMessagerModule.ViewModels
{
    public class MessagerHeaderViewModel: BindableBase, IActiveAware
    {
        IRegionManager regionManager;
        IViewNameResolver viewNameResolver;
        public MessagerInboxViewModel MessagerInboxViewModel { get; private set; }
        public MessagerSelectorViewModel MessagerSelectorViewModel { get; private set; }

        public MessagerHeaderViewModel(IRegionManager regionManager, IViewNameResolver viewNameResolver, MessagerInboxViewModel messagerInboxViewModel, MessagerSelectorViewModel messagerSelectorViewModel)
        {
            if ((this.regionManager = regionManager) == null)
                throw new ArgumentNullException("regionManager");
            if ((this.viewNameResolver = viewNameResolver) == null)
                throw new ArgumentNullException("viewNameResolver");
            if ((MessagerInboxViewModel = messagerInboxViewModel) == null)
                throw new ArgumentNullException("messagerInboxViewModel");
            if ((MessagerSelectorViewModel = messagerSelectorViewModel) == null)
                throw new ArgumentNullException("messagerInboxViewModel");
        }

        private CompositeCommand refreshAllCommand;
        public CompositeCommand RefreshAllCommand
        { 
            get
            {
                if (refreshAllCommand != null)
                    return refreshAllCommand;
                refreshAllCommand = new CompositeCommand();
                refreshAllCommand.RegisterCommand(MessagerSelectorViewModel.RefreshAllCommand);
                refreshAllCommand.RegisterCommand(MessagerInboxViewModel.RefreshAllCommand);
                return refreshAllCommand;
            }
        }

        bool isActive;
        public event EventHandler IsActiveChanged = delegate { };
        public bool IsActive
        {
            get { return isActive; }
            set
            {
                if (isActive == value)
                    return;

                isActive = value;
                IsActiveChanged(this, EventArgs.Empty);
                OnPropertyChanged(() => IsActive);
                if (value)
                {
                    regionManager.RequestNavigate(RegionNames.ModuleContent, viewNameResolver.Resolve<MessagerInboxViewModel>());
                    regionManager.RequestNavigate(RegionNames.ListItems, viewNameResolver.Resolve<MessagerSelectorViewModel>());
                }
                else
                {
                    regionManager.Regions[RegionNames.ListItems].DeactivateActiveViews();
                }
            }
        }
    }
}
