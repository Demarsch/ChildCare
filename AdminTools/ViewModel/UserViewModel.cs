using System;
using DataLib;
using GalaSoft.MvvmLight;
using Core;

namespace AdminTools.ViewModel
{
    public class UserViewModel : ObservableObject
    {
        private readonly User user;
        private IPersonService personService;

        public UserViewModel(User user, IPersonService personService)
        {
            this.user = user;
            this.personService = personService;
            var person = personService.GetPersonById(user.PersonId != 0 ? user.PersonId : user.Person.Id);
            sid = !IsEmpty ? user.SID : string.Empty;
            isActive = !IsEmpty ? !user.EndDateTime.HasValue : false;
            userFullName = !IsEmpty ? person.FullName : string.Empty;
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
                
        private string userFullName;
        public string UserFullName
        {
            get { return userFullName; }
            set { Set("UserFullName", ref userFullName, value); }
        }

        private string sid;
        public string SID
        {
            get { return sid; }
            set { Set("SID", ref sid, value); }
        }

        private bool isActive;
        public bool IsActive
        {
            get { return isActive; }
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








