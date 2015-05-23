using AdminTools.View;
using Core;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using log4net;
using DataLib;
using MainLib;
using AdminTools.DTO;

namespace AdminTools.ViewModel
{
    class UserEditorViewModel : ObservableObject
    {
        #region UserData

        private ObservableCollection<UserViewModel> users;
        public RelayCommand SearchUserCommand { get; private set; }
        public RelayCommand NewUserCommand { get; private set; }
        public RelayCommand AddUserPermissionsCommand { get; private set; }
        private string searchText = String.Empty;

        private IUserService userService;
        private IUserSystemInfoService userSystemInfoService;
        private IPersonService personService;
        private UserAccountViewModel userAccountViewModel;
        private ILog log;
        private IDialogService dialogService;
        private IPermissionService permissionService;

        #endregion

        #region Constructor

        public UserEditorViewModel(IUserService userService, IUserSystemInfoService userSystemInfoService,
            IPersonService personService, IPermissionService permissionService, UserAccountViewModel userAccountViewModel, ILog log, IDialogService dialogService)
        {
            this.userService = userService;
            this.userSystemInfoService = userSystemInfoService;
            this.personService = personService;
            this.permissionService = permissionService;
            this.userAccountViewModel = userAccountViewModel;
            this.log = log;
            this.dialogService = dialogService;
                       
            users = new ObservableCollection<UserViewModel>(userService.GetAllActiveUsers(DateTime.Now).Select(x => new UserViewModel(x, personService)).ToArray());
            this.EditUserCommand = new RelayCommand<object>(EditUser);
            this.SearchUserCommand = new RelayCommand(this.LoadUsers);
            this.NewUserCommand = new RelayCommand(this.NewUser);
            this.UpdateUserStatusCommand = new RelayCommand<object>(UpdateUserStatus);
            this.SynchWithActiveDirectoryCommand = new RelayCommand<object>(SynchWithActiveDirectory);
            this.SelectUserADCommand = new RelayCommand<object>(SelectUserAD);

            this.AddUserPermissionsCommand = new RelayCommand(this.AddUserPermissions);
            this.EditUserPermissionCommand = new RelayCommand<object>(EditUserPermission);
            this.RemoveUserPermissionCommand = new RelayCommand<object>(RemoveUserPermission);
            ShowOnlyActiveUsers = true;
        }    

        #endregion 

        #region Properties
                     
        public ObservableCollection<UserViewModel> Users
        {
            get { return users; }
            set { Set("Users", ref users, value); }
        }
     
        private UserViewModel selectedUser;
        public UserViewModel SelectedUser
        {
            get { return selectedUser; }
            set 
            {
                if (!Set("SelectedUser", ref selectedUser, value))
                    return;
                if (value != null)
                    UserPermissions = new ObservableCollection<UserPermissionsDTO>(userService.GetUserPermissions(value.Id)
                        .Select(x => new UserPermissionsDTO() { PermissionName = permissionService.GetPermissionById(x.PermissionId).Name, BeginDate = x.BeginDateTime, EndDate = x.EndDateTime, IsGranted = x.IsGranted}));
            }
        }       

        public string SearchText
        {
            get { return searchText; }
            set { Set("SearchText", ref searchText, value); }
        }

        private bool showOnlyActiveUsers;
        public bool ShowOnlyActiveUsers
        {
            get { return showOnlyActiveUsers; }
            set 
            {
                if (!Set("ShowOnlyActiveUsers", ref showOnlyActiveUsers, value))
                    return;
                LoadUsersAsync();
            }
        }

        private bool showPopup;
        public bool ShowPopup
        {
            get { return showPopup; }
            set { Set("ShowPopup", ref showPopup, value); }
        }

        private ObservableCollection<UserPermissionsDTO> userPermissions;
        public ObservableCollection<UserPermissionsDTO> UserPermissions
        {
            get { return userPermissions; }
            set { Set("UserPermissions", ref userPermissions, value); }

        }
        private ObservableCollection<UserSystemInfo> usersAD;
        public ObservableCollection<UserSystemInfo> UsersAD
        {
            get { return usersAD; }
            set { Set("UsersAD", ref usersAD, value); }
        }

        private UserSystemInfo selectedUserAD;
        public UserSystemInfo SelectedUserAD
        {
            get { return selectedUserAD; }
            set { Set("SelectedUserAD", ref selectedUserAD, value); }
        }

        private RelayCommand<object> editUserPermissionCommand;
        public RelayCommand<object> EditUserPermissionCommand
        {
            get { return editUserPermissionCommand; }
            set { Set("EditUserPermissionCommand", ref editUserPermissionCommand, value); }
        }

        private RelayCommand<object> removeUserPermissionCommand;
        public RelayCommand<object> RemoveUserPermissionCommand
        {
            get { return removeUserPermissionCommand; }
            set { Set("RemoveUserPermissionCommand", ref removeUserPermissionCommand, value); }
        }

        private RelayCommand<object> editUserCommand;
        public RelayCommand<object> EditUserCommand
        {
            get { return editUserCommand; }
            set { Set("EditUserCommand", ref editUserCommand, value); }
        }

        private RelayCommand<object> updateUserStatusCommand;
        public RelayCommand<object> UpdateUserStatusCommand
        {
            get { return updateUserStatusCommand; }
            set { Set("UpdateUserStatusCommand", ref updateUserStatusCommand, value); }
        }

        private RelayCommand<object> selectUserADCommand;
        public RelayCommand<object> SelectUserADCommand
        {
            get { return selectUserADCommand; }
            set { Set("SelectUserADCommand", ref selectUserADCommand, value); }
        }

