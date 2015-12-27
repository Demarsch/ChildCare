using System;
using System.Linq;
using System.Security.Principal;

namespace Core.Data.Services
{
    public class UserService : IUserService
    {
        private readonly IDbContextProvider contextProvider;

        public UserService(IDbContextProvider contextProvider)
        {
            if (contextProvider == null)
            {
                throw new ArgumentNullException("contextProvider");
            }
            this.contextProvider = contextProvider;
        }

        public string GetCurrentUserSID()
        {
            return WindowsIdentity.GetCurrent().User.Value;
        }

        public User GetCurrentUser()
        {
            using (var db = contextProvider.CreateNewContext())
            {
                var curSID = GetCurrentUserSID();
                return db.Set<User>().FirstOrDefault(x => x.SID != null && x.SID.ToLower() == curSID);
            }
        }
    }
}
