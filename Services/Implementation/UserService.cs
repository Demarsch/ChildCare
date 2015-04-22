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
        private IDataAccessLayer data;

        public UserService(ISimpleLocator serviceLocator)
        {
            data = serviceLocator.Instance<IDataAccessLayer>();
        }

        public bool Save(User user, out string msg)
        {
            string exception = string.Empty;
            try
            {
                data.Save<User>(user);
                msg = exception;
                return true;
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return false;
            }
        }

        public User GetUserById(int id)
        {
            return data.Cache<User>(id);
        }

        public User GetUserByPersonId(int personId)
        {
            return data.Get<User>(x => x.PersonId == personId, (x => x.Person)).FirstOrDefault();
        }

        public ICollection<User> GetAllUsers()
        {
            return data.Get<User>(x => true, (x => x.Person)).OrderBy(x => x.Person).ToArray();
        }

        public ICollection<User> GetAllUsers(string byName)
        {
            return data.Get<User>(x => x.Person.FullName.Contains(byName), (x => x.Person)).OrderBy(x => x.Person.FullName).ToArray();
        }

        public ICollection<User> GetAllActiveUsers(DateTime from, DateTime to)
        {
            return data.Get<User>(x => x.BeginDateTime < to && x.EndDateTime > from, (x => x.Person)).OrderBy(x => x.Person.FullName).ToArray();
        }

        public ICollection<User> GetAllActiveUsers(DateTime onDate)
        {
            return data.Get<User>(x => x.BeginDateTime <= onDate && (x.BeginDateTime != x.EndDateTime ? x.EndDateTime >= onDate : true), (x => x.Person)).OrderBy(x => x.Person.FullName).ToArray();
        }
    }
}
