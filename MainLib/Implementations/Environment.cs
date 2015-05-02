﻿using System;

namespace Core
{
    public class Environment : IEnvironment
    {
        private readonly IDataContextProvider dataContextProvider;

        public Environment(IDataContextProvider dataContextProvider)
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
                //TODO: get current user from data context
                return currentUser ??
                       (currentUser =
                           new UserDTO
                           {
                               UserId = 1,
                               PersonId = 1,
                               PersonFullName = "Adminov Admin Adminovich",
                               PersonShortName = "Adminov A.A."
                           });
            }
        }
    }
}
