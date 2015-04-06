using System;
using DataLib;
using GalaSoft.MvvmLight;

namespace AdminTools.ViewModel
{
    public class UserViewModel : ObservableObject
    {
        private readonly User user;

        public UserViewModel(User user)
        {
            this.user = user;
        }

        public bool IsEmpty
        {
            get { return user == null; }
        }
        
        public int Id
        {
            get { return IsEmpty ? 0 : user.Id; }
        }

        public string UserFullName
        {
            get { return IsEmpty ? string.Empty : user.Person.FullName; }
        }

        public string SID
        {
            get { return IsEmpty ? string.Empty : user.SID; }
        }
        
    }
}








