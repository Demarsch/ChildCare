using System.Collections.Generic;
using DataLib;
using System;

namespace Core
{
    public interface IUserService
    {
        ICollection<User> GetAllUsers();
        ICollection<User> GetAllUsers(string byName);
        ICollection<User> GetAllActiveUsers(DateTime onDate);
        ICollection<User> GetAllActiveUsers(DateTime from, DateTime to);
        ICollection<UserPermission> GetUserPermissions(int userId);
        User GetUserById(int id);
        User GetCurrentUser();
        User GetUserByPersonId(int personId);
        bool Save(User user, out string msg);
    }
}
