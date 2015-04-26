using System;
using DataLib;
using GalaSoft.MvvmLight;
using Core;

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

        public int PersonId
        {
            get { return IsEmpty ? 0 : user.PersonId; }
        }

        public string UserFullName
        {
            get { return IsEmpty ? string.Empty : user.Person.FullName; }
        }             

        public string SID
        {
            get { return IsEmpty ? string.Empty : user.SID; }
        }

        private bool isActive;
        public bool IsActive
        {
            get { return !user.EndDateTime.HasValue; }
            set { Set("IsActive", ref isActive, value); }
        }        

        public bool NameContainsText(string text)
        {
            if (String.IsNullOrEmpty(text) || String.IsNullOrEmpty(this.UserFullName))
                return false;

            return this.UserFullName.IndexOf(text, StringComparison.InvariantCultureIgnoreCase) > -1;
        }
    }
}








