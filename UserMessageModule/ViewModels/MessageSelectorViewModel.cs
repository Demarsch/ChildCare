using Core.Data;
using Core.Data.Misc;
using Core.Data.Services;
using Core.Extensions;
using Core.Notification;
using Core.Wpf.Events;
using Core.Wpf.Mvvm;
using Core.Wpf.Services;
using log4net;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Dynamic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UserMessageModule.Services;

namespace UserMessageModule.ViewModels
{
    public class MessageSelectorViewModel : BindableBase, IDisposable, INavigationAware
    {
        IUserMessageService userMessageService;
        ILog logService;
        IUserService userService;
        INotificationService notificationService;
        IDialogService dialogService;
        IEventAggregator eventAggregator;

        public MessageSelectorViewModel(IUserMessageService userMessageService, ILog logService, IUserService userService,
            INotificationService notificationService, IDialogService dialogService, IEventAggregator eventAggregator)
        {
            if ((this.userMessageService = userMessageService) == null)
                throw new ArgumentNullException("userMessageService");
            if ((this.logService = logService) == null)
                throw new ArgumentNullException("logService");
            if ((this.userService = userService) == null)
                throw new ArgumentNullException("userService");
            if ((this.notificationService = notificationService) == null)
                throw new ArgumentNullException("notificationService");
            if ((this.dialogService = dialogService) == null)
                throw new ArgumentNullException("dialogService");
            if ((this.eventAggregator = eventAggregator) == null)
                throw new ArgumentNullException("eventAggregator");
            LoadItems();
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

        public void Dispose()
        {
            if (userMessagesChangeSubscription == null)
                return;
            userMessagesChangeSubscription.Notified -= userMessagesChangeSubscription_Notified;
            userMessagesChangeSubscription.Dispose();
        }

        private DelegateCommand refreshAllCommand;
        public DelegateCommand RefreshAllCommand { get { return refreshAllCommand ?? (refreshAllCommand = new DelegateCommand(() => { LoadItems(); })); } }

        private INotificationServiceSubscription<UserMessage> userMessagesChangeSubscription;
        void userMessagesChangeSubscription_Notified(object sender, NotificationEventArgs<UserMessage> e)
        {
            LoadItems();
        }

        int currentUserId;
        private ObservableCollectionEx<dynamic> items;
        public ObservableCollectionEx<dynamic> Items { get { return items ?? (items = new ObservableCollectionEx<dynamic>()); } }
        public CancellationTokenSource loadItemsCTS;

        public async void LoadItems()
        {
            logService.Info("Loading messages types...");

            if (userMessagesChangeSubscription != null)
            {
                userMessagesChangeSubscription.Notified -= userMessagesChangeSubscription_Notified;
                userMessagesChangeSubscription.Dispose();
            }

            IDisposableQueryable<UserMessage> query = null;
            try
            {
                if (currentUserId == 0)
                    currentUserId = userService.GetCurrentUserId();

                query = userMessageService.GetMessages(0, currentUserId, DateTime.Now);

                if (loadItemsCTS != null)
                {
                    loadItemsCTS.Cancel();
                    loadItemsCTS.Dispose();
                }
                loadItemsCTS = new CancellationTokenSource();

                var result = await query.GroupBy(x => x.UserMessageTypeId).Select(x => new
                    {
                        x.Key,
                        CountAll = x.Count(),
                        CountNew = x.Count(z => !z.ReadDateTime.HasValue)
                    }).ToArrayAsync(loadItemsCTS.Token);

                List<dynamic> items = new List<dynamic>();

                dynamic allitem = new ExpandoObject();
                allitem.Id = 0;
                allitem.Text = ModuleStrings.MessageSelectorAllItem;
                var allnew = result.Any() ? result.Sum(x => x.CountNew) : 0;
                allitem.Info = result.Any() ? ((allnew > 0 ? allnew + " / " : "") + result.Sum(x => x.CountAll)) : "";
                allitem.HasNew = (allnew > 0);

                HeaderText = string.Format("{0} {1}", ModuleStrings.TabHeaderTitle, allitem.Info);
                if (allnew > 0)
                    eventAggregator.GetEvent<ShellNotificationPopupEvent>().Publish(
                        new Core.Wpf.Misc.ShellNotificationPopupEventData(
                            ModuleStrings.TabHeaderTitle, 
                            ModuleStrings.TabHeaderTipBegin + allnew + ((allnew % 10 == 1 && allnew / 10 != 1) ? ModuleStrings.TabHeaderTipEnd1 : ((allnew % 10 > 1 && allnew % 10 < 4 && allnew / 10 != 1) ? ModuleStrings.TabHeaderTipEnd234 : ModuleStrings.TabHeaderTipEnd))
                            ));

                items.Add(allitem);
                items.AddRange(await result.Select(x =>
                    {
                        dynamic t = new ExpandoObject();
                        t.Id = x.Key;
                        t.Text = userMessageService.GetMessageTypeById(x.Key).ShortName;
                        t.Info = (x.CountNew > 0 ? x.CountNew + " / " : "") + x.CountAll;
                        t.HasNew = (x.CountNew > 0);
                        return t;
                    }).OrderBy(x => x.Id == 1).ThenBy(x => x.Text).ToArrayAsync(loadItemsCTS.Token));

                SelectedItemNoRaise = true;
                Items.Replace(items);
                SelectedItemNoRaise = false;
                SelectedItem = Items.FirstOrDefault(x => x.Id == (SelectedItem != null ? SelectedItem.Id : -1)) ?? Items.FirstOrDefault();

                userMessagesChangeSubscription = notificationService.Subscribe<UserMessage>(x => x.RecieverUserId == currentUserId);
                if (userMessagesChangeSubscription != null)
                    userMessagesChangeSubscription.Notified += userMessagesChangeSubscription_Notified;
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to load user messages types");
                dialogService.ShowError(ModuleStrings.MessageTypesLoadingError);
            }
            finally
            {
                if (query != null)
                    query.Dispose();
            }
        }

        private string headerText;
        public string HeaderText { get { return headerText; } set { SetProperty(ref headerText, value); } }

        bool SelectedItemNoRaise;
        public event EventHandler<dynamic> SelectionChanged;
        private dynamic selectedItem;
        public dynamic SelectedItem
        {
            get { return selectedItem; }
            set
            {
                if (SelectedItemNoRaise)
                    return;
                if (SetProperty(ref selectedItem, value) && SelectionChanged != null)
                    SelectionChanged(this, selectedItem);
            }
        }
    }
}
