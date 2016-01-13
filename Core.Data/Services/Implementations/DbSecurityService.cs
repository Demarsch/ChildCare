using System;
using System.Collections.Generic;
using System.Linq;
using Core.Services;
using Core.Extensions;

namespace Core.Data.Services
{
    public class DbSecurityService : ISecurityService
    {
        private readonly IEnvironment environment;

        private readonly IDbContextProvider contextProvider;

        private readonly ICacheService cacheService;

        private HashSet<Permission> currentUserPermissions;

        public DbSecurityService(IEnvironment environment, IDbContextProvider contextProvider, ICacheService cacheService)
        {
            if (environment == null)
            {
                throw new ArgumentNullException("environment");
            }
            if (contextProvider == null)
            {
                throw new ArgumentNullException("contextProvider");
            }
            if (cacheService == null)
            {
                throw new ArgumentNullException("cacheService");
            }
            this.environment = environment;
            this.contextProvider = contextProvider;
            this.cacheService = cacheService;
        }

        public bool HasPermission(string privilegeName)
        {
            if (currentUserPermissions == null)
            {
                currentUserPermissions = GetCurrentUserPermissions();
            }
            return currentUserPermissions.Contains(cacheService.GetItemByName<Permission>(privilegeName));
        }

        private HashSet<Permission> GetCurrentUserPermissions()
        {
            int[] userGroupsId;
            using (var context = contextProvider.CreateNewContext())
            {
                userGroupsId = context.Set<UserPermissionGroup>()
                                      .Where(x => x.UserId == environment.CurrentUser.Id)
                                      .Select(x => x.PermissionGroupId)
                                      .ToArray();
            }
            return new HashSet<Permission>(userGroupsId.Select(x => cacheService.GetItemById<PermissionGroup>(x))
                                                       .SelectMany(x => x.PermissionGroupMemberships)
                                                       .Select(x => x.Permission));
        }
    }
}