        private RelayCommand<object> synchWithActiveDirectoryCommand;
        public RelayCommand<object> SynchWithActiveDirectoryCommand
        {
            get { return synchWithActiveDirectoryCommand; }
            set { Set("SynchWithActiveDirectoryCommand", ref synchWithActiveDirectoryCommand, value); }
        }
        #endregion

        private void EditUser(object parameter)
        {
            //if (parameter == null)
            //{
            //    MessageBox.Show("Не выбран пользователь.");
            //    return;
            //}
            //var personViewModel = new EditPersonViewModel(log, personService, dialogService, (parameter as UserViewModel).PersonId);
            //(new EditPersonView() { DataContext = personViewModel, WindowStartupLocation = WindowStartupLocation.CenterScreen }).ShowDialog();

            ////TODO: Change verification way
            //if (personViewModel.EditPersonDataViewModel == null || personViewModel.EditPersonDataViewModel.TextMessage != "Данные сохранены") return;
            //SelectedUser.UserFullName = personViewModel.PersonFullName;
        }

        private void NewUser()
        {
            var userAccountModelView = new UserAccountViewModel(userService, userSystemInfoService, log, personService, dialogService);
            (new UserAccountView() { DataContext = userAccountModelView }).ShowDialog();

            if (userAccountModelView.CurrentUser != null)
                Users.Add(userAccountModelView.CurrentUser);
        }

        private void UpdateUserStatus(object parameter)
        {
            if (parameter == null)
            {
                MessageBox.Show("Не выбран пользователь.");
                return;
            }
            SelectedUser = (parameter as UserViewModel);
            User user = userService.GetUserById((parameter as UserViewModel).Id);
            user.EndDateTime = (user.EndDateTime.HasValue ? (DateTime?)null : DateTime.Now);
            string message = string.Empty;
            if (userService.Save(user, out message))
            {
                SelectedUser.IsActive = !user.EndDateTime.HasValue;
                if (ShowOnlyActiveUsers && !SelectedUser.IsActive)
                    Users.Remove(SelectedUser);
            }
            else
            {
                MessageBox.Show("При сохранении возникла ошибка: " + message);
                log.Error(string.Format("Failed to update user status. " + message));
            }
        }

        private void SynchWithActiveDirectory(object parameter)
        {
            if (parameter == null)
            {
                MessageBox.Show("Не выбран пользователь.");
                return;
            }
            
            /*var data = new List<UserSystemInfo>();
            data.Add(new UserSystemInfo() { UserName = "Жариков Игорь Александрович", PrincipalName = "izharikov@lpu", Enabled = true, SID = "izharikov" });
            data.Add(new UserSystemInfo() { UserName = "Жариков Петр Петрович", PrincipalName = "pzharikov@lpu", Enabled = false, SID = "pzharikov" });

            UsersAD = new ObservableCollection<UserSystemInfo>(data);*/

            UsersAD = new ObservableCollection<UserSystemInfo>(userSystemInfoService.Find(SelectedUser.UserFullName));
            if (UsersAD.Any())
                ShowPopup = true;
            else
                MessageBox.Show("В Active Directory выбранный пользователь не зарегистрирован.");
        }

        private void SelectUserAD(object parameter)
        {
            if (parameter == null)
                return;
            SelectedUserAD = parameter as UserSystemInfo;
            ShowPopup = false;

            User user = userService.GetUserById(SelectedUser.Id);
            user.SID = SelectedUserAD.SID;
            user.EndDateTime = (SelectedUserAD.Enabled ? (DateTime?)null : DateTime.Now);
            string message = string.Empty;
            if (!userService.Save(user, out message))                
            {
                MessageBox.Show("При сохранении возникла ошибка: " + message);
                log.Error(string.Format("Failed to Save user. " + message));
            }

            SelectedUser.SID = SelectedUserAD.SID;
            SelectedUser.IsActive = SelectedUserAD.Enabled;
        }

        private void AddUserPermissions()
        {

        }

        private void EditUserPermission(object parameter)
        {
            if (parameter == null)
            {
                MessageBox.Show("Не выбрано право для редактирования.");
                return;
            }
        }

        private void RemoveUserPermission(object parameter)
        {
            if (parameter == null)
            {
                MessageBox.Show("Не выбрано право для удаления.");
                return;
            }
        }
        #region Search Logic

        private async void LoadUsers()
        {
            var task = Task.Factory.StartNew(LoadUsersAsync);
            await task;
        }

        private void LoadUsersAsync()
        {         
            var collection = new ObservableCollection<UserViewModel>();
            if (searchText == string.Empty)
                collection = new ObservableCollection<UserViewModel>(userService.GetAllUsers().Select(x => new UserViewModel(x, personService)).Where(x => (ShowOnlyActiveUsers ? x.IsActive : true)).ToArray());
            else if (searchText.Length >= 3)
                collection = new ObservableCollection<UserViewModel>(userService.GetAllUsers(searchText).Select(x => new UserViewModel(x, personService)).Where(x => (ShowOnlyActiveUsers ? x.IsActive : true)).ToArray());
            else
            {
                MessageBox.Show(
                        "Слишком короткая строка для поиска.",
                        "Попробуйте снова",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                        );
            }
            
            if (!collection.Any() && Users != null)
            {
                MessageBox.Show(
                        "Не найдено ни одного совпадения.",
                        "Попробуйте снова",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                        );
            }
            else if (collection.Any())
                Users = collection;
        }       

        #endregion
    }
}
