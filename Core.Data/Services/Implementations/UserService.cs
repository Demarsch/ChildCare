using System;
using System.Collections.Generic;
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

        public int GetCurrentUserId()
        {
            using (var db = contextProvider.CreateNewContext())
            {
                var curSID = GetCurrentUserSID();
                return db.Set<User>().Where(x => x.SID == curSID).Select(x => x.Id).FirstOrDefault();
            }
        }

        public User GetCurrentUser()
        {
            using (var db = contextProvider.CreateNewContext())
            {
                var curSID = GetCurrentUserSID();
                return db.Set<User>().FirstOrDefault(x => x.SID == curSID);
            }
        }

        public bool HasPersonStaff(int personStaffId)
        {
            using (var db = contextProvider.CreateNewContext())
            {
                var curSID = GetCurrentUserSID();
                return db.Set<User>().Any(x => x.SID == curSID && x.Person.PersonStaffs.Any(y => y.Id == personStaffId));
            }
        }


        public bool HasStaff(int staffId)
        {
            using (var db = contextProvider.CreateNewContext())
            {
                var curSID = GetCurrentUserSID();
                return db.Set<User>().Any(x => x.SID == curSID && x.Person.PersonStaffs.Any(y => y.StaffId == staffId));
            }
        }


        public IEnumerable<int> GetCurrentUserPersonStaffIds()
        {
            using (var db = contextProvider.CreateNewContext())
            {
                var curSID = GetCurrentUserSID();
                return db.Set<User>().Where(x => x.SID == curSID).SelectMany(x => x.Person.PersonStaffs.Select(y => y.Id)).ToArray();
            }
        }

        public IEnumerable<int> GetCurrentUserStaffIds()
        {
            using (var db = contextProvider.CreateNewContext())
            {
                var curSID = GetCurrentUserSID();
                return db.Set<User>().Where(x => x.SID == curSID).SelectMany(x => x.Person.PersonStaffs.Select(y => y.StaffId)).ToArray();
            }
        }


        public string GetCurrentUserSettingsValue(string parameterName)
        {
            using (var db = contextProvider.CreateNewContext())
            {
                var curSID = GetCurrentUserSID();
                return db.Set<UserSetting>().Where(x => x.Name.ToLower() == parameterName.ToLower() && x.User.SID == curSID).Select(x => x.Value).FirstOrDefault() ?? string.Empty;
            }
        }

        public void SetCurrentUserSettingsValue(string parameterName, string value)
        {
            using (var db = contextProvider.CreateNewContext())
            {
                var curSID = GetCurrentUserSID();
                var s = db.Set<UserSetting>().FirstOrDefault(x => x.Name.ToLower() == parameterName.ToLower() && x.User.SID == curSID);
                if (s == null)
                {
                    s = new UserSetting() { UserId = GetCurrentUserId(), Name = parameterName };
                    db.Set<UserSetting>().Add(s);
                }
                s.Value = value;
                db.SaveChanges();
            }
        }
    }
}
