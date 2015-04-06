using System.Collections.Generic;
using DataLib;
using System;

namespace Core
{
    public interface IUserService
    {
        ICollection<User> GetAllUsers();
        ICollection<User> GetAllActiveUsers(DateTime onDate);
        ICollection<User> GetAllActiveUsers(DateTime from, DateTime to);
        EntityContext<User> GetUserById(int id);
    }
}