using System;
using System.Linq;
using Core.DTO;
using Core.Misc;

namespace Core.Data.Services
{
    public class DbEnvironment : IEnvironment
    {
        private readonly IDbContextProvider dataContextProvider;

        public DbEnvironment(IDbContextProvider dataContextProvider)
        {
            if (dataContextProvider == null)
            {
                throw new ArgumentNullException("dataContextProvider");
            }
            this.dataContextProvider = dataContextProvider;
        }

        private UserDTO currentUser;

        public UserDTO CurrentUser
        {
            get
            {
                if (currentUser == null)
                {
                    var userLogin = dataContextProvider.SharedContext.Database.SqlQuery<string>("SELECT SYSTEM_USER").First();
                    using (var context = dataContextProvider.CreateNewContext())
                    {
                        currentUser = context.Set<User>().Where(x => x.SID == userLogin)
                                             .Select(x => new UserDTO
                                                          {
                                                              PersonFullName = x.Person.FullName,
                                                              PersonId = x.PersonId,
                                                              PersonShortName = x.Person.ShortName,
                                                              UserId = x.Id
                                                          })
                                             .FirstOrDefault();
                        if (currentUser == null)
                        {
                            throw new ApplicationException(string.Format("Database doesn't contain information about user with login '{0}'", userLogin));
                        }
                    }
                }
                return currentUser;
            }
        }

        public DateTime CurrentDate
        {
            get
            {
                return dataContextProvider.SharedContext
                    .Database
                    .SqlQuery<DateTime>("SELECT GETDATE()")
                    .First();
            }
        }
    }
}
