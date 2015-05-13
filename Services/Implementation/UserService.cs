using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DataLib;

namespace Core
{
    public class UserService : IUserService
    {
        private IDataContextProvider provider;

        public UserService(IDataContextProvider Provider)
        {
            provider = Provider;
        }

        public bool Save(User user, out string msg)
        {
            string exception = string.Empty;
            try
            {
                using (var db = provider.GetNewDataContext())
                {
                    var dbuser = user.Id > 0 ? db.GetById<User>(user.Id) : new User();
                    dbuser.PersonId = user.PersonId;
                    dbuser.SID = user.SID;
                    dbuser.BeginDateTime = user.BeginDateTime;
                    dbuser.EndDateTime = user.EndDateTime;
                    if (dbuser.Id == 0)
                        db.Add<User>(dbuser);
                    db.Save();
                    msg = exception;
                    return true;
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return false;
            }
        }

        // ????????????????
        public User GetCurrentUser()
        {
            using (var db = provider.GetNewDataContext())
            {
                string login = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                return db.GetData<User>().FirstOrDefault(x => x.SID == login && !x.EndDateTime.HasValue);
            }
        }

        public User GetUserById(int id)
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetById<User>(id);
            }
        }

        public User GetUserByPersonId(int personId)
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<User>().FirstOrDefault(x => x.PersonId == personId);
            }
        }

        public ICollection<User> GetAllUsers()
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<User>().OrderBy(x => x.Person.FullName).ToArray();
            }
        }

        public ICollection<User> GetAllUsers(string byName)
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<User>().Where(x => x.Person.FullName.Contains(byName)).OrderBy(x => x.Person.FullName).ToArray();
            }
        }

        public ICollection<User> GetAllActiveUsers(DateTime from, DateTime to)
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<User>().Where(x => x.BeginDateTime < to && x.EndDateTime > from).OrderBy(x => x.Person.FullName).ToArray();
            }
        }

        public ICollection<User> GetAllActiveUsers(DateTime onDate)
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<User>().Where(x => x.BeginDateTime <= onDate && (x.BeginDateTime != x.EndDateTime ? x.EndDateTime >= onDate : true)).OrderBy(x => x.Person.FullName).ToArray();
            }
        }

        public ICollection<UserPermission> GetUserPermissions(int userId)
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<UserPermission>().Where(x => x.UserId == userId).OrderBy(x => x.Permission.Name).ToArray();
            }
        }
    }
}
