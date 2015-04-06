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
        private readonly IDataContextProvider dataContextProvider;

        public UserService(IDataContextProvider dataContextProvider)
        {
            if (dataContextProvider == null)
                throw new ArgumentNullException("dataContextProvider");
            this.dataContextProvider = dataContextProvider;
        }

        public EntityContext<User> GetUserById(int id)
        {
            var context = dataContextProvider.GetNewDataContext();
            return new EntityContext<User>(context.GetData<User>().Single(x => x.Id == id), context);
        }

        public ICollection<User> GetAllUsers()
        {
            return dataContextProvider.GetNewDataContext().GetData<User>().OrderBy(x => x.Person.FullName).ToArray();
        }

        public ICollection<User> GetAllActiveUsers(DateTime from, DateTime to)
        {
            return dataContextProvider.GetNewDataContext().GetData<User>().Where(x => x.BeginDateTime < to && x.EndDateTime > from).OrderBy(x => x.Person.FullName).ToArray();
        }

        public ICollection<User> GetAllActiveUsers(DateTime onDate)
        {
            return dataContextProvider.GetNewDataContext().GetData<User>().Where(x => x.BeginDateTime >= onDate && x.EndDateTime <= onDate).OrderBy(x => x.Person.FullName).ToArray();
        }
    }
}
