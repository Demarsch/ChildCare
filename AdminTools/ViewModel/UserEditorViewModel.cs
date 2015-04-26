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

namespace AdminTools.ViewModel
{
    class UserEditorViewModel : ObservableObject
    {
        #region UserData

        private readonly ISimpleLocator service;
        private ObservableCollection<UserViewModel> users;
        public RelayCommand SearchUserCommand { get; private set; }
        private string searchText = String.Empty;
        public RelayCommand NewUserCommand { get; private set; }
        #endregion

        #region Constructor

        public UserEditorViewModel(ISimpleLocator service)
        {
            if (service == null)
                throw new ArgumentNullException("userService");
            this.service = service;
                       
            this.EditUserCommand = new RelayCommand<object>(EditUser);
            this.SearchUserCommand = new RelayCommand(this.LoadUsers);
            this.NewUserCommand = new RelayCommand(this.NewUser);
            this.UpdateUserStatusCommand = new RelayCommand<object>(UpdateUserStatus);
            FilterChecked = true;
            LoadUsers();
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
            set { Set("SelectedUser", ref selectedUser, value); }
        }       

        public string SearchText
        {
            get { return searchText; }
            set { Set("SearchText", ref searchText, value); }
        }

        private bool filterChecked;
        public bool FilterChecked
        {
            get { return filterChecked; }
            set 
            {
                if (!Set("FilterChecked", ref filterChecked, value))
                    return;
                LoadUsersAsync();
            }
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

        #endregion

        private void EditUser(object parameter)
        {
            if (parameter == null)
            {
                MessageBox.Show("Не выбран пользователь.");
                return;
            }
            var personViewModel = new EditPersonViewModel(this.service.Instance<ILog>(), this.service.Instance<IPersonService>(), (parameter as UserViewModel).PersonId);
            (new EditPersonView() { DataContext = personViewModel, WindowStartupLocation = WindowStartupLocation.CenterScreen }).ShowDialog();

            //TODO: Change verification way
            if (personViewModel.EditPersonDataViewModel == null || personViewModel.EditPersonDataViewModel.TextMessage != "Данные сохранены") return;

            LoadUsersAsync();
            SelectedUser = Users.FirstOrDefault(x => x.Id == (parameter as UserViewModel).Id);
        }

        private void NewUser()
        {
            var userAccountModelView = new UserAccountViewModel(this.service);
            (new UserAccountView() { DataContext = userAccountModelView }).ShowDialog();

            LoadUsers();
        }

        private void UpdateUserStatus(object parameter)
        {
            if (parameter == null) return;
            User user = this.service.Instance<IUserService>().GetUserById((parameter as UserViewModel).Id);
            user.EndDateTime = (user.EndDateTime.HasValue ? (DateTime?)null : DateTime.Now);
            string message = string.Empty;
            if (this.service.Instance<IUserService>().Save(user, out message))
            {
                LoadUsersAsync();
                SelectedUser = Users.FirstOrDefault(x => x.Id == user.Id);
            }
            else
            {
                MessageBox.Show("При сохранении возникла ошибка: " + message);
                this.service.Instance<ILog>().Error(string.Format("Failed to update user status. " + message));
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
            if (searchText == string.Empty)
                Users = new ObservableCollection<UserViewModel>(service.Instance<IUserService>().GetAllUsers().Select(x => new UserViewModel(x)).ToArray());
            else if (searchText.Length >= 3)
                Users = new ObservableCollection<UserViewModel>(service.Instance<IUserService>().GetAllUsers(searchText).Select(x => new UserViewModel(x)).ToArray());
            else
            {
                MessageBox.Show(
                        "Слишком короткая строка для поиска.",
                        "Попробуйте снова",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                        );
            }
            Users = new ObservableCollection<UserViewModel>(Users.Where(x => (FilterChecked ? x.IsActive : true)));

            if (!users.Any())
            {
                MessageBox.Show(
                        "Не найдено ни одного совпадения.",
                        "Попробуйте снова",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                        );
            }            
        }       

        #endregion
    }
}
