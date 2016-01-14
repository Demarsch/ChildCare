using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using AdminModule.Converters;
using AdminModule.Model;
using AdminModule.Services;
using Core.Extensions;
using Core.Services;
using Core.Wpf.Misc;
using Core.Wpf.Mvvm;
using Core.Wpf.Services;
using Core.Wpf.ViewModels;
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

        private readonly IDialogServiceAsync dialogService;

        private readonly Func<GroupEditDialogViewModel> groupEditDialogViewModelFactory;

        public UserAccessManagerViewModel(ILog log,
                                          ICacheService cacheService,
                                          IUserAccessService userAccessService,
                                          IDialogServiceAsync dialogService,
                                          Func<GroupEditDialogViewModel> groupEditDialogViewModelFactory)
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
            if (dialogService == null)
            {
                throw new ArgumentNullException("dialogService");
            }
            if (groupEditDialogViewModelFactory == null)
            {
                throw new ArgumentNullException("groupEditDialogViewModelFactory");
            }
            this.log = log;
            this.cacheService = cacheService;
            this.userAccessService = userAccessService;
            this.dialogService = dialogService;
            this.groupEditDialogViewModelFactory = groupEditDialogViewModelFactory;
            BusyMediator = new BusyMediator();
            FailureMediator = new FailureMediator();
            Users = new ObservableCollectionEx<UserViewModel>();
            Users.BeforeCollectionChanged += UsersOnBeforeCollectionChanged;
            var view = CollectionViewSource.GetDefaultView(Users);
            view.Filter = FilterUsers;
            view.SortDescriptions.Add(new SortDescription("FullName", ListSortDirection.Ascending));
            Groups = new ObservableCollectionEx<PermissionGroupViewModel>();
            Groups.BeforeCollectionChanged += GroupsOnBeforeCollectionChanged;
            view = CollectionViewSource.GetDefaultView(Groups);
            view.Filter = FilterGroups;
            view.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            Permissions = new ObservableCollectionEx<PermissionViewModel>();
            Permissions.BeforeCollectionChanged += PermissionsOnBeforeCollectionChanged;
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
            createNewGroupCommand = new DelegateCommand(CreateNewGroup);
        }

        private readonly char[] separators = { ' ', '-' };

        #region Permissions

        private readonly DelegateCommand<PermissionViewModel> selectPermissionCommand;

        public ICommand SelectPermissionCommand { get { return selectPermissionCommand; } }

        private void PermissionsOnBeforeCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (var oldPermission in e.OldItems.Cast<PermissionViewModel>())
                {
                    oldPermission.IncludeInCurrentGroupRequested -= PermissionOnIncludeInCurrentGroupRequested ;
                    oldPermission.ExcludeFromCurrentGroupRequested -= PermissionOnExcludeFromCurrentGroupRequested;
                }
            }
            if (e.NewItems != null)
            {
                foreach (var newPermission in e.NewItems.Cast<PermissionViewModel>())
                {
                    newPermission.IncludeInCurrentGroupRequested += PermissionOnIncludeInCurrentGroupRequested;
                    newPermission.ExcludeFromCurrentGroupRequested += PermissionOnExcludeFromCurrentGroupRequested;
                }
            }
        }

        private async void PermissionOnExcludeFromCurrentGroupRequested(object sender, EventArgs eventArgs)
        {
            var group = SelectedSecurityObject as PermissionGroupViewModel;
            var permission = (PermissionViewModel)sender;
            if (group == null)
            {
                log.Warn("Trying to remove permission from group but group wasn't selected");
                return;
            }
            await ExcludePermissionFromGroup(permission, group);
        }

        private async void PermissionOnIncludeInCurrentGroupRequested(object sender, EventArgs eventArgs)
        {
            var group = SelectedSecurityObject as PermissionGroupViewModel;
            var permission = (PermissionViewModel)sender;
            if (group == null)
            {
                log.Warn("Trying to add permission to group but group wasn't selected");
                return;
            }
            await IncludePermissionIntoGroup(permission, group);
        }

        private void SelectPermission(PermissionViewModel permission)
        {
            SelectedSecurityObject = permission == SelectedSecurityObject ? null : permission;
            ClearUserView();
            ClearPermissionView();
            var view = CollectionViewSource.GetDefaultView(Groups);
            using (view.DeferRefresh())
            {
                view.GroupDescriptions.Clear();
                view.SortDescriptions.Clear();
                if (SelectedSecurityObject != null)
                {
                    view.GroupDescriptions.Add(new PropertyGroupDescription(null, PermissionGroupViewModelToGroupHeaderConverter.Instance));
                    view.SortDescriptions.Add(new SortDescription("CurrentPermissionIsIncluded", ListSortDirection.Descending));
                    view.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
                }
                else
                {
                    view.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
                }
            }
        }
        private void ClearPermissionView()
        {
            var view = CollectionViewSource.GetDefaultView(Permissions);
            using (view.DeferRefresh())
            {
                view.GroupDescriptions.Clear();
                view.SortDescriptions.Clear();
                view.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            }
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

        private void UsersOnBeforeCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (var oldUser in e.OldItems.Cast<UserViewModel>())
                {
                    oldUser.IncludeIntoCurrentGroupRequested -= UserOnIncludeIntoCurrentGroupRequested;
                    oldUser.ExcludeFromCurrentGroupRequested -= UserOnExcludeFromCurrentGroupRequested;
                    oldUser.ActivationChangeRequested -= UserOnActivationChangeRequested;
                    oldUser.EditRequested -= UserOnEditRequested;
                }
            }
            if (e.NewItems != null)
            {
                foreach (var newUser in e.NewItems.Cast<UserViewModel>())
                {
                    newUser.IncludeIntoCurrentGroupRequested += UserOnIncludeIntoCurrentGroupRequested;
                    newUser.ExcludeFromCurrentGroupRequested += UserOnExcludeFromCurrentGroupRequested;
                    newUser.ActivationChangeRequested += UserOnActivationChangeRequested;
                    newUser.EditRequested += UserOnEditRequested;
                }
            }
        }

        private async void UserOnActivationChangeRequested(object sender, EventArgs eventArgs)
        {
            var user = (UserViewModel)sender;
            try
            {
                log.InfoFormat("Trying to {0} user '{1}'...", user.IsActive ? "deactivate" : "activate", user.FullName);
                BusyMediator.Activate(string.Format("{0} пользователя...", user.IsActive ? "Деактивируем" : "Активируем"));
                user.IsActive = !user.IsActive;
                await userAccessService.ChangeUserActivationAsync(user.User);
            }
            catch (Exception ex)
            {
                log.ErrorFormatEx(ex, "Failed to {0} user {1}", user.IsActive ? "deactivate" : "activate", user.FullName);
                FailureMediator.Activate(string.Format("Не удалось {0} пользователя. Попробуйте еще раз. Если ошибка повторится, пожалуйста, обратитесь в службу поддержки", user.IsActive ? "деактивировать" : "активировать"),
                                                       exception: ex,
                                                       canBeDeactivated: true);
            }
            finally
            {
                BusyMediator.Deactivate();
            }
        }

        private void UserOnEditRequested(object sender, EventArgs eventArgs)
        {
            throw new NotImplementedException();
        }

        private async void UserOnExcludeFromCurrentGroupRequested(object sender, EventArgs eventArgs)
        {
            var currentGroup = SelectedSecurityObject as PermissionGroupViewModel;
            var user = (UserViewModel)sender;
            if (currentGroup == null)
            {
                log.Warn("Trying to remove user from group but group wasn't selected");
                return;
            }
            await ExcludeUserFromGroup(user, currentGroup);
        }

        private async void UserOnIncludeIntoCurrentGroupRequested(object sender, EventArgs eventArgs)
        {
            var currentGroup = SelectedSecurityObject as PermissionGroupViewModel;
            var user = (UserViewModel)sender;
            if (currentGroup == null)
            {
                log.Warn("Trying to add user to group but group wasn't selected");
                return;
            }
            await IncludeUserIntoGroup(user, currentGroup);
        }

        private void SelectUser(UserViewModel user)
        {
            SelectedSecurityObject = user == SelectedSecurityObject ? null : user;
            ClearUserView();
            ClearPermissionView();
            var view = CollectionViewSource.GetDefaultView(Groups);
            using (view.DeferRefresh())
            {
                view.GroupDescriptions.Clear();
                view.SortDescriptions.Clear();
                if (SelectedSecurityObject != null)
                {
                    view.GroupDescriptions.Add(new PropertyGroupDescription(null, PermissionGroupViewModelToGroupHeaderConverter.Instance));
                    view.SortDescriptions.Add(new SortDescription("CurrentUserIsIncluded", ListSortDirection.Descending));
                    view.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
                }
                else
                {
                    view.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
                }
            }
        }

        private void ClearUserView()
        {
            var view = CollectionViewSource.GetDefaultView(Users);
            using (view.DeferRefresh())
            {
                view.GroupDescriptions.Clear();
                view.SortDescriptions.Clear();
                view.SortDescriptions.Add(new SortDescription("FullName", ListSortDirection.Ascending));
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

        private readonly DelegateCommand createNewGroupCommand;

        public ICommand CreateNewGroupCommand { get { return createNewGroupCommand; } }

        private async void CreateNewGroup()
        {
            var viewModel = groupEditDialogViewModelFactory();
            viewModel.ExistingPermissionGroup = null;
            viewModel.Name = string.Empty;
            viewModel.Description = string.Empty;
            var dialogResult = await dialogService.ShowDialogAsync(viewModel);
            if (dialogResult == true)
            {
                try
                {
                    log.InfoFormat("Creating new group with name '{0}'...", viewModel.Name);
                    BusyMediator.Activate("Сохранение группы...");
                    var newGroup = await userAccessService.CreateNewPermissionGroupAsync(viewModel.Name, viewModel.Description);
                    var newGroupViewModel = new PermissionGroupViewModel(newGroup) { UserMode = SelectedSecurityObject as UserViewModel };
                    Groups.Add(newGroupViewModel);
                }
                catch (Exception ex)
                {
                    log.ErrorFormatEx(ex, "Failed to add new group with name '{0}'", viewModel.Name);
                    FailureMediator.Activate("Не удалось создать новую группу. Попробуйте еще раз. Если ошибка повторится, пожалуйста, обратитесь в службу поддержки", exception: ex, canBeDeactivated: true);
                }
                finally
                {
                    viewModel.CancelValidation();
                    BusyMediator.Deactivate();
                }
            }
        }

        private void GroupsOnBeforeCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (var oldGroup in e.OldItems.Cast<PermissionGroupViewModel>())
                {
                    oldGroup.CurrentUserExcludeRequested -= GroupOnCurrentUserExcludeRequested;
                    oldGroup.CurrentUserIncludeRequested -= GroupOnCurrentUserIncludeRequested;
                    oldGroup.CurrentPermissionIncludeRequested -= GroupOnCurrentPermissionIncludeRequested;
                    oldGroup.CurrentPermissionExcludeRequested -= GroupOnCurrentPermissionExcludeRequested;
                    oldGroup.DeleteRequested -= GroupOnDeleteRequested;
                    oldGroup.EditRequested -= GroupOnEditRequested;
                }
            }
            if (e.NewItems != null)
            {
                foreach (var newGroup in e.NewItems.Cast<PermissionGroupViewModel>())
                {
                    newGroup.CurrentUserExcludeRequested += GroupOnCurrentUserExcludeRequested;
                    newGroup.CurrentUserIncludeRequested += GroupOnCurrentUserIncludeRequested;
                    newGroup.CurrentPermissionIncludeRequested += GroupOnCurrentPermissionIncludeRequested;
                    newGroup.CurrentPermissionExcludeRequested += GroupOnCurrentPermissionExcludeRequested;
                    newGroup.DeleteRequested += GroupOnDeleteRequested;
                    newGroup.EditRequested += GroupOnEditRequested;
                }
            }
        }

        private async void GroupOnCurrentPermissionExcludeRequested(object sender, EventArgs eventArgs)
        {
            var permission = SelectedSecurityObject as PermissionViewModel;
            var group = (PermissionGroupViewModel)sender;
            if (permission == null)
            {
                log.Warn("Trying to remove permission from group but permission wasn't selected");
                return;
            }
            await ExcludePermissionFromGroup(permission, group);
        }

        private async void GroupOnCurrentPermissionIncludeRequested(object sender, EventArgs eventArgs)
        {
            var permission = SelectedSecurityObject as PermissionViewModel;
            var group = (PermissionGroupViewModel)sender;
            if (permission == null)
            {
                log.Warn("Trying to add permission to group but permission wasn't selected");
                return;
            }
            await IncludePermissionIntoGroup(permission, group);
        }

        private async void GroupOnEditRequested(object sender, EventArgs eventArgs)
        {
            var viewModel = groupEditDialogViewModelFactory();
            var existingGroup = (PermissionGroupViewModel)sender;
            viewModel.ExistingPermissionGroup = existingGroup;
            var dialogResult = await dialogService.ShowDialogAsync(viewModel);
            if (dialogResult != true)
            {
                return;
            }
            try
            {
                log.InfoFormat("Saving changes to existing group with name '{0}'...", viewModel.Name);
                BusyMediator.Activate("Сохранение группы...");
                existingGroup.Group = await userAccessService.SavePermissionGroupAsync(viewModel.Name, viewModel.Description, existingGroup.Group);
            }
            catch (Exception ex)
            {
                log.ErrorFormatEx(ex, "Failed to save changes to existing group with name '{0}'", viewModel.Name);
                FailureMediator.Activate("Не удалось сохранить изменения в группе. Попробуйте еще раз. Если ошибка повторится, пожалуйста, обратитесь в службу поддержки", exception: ex, canBeDeactivated: true);
            }
            finally
            {
                viewModel.CancelValidation();
                BusyMediator.Deactivate();
            }
        }

        private async void GroupOnDeleteRequested(object sender, EventArgs eventArgs)
        {
            var group = (PermissionGroupViewModel)sender;
            var question = @group.Group.UserPermissionGroups.Count > 0
                ? "В выбранной группе состоят пользователи. Вы уверены что хотите убрать пользователей из группы и удалить ее безвозвратно?"
                : "Выбранная группа будет удалена безвозвратно. Продолжить?";
            var viewModel = new ConfirmationDialogViewModel();
            viewModel.Question = question;
            viewModel.ConfirmButtonText = "Удалить";
            viewModel.CancelButtonText = "Отмена";
            var dialogResult = await dialogService.ShowDialogAsync(viewModel);
            if (dialogResult != true)
            {
                return;
            }
            try
            {
                log.InfoFormat("Deleting existing group with name '{0}'...", group.Name);
                BusyMediator.Activate("Удаление группы...");
                await userAccessService.DeletePermissionGroupAsync(group.Group);
                Groups.Remove(group);
            }
            catch (Exception ex)
            {
                log.ErrorFormatEx(ex, "Failed to delete existing group with name '{0}'", group.Name);
                FailureMediator.Activate("Не удалось удалить группу. Попробуйте еще раз. Если ошибка повторится, пожалуйста, обратитесь в службу поддержки", exception: ex, canBeDeactivated: true);
            }
            finally
            {
                BusyMediator.Deactivate();
            }
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
            await IncludeUserIntoGroup(currentUser, group);
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
            await ExcludeUserFromGroup(currentUser, group);
        }

        private readonly DelegateCommand<PermissionGroupViewModel> selectGroupCommand;

        public ICommand SelectGroupCommand { get { return selectGroupCommand; } }

        private void SelectGroup(PermissionGroupViewModel group)
        {
            SelectedSecurityObject = group == SelectedSecurityObject ? null : group;
            ClearGroupView();
            var usersView = CollectionViewSource.GetDefaultView(Users);
            using (usersView.DeferRefresh())
            {
                usersView.GroupDescriptions.Clear();
                usersView.SortDescriptions.Clear();
                if (SelectedSecurityObject != null)
                {
                    usersView.GroupDescriptions.Add(new PropertyGroupDescription("IsIncludedInCurrentGroup"));
                    usersView.SortDescriptions.Add(new SortDescription("IsIncludedInCurrentGroup", ListSortDirection.Descending));
                    usersView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
                }
                else
                {
                    usersView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
                }
            }
            var permissionsView = CollectionViewSource.GetDefaultView(Permissions);
            using (permissionsView.DeferRefresh())
            {
                permissionsView.GroupDescriptions.Clear();
                permissionsView.SortDescriptions.Clear();
                if (SelectedSecurityObject != null)
                {
                    permissionsView.GroupDescriptions.Add(new PropertyGroupDescription("IsIncludedInCurrentGroup"));
                    permissionsView.SortDescriptions.Add(new SortDescription("IsIncludedInCurrentGroup", ListSortDirection.Descending));
                    permissionsView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
                }
                else
                {
                    permissionsView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
                }
            }
        }

        private void ClearGroupView()
        {
            var view = CollectionViewSource.GetDefaultView(Groups);
            using (view.DeferRefresh())
            {
                view.GroupDescriptions.Clear();
                view.SortDescriptions.Clear();
                view.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            }
        }

        public ObservableCollectionEx<PermissionGroupViewModel> Groups { get; private set; }

        private bool FilterGroups(object obj)
        {
            if (groupsFilterWords == null || groupsFilterWords.Length == 0)
            {
                return true;
            }
            var group = (PermissionGroupViewModel)obj;
            return groupsFilterWords.All(x => group.Name.IndexOf(x, StringComparison.CurrentCultureIgnoreCase) != -1);
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

        #region Cross-functionality

        private async Task IncludeUserIntoGroup(UserViewModel user, PermissionGroupViewModel group)
        {
            try
            {
                log.InfoFormat("Adding user '{0}' to group '{1}'...", user.FullName, group.Name);
                BusyMediator.Activate("Добавляем пользователя в группу...");
                await userAccessService.AddUserToGroupAsync(user.Id, group.Group.Id);
                log.InfoFormat("Successfully added user '{0}' to group '{1}'", user.FullName, group.Name);
                if (SelectedSecurityObject == user)
                {
                    group.UserMode = user;
                    CollectionViewSource.GetDefaultView(Groups).Refresh();
                }
                else
                {
                    user.GroupMode = group;
                    CollectionViewSource.GetDefaultView(Users).Refresh();

                }
            }
            catch (Exception ex)
            {
                log.ErrorFormatEx(ex, "Failed to add user '{0}' to group '{1}'", user.FullName, group.Name);
                FailureMediator.Activate("Не удалось добавить пользователя в группу. Попробуйте еще раз. Если ошибка повторится, пожалуйста, обратитесь в службу поддержки", exception: ex, canBeDeactivated: true);
            }
            finally
            {
                BusyMediator.Deactivate();
            }
        }

        private async Task ExcludeUserFromGroup(UserViewModel user, PermissionGroupViewModel group)
        {
            try
            {
                log.InfoFormat("Removing user '{0}' from group '{1}'...", user.FullName, group.Name);
                BusyMediator.Activate("Удаляем пользователя из группы...");
                await userAccessService.RemoveUserFromGroupAsync(user.Id, group.Group.Id);
                log.InfoFormat("Successfully removed user '{0}' to group '{1}'", user.FullName, group.Name);
                if (SelectedSecurityObject == user)
                {
                    group.UserMode = user;
                    CollectionViewSource.GetDefaultView(Groups).Refresh();
                }
                else
                {
                    user.GroupMode = group;
                    CollectionViewSource.GetDefaultView(Users).Refresh();

                }
            }
            catch (Exception ex)
            {
                log.ErrorFormatEx(ex, "Failed to remove user '{0}' from group '{1}'", user.FullName, group.Name);
                FailureMediator.Activate("Не удалось удалить пользователя из группы. Попробуйте еще раз. Если ошибка повторится, пожалуйста, обратитесь в службу поддержки", exception: ex, canBeDeactivated: true);
            }
            finally
            {
                BusyMediator.Deactivate();
            }
        }

        private async Task ExcludePermissionFromGroup(PermissionViewModel permission, PermissionGroupViewModel group)
        {
            try
            {
                log.InfoFormat("Removing permission '{0}' from group '{1}'...", permission.Name, group.Name);
                BusyMediator.Activate("Удаляем право из группы...");
                await userAccessService.RemovePermissionFromGroupAsync(permission.Permission.Id, group.Group.Id);
                log.InfoFormat("Successfully removed permission '{0}' from group '{1}'", permission.Name, group.Name);
                if (SelectedSecurityObject == permission)
                {
                    group.PermissionMode = permission;
                    CollectionViewSource.GetDefaultView(Groups).Refresh();
                }
                else
                {
                    permission.GroupMode = group;
                    CollectionViewSource.GetDefaultView(Permissions).Refresh();
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormatEx(ex, "Failed to remove permission '{0}' from group '{1}'", permission.Name, group.Name);
                FailureMediator.Activate("Не удалось удалить право из группы. Попробуйте еще раз. Если ошибка повторится, пожалуйста, обратитесь в службу поддержки", exception: ex, canBeDeactivated: true);
            }
            finally
            {
                BusyMediator.Deactivate();
            }
        }

        private async Task IncludePermissionIntoGroup(PermissionViewModel permission, PermissionGroupViewModel group)
        {
            try
            {
                log.InfoFormat("Adding permission '{0}' to group '{1}'...", permission.Name, group.Name);
                BusyMediator.Activate("Добавляем право в группу...");
                await userAccessService.AddPermissionToGroupAsync(permission.Permission.Id, group.Group.Id);
                log.InfoFormat("Successfully added permission '{0}' to group '{1}'", permission.Name, group.Name);
                if (SelectedSecurityObject == permission)
                {
                    group.PermissionMode = permission;
                    CollectionViewSource.GetDefaultView(Groups).Refresh();
                }
                else
                {
                    permission.GroupMode = group;
                    CollectionViewSource.GetDefaultView(Permissions).Refresh();
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormatEx(ex, "Failed to add permission '{0}' to group '{1}'", permission.Name, group.Name);
                FailureMediator.Activate("Не удалось добавить право в группу. Попробуйте еще раз. Если ошибка повторится, пожалуйста, обратитесь в службу поддержки", exception: ex, canBeDeactivated: true);
            }
            finally
            {
                BusyMediator.Deactivate();
            }
        }

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
                Groups.ForEach(x => x.PermissionMode = value as PermissionViewModel);
                Users.ForEach(x => x.GroupMode = value as PermissionGroupViewModel);
                Permissions.ForEach(x => x.GroupMode = value as PermissionGroupViewModel);
            }
        }

        private SecurityObjectType? selectedSecurityObjectType;

        public SecurityObjectType? SelectedSecurityObjectType
        {
            get { return selectedSecurityObjectType; }
            private set { SetProperty(ref selectedSecurityObjectType, value); }
        }

        #endregion

        public BusyMediator BusyMediator { get; private set; }

        public FailureMediator FailureMediator { get; private set; }

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
                Users.Replace(usersLoadingTask.Result.Select(x => new UserViewModel(x, cacheService)));
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
            Users.Clear();
            Users.BeforeCollectionChanged -= UsersOnBeforeCollectionChanged;
            Permissions.Clear();
            Permissions.BeforeCollectionChanged -= PermissionsOnBeforeCollectionChanged;
        }
    }
}
