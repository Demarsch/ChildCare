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

        public User GetCurrentUser()
        {
            using (var db = contextProvider.CreateNewContext())
            {
                var curSID = GetCurrentUserSID();
                return db.Set<User>().FirstOrDefault(x => x.SID != null && x.SID.ToLower() == curSID);
            }
        }

        public bool HasPersonStaff(int personStaffId)
        {
            using (var db = contextProvider.CreateNewContext())
            {
                var curSID = GetCurrentUserSID();
                return db.Set<User>().Any(x => x.SID != null && x.SID.ToLower() == curSID && x.Person.PersonStaffs.Any(y => y.Id == personStaffId));
            }
        }


        public bool HasStaff(int staffId)
        {
            using (var db = contextProvider.CreateNewContext())
            {
                var curSID = GetCurrentUserSID();
                return db.Set<User>().Any(x => x.SID != null && x.SID.ToLower() == curSID && x.Person.PersonStaffs.Any(y => y.StaffId == staffId));
            }
        }


        public IEnumerable<int> GetCurrentUserPersonStaffIds()
        {
            using (var db = contextProvider.CreateNewContext())
            {
                var curSID = GetCurrentUserSID();
                return db.Set<User>().Where(x => x.SID != null && x.SID.ToLower() == curSID).SelectMany(x => x.Person.PersonStaffs.Select(y => y.Id)).ToArray();
            }
        }

        public IEnumerable<int> GetCurrentUserStaffIds()
        {
            using (var db = contextProvider.CreateNewContext())
            {
                var curSID = GetCurrentUserSID();
                return db.Set<User>().Where(x => x.SID != null && x.SID.ToLower() == curSID).SelectMany(x => x.Person.PersonStaffs.Select(y => y.StaffId)).ToArray();
            }
        }
    }
}
