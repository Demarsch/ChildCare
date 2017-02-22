using Core.Data;
using Core.Data.Misc;
using Core.Data.Services;
using Core.Extensions;
using Core.Misc;
using Core.Notification;
using Core.Services;
using Core.Wpf.Events;
using Core.Wpf.Misc;
using Core.Wpf.Mvvm;
using Core.Wpf.Services;
using log4net;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using UserMessageModule.Services;

namespace UserMessageModule.ViewModels
{
    public class MessageInboxViewModel : BindableBase, IDisposable
    {
        IEventAggregator eventAggregator;
        IDialogService dialogService;
        ILog logService;
        IUserMessageService userMessageService;
        IUserService userService;
        INotificationService notificationService;
        MessageSelectorViewModel messageSelectorViewModel;
        
        public MessageInboxViewModel(IEventAggregator eventAggregator, IDialogService dialogService, ILog logService,
            IUserMessageService userMessageService, IUserService userService, INotificationService notificationService, MessageSelectorViewModel messageSelectorViewModel)
        {
            if ((this.eventAggregator = eventAggregator) == null)
                throw new ArgumentNullException("eventAggregatorСonclusionViewModel");
            if ((this.dialogService = dialogService) == null)
                throw new ArgumentNullException("dialogService");
            if ((this.logService = logService) == null)
                throw new ArgumentNullException("logService");
            if ((this.userMessageService = userMessageService) == null)
                throw new ArgumentNullException("userMessageService");
            if ((this.userService = userService) == null)
                throw new ArgumentNullException("userService");
            if ((this.notificationService = notificationService) == null)
                throw new ArgumentNullException("notificationService");
            if ((this.messageSelectorViewModel = messageSelectorViewModel) == null)
                throw new ArgumentNullException("messageSelectorViewModel");
            ShowRead = userService.GetCurrentUserSettingsValue(ModuleStrings.ShowReadSetting) == bool.TrueString;
            ReadOpened = userService.GetCurrentUserSettingsValue(ModuleStrings.ReadOpenedSetting) == bool.TrueString;
            messageSelectorViewModel.SelectionChanged += messageSelectorViewModel_SelectionChanged;
        }

        public void Dispose()
        {
            if (userMessagesChangeSubscription == null)
                return;
            userMessagesChangeSubscription.Notified -= userMessagesChangeSubscription_Notified;
            userMessagesChangeSubscription.Dispose();
        }

        private BusyMediator busyMediator;
        public BusyMediator BusyMediator { get { return busyMediator ?? (busyMediator = new BusyMediator()); } }
        
        private DelegateCommand refreshAllCommand;
        public DelegateCommand RefreshAllCommand { get { return refreshAllCommand ?? (refreshAllCommand = new DelegateCommand(LoadItems)); } }

        private INotificationServiceSubscription<UserMessage> userMessagesChangeSubscription;
        void userMessagesChangeSubscription_Notified(object sender, NotificationEventArgs<UserMessage> e)
        {
            LoadItems();
        }

        int currentUserId;
        int currentMessageTypeId;
        void messageSelectorViewModel_SelectionChanged(object sender, dynamic e)
        {
            currentMessageTypeId = e != null ? e.Id : 0;
            LoadItems();
        }

        private bool showRead;
        public bool ShowRead
        { 
            get { return showRead; }
            set 
            { 
                if (SetProperty(ref showRead, value))
                    userService.SetCurrentUserSettingsValue(ModuleStrings.ShowReadSetting, ShowRead.ToString());
            }
        }

        private ObservableCollectionEx<dynamic> items;
        public ObservableCollectionEx<dynamic> Items { get { return items ?? (items = new ObservableCollectionEx<dynamic>()); } }
        public CancellationTokenSource loadItemsCTS;

