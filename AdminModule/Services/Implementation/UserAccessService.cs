using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using AdminModule.Model;
using Core.Data;
using Core.Data.Misc;
using Core.Data.Services;
using Core.Services;

namespace AdminModule.Services
{
    public class UserAccessService : IUserAccessService
    {
        private readonly ICacheService cacheService;

        private readonly IDbContextProvider contextProvider;

        public UserAccessService(ICacheService cacheService, IDbContextProvider contextProvider)
        {
            if (cacheService == null)
            {
                throw new ArgumentNullException("cacheService");
            }
            if (contextProvider == null)
            {
                throw new ArgumentNullException("contextProvider");
            }
            this.cacheService = cacheService;
            this.contextProvider = contextProvider;
        }

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return Task.Factory.StartNew(() => cacheService.GetItems<Permission>());
        }

        public Task<IEnumerable<PermissionGroup>> GetGroupsAsync()
        {
            return Task.Factory.StartNew(() => cacheService.GetItems<PermissionGroup>());
        }

        public async Task<IEnumerable<UserDTO>> GetUsersAsync()
        {
            using (var context = contextProvider.CreateLightweightContext())
            {
                return await context.Set<User>()
                              .Select(x => new UserDTO
                                           {
                                               ActiveFrom = x.BeginDateTime,
                                               ActiveTo = x.EndDateTime,
                                               BirthDate = x.Person.BirthDate,
                                               IsMale = x.Person.IsMale,
                                               FullName = x.Person.FullName,
                                               PersonId = x.PersonId,
                                               PhotoData = x.Person.Document.FileData,
                                               Sid = x.SID,
                                               UserId = x.Id
                                           })
                              .ToArrayAsync();
            }
        }
    }
}