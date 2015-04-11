using Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdminTools.ViewModel
{
    class UserEditorViewModel : BasicViewModel
    {
        #region UserData

        private readonly IUserService userService;
        private UserViewModel currentUser;
        private ObservableCollection<UserViewModel> users;

        #endregion

        #region Constructor

        public UserEditorViewModel(IUserService userService)
        {           
            if (userService == null)
                throw new ArgumentNullException("userService");
            this.userService = userService;         

            users = new ObservableCollection<UserViewModel>(userService.GetAllActiveUsers(DateTime.Now).Select(x => new UserViewModel(x)).ToArray());
        }    

        #endregion 

        #region Properties
              
        #region User

        public ObservableCollection<UserViewModel> Users
        {
            get { return users; }
            private set { Set("User", ref users, value); }
        }

        public UserViewModel CurrentUser
        {
            get { return currentUser; }
            set
            {
                var isPatientSelected = IsUserSelected;
                if (Set("CurrentUser", ref currentUser, value) && IsUserSelected != isPatientSelected)
                    RaisePropertyChanged("IsPatientSelected");
            }
        }

        public bool IsUserSelected { get { return currentUser != null; } }

        private UserViewModel selectedUser;
        public UserViewModel SelectedUser
        {
            get { return selectedUser; }
            set
            {
                Set("SelectedUser", ref selectedUser, value);
                if (value == null)
                    return;
                SelectUser(value);
            }
        }

        private void SelectUser(UserViewModel user)
        {
            CurrentUser = user;
        }

        #endregion

        #endregion
    }
}
