using System;
using AdminModule.Model;
using Prism.Mvvm;

namespace AdminModule.ViewModels
{
    public class UserViewModel : BindableBase
    {
        public UserViewModel(UserDTO user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            FullName = user.FullName;
        }

        private string fullName;

        public string FullName
        {
            get { return fullName; }
            set { SetProperty(ref fullName, value); }
        }
    }
}