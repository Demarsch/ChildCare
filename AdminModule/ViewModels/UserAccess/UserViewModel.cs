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
            Id = user.Id;
            FullName = user.FullName;
        }

        private string fullName;

        public string FullName
        {
            get { return fullName; }
            set { SetProperty(ref fullName, value); }
        }

        public int Id { get; private set; }
    }
}