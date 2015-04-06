using Core;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AdminTools.ViewModel
{
    class UserEditorViewModel : FailableViewModel
    {
        #region UserData

        private readonly ISimpleLocator service;
        private UserViewModel currentUser;
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

            users = new ObservableCollection<UserViewModel>(service.Instance<IUserService>().GetAllActiveUsers(DateTime.Now).Select(x => new UserViewModel(x)).ToArray());
            
            this.EditUserCommand = new RelayCommand<object>(EditUser);
            this.SearchUserCommand = new RelayCommand(this.SearchUser);
            this.NewUserCommand = new RelayCommand(this.NewUser);
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

        private RelayCommand<object> editUserCommand;
        public RelayCommand<object> EditUserCommand
        {
            get { return editUserCommand; }
            set { Set("EditUserCommand", ref editUserCommand, value); }
        }

        #endregion

        private void EditUser(object parameter)
        {
            if (parameter == null)
                return;
            MessageBox.Show(
                        "Редактирование учетной записи пользователя " + (parameter as UserViewModel).UserFullName,
                        "Info",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                        );
        }

        private void NewUser()
        {
            MessageBox.Show(
                        "Создание учетной записи пользователя. ",
                        "Info",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                        );
        }

        #region Search Logic

        private void SearchUser()
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
