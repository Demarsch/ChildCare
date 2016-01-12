using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using AdminModule.Model;
using AdminModule.Services;
using Core.Data;
using Core.Extensions;
using Core.Services;
using Core.Wpf.Misc;
using Core.Wpf.Mvvm;
using log4net;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;

namespace AdminModule.ViewModels
{
    public class UserAccessManagerViewModel : BindableBase, INavigationAware, IDisposable
    {
        private readonly ILog log;

        private readonly ICacheService cacheService;

        private readonly IUserAccessService userAccessService;

        public UserAccessManagerViewModel(ILog log, ICacheService cacheService, IUserAccessService userAccessService)
        {
            if (log == null)
            {
                throw new ArgumentNullException("log");
            }
            if (cacheService == null)
            {
                throw new ArgumentNullException("cacheService");
            }
            if (userAccessService == null)
            {
                throw new ArgumentNullException("userAccessService");
            }
            this.log = log;
            this.cacheService = cacheService;
            this.userAccessService = userAccessService;
            BusyMediator = new BusyMediator();
            FailureMediator = new FailureMediator();
            Users = new ObservableCollectionEx<UserViewModel>();
            var view = CollectionViewSource.GetDefaultView(Users);
            view.Filter = FilterUsers;
            view.SortDescriptions.Add(new SortDescription("FullName", ListSortDirection.Ascending));
            Groups = new ObservableCollectionEx<PermissionGroupViewModel>();
            Groups.BeforeCollectionChanged += GroupsOnBeforeCollectionChanged;
            view = CollectionViewSource.GetDefaultView(Groups);
            view.Filter = FilterGroups;
            view.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            Permissions = new ObservableCollectionEx<PermissionViewModel>();
            view = CollectionViewSource.GetDefaultView(Permissions);
            view.Filter = FilterPermissions;
            view.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            initialLoadingWrapper = new CommandWrapper
                                    {
                                        Command = new DelegateCommand(async () => await InitialLoadingAsync()),
                                        CommandName = "Повторить"
                                    };
            selectPermissionCommand = new DelegateCommand<PermissionViewModel>(SelectPermission);
            selectUserCommand = new DelegateCommand<UserViewModel>(SelectUser);
            selectGroupCommand = new DelegateCommand<PermissionGroupViewModel>(SelectGroup);
        }

        private readonly char[] separators = { ' ', '-' };

        #region Permissions

        private readonly DelegateCommand<PermissionViewModel> selectPermissionCommand;

        public ICommand SelectPermissionCommand { get { return selectPermissionCommand; } }

        private void SelectPermission(PermissionViewModel permission)
        {
            SelectedSecurityObject = permission == SelectedSecurityObject ? null : permission;
        }

        public ObservableCollectionEx<PermissionViewModel> Permissions { get; private set; }

        private bool FilterPermissions(object obj)
        {
            if (permissionsFilterWords == null || permissionsFilterWords.Length == 0)
            {
                return true;
            }
            var permission = (PermissionViewModel)obj;
            return permissionsFilterWords.All(x => permission.Name.IndexOf(x, StringComparison.CurrentCultureIgnoreCase) != -1);
        }

        private string[] permissionsFilterWords;

        private string permissionsFilter;

        public string PermissionsFilter
        {
            get { return permissionsFilter; }
            set
            {
                if (SetProperty(ref permissionsFilter, value))
                {
                    permissionsFilterWords = (value ?? string.Empty).Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    CollectionViewSource.GetDefaultView(Permissions).Refresh();
                }
            }
        }

        #endregion

        #region Users

        private readonly DelegateCommand<UserViewModel> selectUserCommand;

        public ICommand SelectUserCommand { get { return selectUserCommand; } }

        private void SelectUser(UserViewModel user)
        {
            SelectedSecurityObject = user == SelectedSecurityObject ? null : user;
            var view = CollectionViewSource.GetDefaultView(Groups);
            using (view.DeferRefresh())
            {
                view.GroupDescriptions.Clear();
                if (SelectedSecurityObject != null)
                {
                    view.GroupDescriptions.Add(new PropertyGroupDescription("CurrentUserIsIncluded"));
                }
            }
        }

        public ObservableCollectionEx<UserViewModel> Users { get; private set; }