        public async void LoadItems()
        {
            BusyMediator.Activate("Загрузка сообщений...");
            logService.Info(ModuleStrings.MessagesLoading);

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

                var filtered = currentMessageTypeId == 0 ? query.AsQueryable() : query.Where(x => x.UserMessageTypeId == currentMessageTypeId);
                if (!ShowRead)
                    filtered = filtered.Where(x => !x.ReadDateTime.HasValue);

                if (loadItemsCTS != null)
                {
                    loadItemsCTS.Cancel();
                    loadItemsCTS.Dispose();
                }
                loadItemsCTS = new CancellationTokenSource();

                var result = await (await filtered.Select(x => new
                {
                    x.Id,
                    x.UserMessageTypeId,
                    x.UserMessageType.ShortName,
                    x.SendDateTime,
                    x.ReadDateTime,
                    x.MessageTag,
                    x.MessageText,
                    x.IsHighPriority,
                    Sender = x.User.Person.ShortName
                }).ToArrayAsync(loadItemsCTS.Token)).Select(x =>
                {
                    dynamic t = new ExpandoObject();
                    t.Id = x.Id;
                    t.TypeId = x.UserMessageTypeId;
                    t.Theme = x.ShortName;
                    t.SendDateTime = x.SendDateTime;
                    t.State = x.ReadDateTime.HasValue ? 0 : (x.IsHighPriority ? 2 : 1);
                    t.Tag = x.MessageTag;
                    t.Text = x.MessageText;
                    t.Sender = x.Sender;
                    t.HasTag = !string.IsNullOrWhiteSpace(x.MessageTag);
                    return t;
                }).OrderByDescending(x => x.State == 2).ThenByDescending(x => x.SendDateTime).ToArrayAsync(loadItemsCTS.Token);

                SelectedItemNoRaise = true;
                Items.Replace(result);
                SelectedItemNoRaise = false;
                SelectedItem = Items.FirstOrDefault(x => x.Id == (SelectedItem != null ? SelectedItem.Id : -1)) ?? Items.FirstOrDefault();

                //userMessagesChangeSubscription = notificationService.Subscribe<UserMessage>(x => x.RecieverUserId == currentUserId);
                //if (userMessagesChangeSubscription != null)
                //    userMessagesChangeSubscription.Notified += userMessagesChangeSubscription_Notified;
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to load user messages");
                dialogService.ShowError(ModuleStrings.MessagesLoadingError);
            }
            finally
            {
                if (query != null)
                    query.Dispose();
                BusyMediator.Deactivate();
            }
        }

        bool SelectedItemNoRaise;
        public event EventHandler<dynamic> SelectionChanged;
        private dynamic selectedItem;
        public dynamic SelectedItem
        {
            get
            {
                return selectedItem;
            }
            set
            {
                if (SelectedItemNoRaise)
                    return;
                if (!SetProperty(ref selectedItem, value))
                    return;
                ReadMessageCommand.RaiseCanExecuteChanged();
                if (SelectionChanged != null)
                    SelectionChanged(this, selectedItem);
            }
        }

        private DelegateCommand readMessageCommand;
        public DelegateCommand ReadMessageCommand { get { return readMessageCommand ?? (readMessageCommand = new DelegateCommand(ReadMessageCommandAction, () => SelectedItem != null && SelectedItem.State != 0)); } }
        public void ReadMessageCommandAction()
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    userMessageService.SetMessageReadDateTime(SelectedItem.Id, DateTime.Now);
                }
                catch (Exception ex)
                {
                    logService.ErrorFormatEx(ex, "Failed to mark message as readed");
                    Dispatcher.CurrentDispatcher.Invoke(() => dialogService.ShowError(ModuleStrings.MessageReadError));
                }
            });        
        }

        private bool readOpened;
        public bool ReadOpened
        { 
            get { return readOpened; }
            set
            {
                if (SetProperty(ref readOpened, value))
                    userService.SetCurrentUserSettingsValue(ModuleStrings.ReadOpenedSetting, ReadOpened.ToString());
            }
        }

        private DelegateCommand openMessageCommand;
        public DelegateCommand OpenMessageCommand { get { return openMessageCommand ?? (openMessageCommand = new DelegateCommand(OpenMessageCommandAction, () => SelectedItem != null && SelectedItem.HasTag)); } }
        public void OpenMessageCommandAction()
        {
            eventAggregator.GetEvent<OpenUserMessageEvent>().Publish(new OpenUserMessageEventData(SelectedItem.Id, SelectedItem.TypeId, SelectedItem.Text, SelectedItem.Tag));
            if (ReadOpened)
                ReadMessageCommand.Execute();
        }

    }
}
