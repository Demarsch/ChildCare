using System;
using System.Linq;

namespace Core.Data.Services
{
    public class DbEnvironment : IEnvironment
    {
        private readonly IDbContextProvider dataContextProvider;

        private readonly IUserService userService;

        public DbEnvironment(IDbContextProvider dataContextProvider, IUserService userService)
        {
            if (dataContextProvider == null)
            {
                throw new ArgumentNullException("dataContextProvider");
            }
            if (userService == null)
            {
                throw new ArgumentNullException("userService");
            }
            this.dataContextProvider = dataContextProvider;
            this.userService = userService;
        }

        private User currentUser;

        public User CurrentUser
        {
            get
            {
                if (currentUser == null)
                {
                    var userSid = userService.GetCurrentUserSID();
                    currentUser = userService.GetCurrentUser();
                    if (currentUser == null)
                    {
                        throw new ApplicationException(string.Format("Database doesn't contain information about user with SID '{0}'", userSid));
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