        private bool FilterUsers(object obj)
        {
            if (usersFilterWords == null || usersFilterWords.Length == 0)
            {
                return true;
            }
            var user = (UserViewModel)obj;
            return usersFilterWords.All(x => user.FullName.IndexOf(x, StringComparison.CurrentCultureIgnoreCase) != -1);
        }

        private string[] usersFilterWords;

        private string usersFilter;

        public string UsersFilter
        {
            get { return usersFilter; }
            set
            {
                if (SetProperty(ref usersFilter, value))
                {
                    usersFilterWords = (value ?? string.Empty).Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    CollectionViewSource.GetDefaultView(Users).Refresh();
                }
            }
        }

        #endregion

        #region Groups

        private void GroupsOnBeforeCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (var oldGroup in e.OldItems.Cast<PermissionGroupViewModel>())
            {
                oldGroup.CurrentUserExcludeRequested -= GroupOnCurrentUserExcludeRequested;
                oldGroup.CurrentUserIncludeRequested -= GroupOnCurrentUserIncludeRequested;
                oldGroup.DeleteRequested -= GroupOnDeleteRequested;
            }
            foreach (var newGroup in e.NewItems.Cast<PermissionGroupViewModel>())
            {
                newGroup.CurrentUserExcludeRequested += GroupOnCurrentUserExcludeRequested;
                newGroup.CurrentUserIncludeRequested += GroupOnCurrentUserIncludeRequested;
                newGroup.DeleteRequested += GroupOnDeleteRequested;
            }
        }

        private void GroupOnDeleteRequested(object sender, EventArgs eventArgs)
        {
            throw new NotImplementedException();
        }

        private async void GroupOnCurrentUserIncludeRequested(object sender, EventArgs eventArgs)
        {
            var currentUser = SelectedSecurityObject as UserViewModel;
            var group = (PermissionGroupViewModel)sender;
            if (currentUser == null)
            {
                log.Warn("Trying to add user to group but user wasn't selected");
                return;
            }
            try
            {
                log.InfoFormat("Adding user '{0}' to group '{1}'...", currentUser.FullName, group.Name);
                BusyMediator.Activate("Добавляем пользователя в группу...");
                await userAccessService.AddUserToGroupAsync(currentUser.Id, group.Id);
                group.UserMode = currentUser;
                log.InfoFormat("Successfully added user '{0}' to group '{1}'", currentUser.FullName, group.Name);
                CollectionViewSource.GetDefaultView(Groups).Refresh();
            }
            catch (Exception ex)
            {
                log.InfoFormat("Failed to add user '{0}' to group '{1}'", currentUser.FullName, group.Name);
                FailureMediator.Activate("Не удалось добавить пользователя в группу. Попробуйте еще раз. Если ошибка повторится, пожалуйста, обратитесь в службу поддержки", exception:ex, canBeDeactivated:true);
            }
            finally
            {
                BusyMediator.Deactivate();
            }
        }

        private async void GroupOnCurrentUserExcludeRequested(object sender, EventArgs eventArgs)
        {
            var currentUser = SelectedSecurityObject as UserViewModel;
            var group = (PermissionGroupViewModel)sender;
            if (currentUser == null)
            {
                log.Warn("Trying to remove user from group but user wasn't selected");
                return;
            }
            try
            {
                log.InfoFormat("Removing user '{0}' from group '{1}'...", currentUser.FullName, group.Name);
                BusyMediator.Activate("Удаляем пользователя из группы...");
                await userAccessService.RemoveUserFromGroupAsync(currentUser.Id, group.Id);
                group.UserMode = currentUser;
                log.InfoFormat("Successfully removed user '{0}' to group '{1}'", currentUser.FullName, group.Name);
                CollectionViewSource.GetDefaultView(Groups).Refresh();
            }
            catch (Exception ex)
            {
                log.InfoFormat("Failed to remove user '{0}' from group '{1}'", currentUser.FullName, group.Name);
                FailureMediator.Activate("Не удалось удалить пользователя из группы. Попробуйте еще раз. Если ошибка повторится, пожалуйста, обратитесь в службу поддержки", exception: ex, canBeDeactivated: true);
            }
            finally
            {
                BusyMediator.Deactivate();
            }
        }

