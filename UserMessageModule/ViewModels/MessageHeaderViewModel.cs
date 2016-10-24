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

namespace UserMessageModule.ViewModels
{
    public class MessageHeaderViewModel: BindableBase, IActiveAware
    {
        IRegionManager regionManager;
        IViewNameResolver viewNameResolver;
        public MessageInboxViewModel MessageInboxViewModel { get; private set; }
        public MessageSelectorViewModel MessageSelectorViewModel { get; private set; }

        public MessageHeaderViewModel(IRegionManager regionManager, IViewNameResolver viewNameResolver, MessageInboxViewModel messageInboxViewModel, MessageSelectorViewModel messageSelectorViewModel)
        {
            if ((this.regionManager = regionManager) == null)
                throw new ArgumentNullException("regionManager");
            if ((this.viewNameResolver = viewNameResolver) == null)
                throw new ArgumentNullException("viewNameResolver");
            if ((MessageInboxViewModel = messageInboxViewModel) == null)
                throw new ArgumentNullException("messageInboxViewModel");
            if ((MessageSelectorViewModel = messageSelectorViewModel) == null)
                throw new ArgumentNullException("messageInboxViewModel");
        }

        private CompositeCommand refreshAllCommand;
        public CompositeCommand RefreshAllCommand
        { 
            get
            {
                if (refreshAllCommand != null)
                    return refreshAllCommand;
                refreshAllCommand = new CompositeCommand();
                refreshAllCommand.RegisterCommand(MessageSelectorViewModel.RefreshAllCommand);
                refreshAllCommand.RegisterCommand(MessageInboxViewModel.RefreshAllCommand);
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
                    regionManager.RequestNavigate(RegionNames.ModuleContent, viewNameResolver.Resolve<MessageInboxViewModel>());
                    regionManager.RequestNavigate(RegionNames.ListItems, viewNameResolver.Resolve<MessageSelectorViewModel>());
                }
                else
                {
                    regionManager.Regions[RegionNames.ListItems].DeactivateActiveViews();
                }
            }
        }
    }
}