        private readonly DelegateCommand<PermissionGroupViewModel> selectGroupCommand;

        public ICommand SelectGroupCommand { get { return selectGroupCommand; } }

        private void SelectGroup(PermissionGroupViewModel @group)
        {
            SelectedSecurityObject = @group == SelectedSecurityObject ? null : @group;
        }

        public ObservableCollectionEx<PermissionGroupViewModel> Groups { get; private set; }

        private bool FilterGroups(object obj)
        {
            if (groupsFilterWords == null || groupsFilterWords.Length == 0)
            {
                return true;
            }
            var @group = (PermissionGroupViewModel)obj;
            return groupsFilterWords.All(x => @group.Name.IndexOf(x, StringComparison.CurrentCultureIgnoreCase) != -1);
        }

        private string[] groupsFilterWords;

        private string groupsFilter;

        public string GroupsFilter
        {
            get { return groupsFilter; }
            set
            {
                if (SetProperty(ref groupsFilter, value))
                {
                    groupsFilterWords = (value ?? string.Empty).Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    CollectionViewSource.GetDefaultView(Groups).Refresh();
                }
            }
        }

        #endregion

        public BusyMediator BusyMediator { get; private set; }

        public FailureMediator FailureMediator { get; private set; }

        private object selectedSecurityObject;

        public object SelectedSecurityObject
        {
            get { return selectedSecurityObject; }
            private set
            {
                if (!SetProperty(ref selectedSecurityObject, value))
                {
                    return;
                }
                if (value == null)
                {
                    SelectedSecurityObjectType = null;
                }
                else if (value.GetType() == typeof(UserViewModel))
                {
                    SelectedSecurityObjectType = SecurityObjectType.User;
                }
                else if (value.GetType() == typeof(PermissionGroupViewModel))
                {
                    SelectedSecurityObjectType = SecurityObjectType.Group;
                }
                else if (value.GetType() == typeof(PermissionViewModel))
                {
                    SelectedSecurityObjectType = SecurityObjectType.Permission;
                }
                else
                {
                    SelectedSecurityObjectType = null;
                }
                Groups.ForEach(x => x.UserMode = value as UserViewModel);
            }
        }

        private SecurityObjectType? selectedSecurityObjectType;

        public SecurityObjectType? SelectedSecurityObjectType
        {
            get { return selectedSecurityObjectType; }
            private set { SetProperty(ref selectedSecurityObjectType, value); }
        }

        private TaskCompletionSource<object> initialLoadingTaskSource;

        private readonly CommandWrapper initialLoadingWrapper;

        private async Task InitialLoadingAsync()
        {
            if (initialLoadingTaskSource != null)
            {
                await initialLoadingTaskSource.Task;
                return;
            }
            initialLoadingTaskSource = new TaskCompletionSource<object>();
            BusyMediator.Activate("Загрузка пользователей, групп и прав...");
            try
            {
                var usersLoadingTask = userAccessService.GetUsersAsync();
                var groupsLoadingTask = userAccessService.GetGroupsAsync();
                var permissionsLoadingTask = userAccessService.GetPermissionsAsync();
                await Task.WhenAll(usersLoadingTask, groupsLoadingTask, permissionsLoadingTask);
                Users.Replace(usersLoadingTask.Result.Select(x => new UserViewModel(x)));
                Groups.Replace(groupsLoadingTask.Result.Select(x => new PermissionGroupViewModel(x)));
                Permissions.Replace(permissionsLoadingTask.Result.Select(x => new PermissionViewModel(x)));
            }
            catch (Exception ex)
            {
                FailureMediator.Activate("Не удалось загрузить списки пользовалетей, групп и прав. Попробуйте еще раз. Если ошибка повторится, пожалуйста, обратитесь в службу поддержки",
                                         initialLoadingWrapper,
                                         ex);
                initialLoadingTaskSource.SetResult(null);
                initialLoadingTaskSource = null;
            }
            finally
            {
                BusyMediator.Deactivate();
            }
        }

        public async void OnNavigatedTo(NavigationContext navigationContext)
        {
            await InitialLoadingAsync();
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        public void Dispose()
        {
            Groups.Clear();
            Groups.BeforeCollectionChanged -= GroupsOnBeforeCollectionChanged;
        }
    }
}
