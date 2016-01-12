using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
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
                                               Id = x.Id
                                           })
                              .ToArrayAsync();
            }
        }

        public async Task AddUserToGroupAsync(int userId, int groupId)
        {
            var @group = cacheService.GetItemById<PermissionGroup>(groupId);
            if (@group.UserPermisionGroups.Any(x => x.UserId == userId))
            {
                return;
            }
            var userGroupMembership = new UserPermisionGroup { PermissionGroupId = groupId, UserId = userId };
            using (var context = contextProvider.CreateLightweightContext())
            {
                context.Entry(userGroupMembership).State = EntityState.Added;
                await context.SaveChangesAsync();
            }
            @group.UserPermisionGroups.Add(userGroupMembership);
            userGroupMembership.PermissionGroup = @group;
        }

        public async Task RemoveUserFromGroupAsync(int userId, int groupId)
        {
            var @group = cacheService.GetItemById<PermissionGroup>(groupId);
            var itemToRemove = @group.UserPermisionGroups.FirstOrDefault(x => x.UserId == userId);
            if (itemToRemove == null)
            {
                return;
            }
            @group.UserPermisionGroups.Remove(itemToRemove);
            using (var context = contextProvider.CreateLightweightContext())
            {
                context.Entry(itemToRemove).State = EntityState.Deleted;
                await context.SaveChangesAsync();
            }
        }

        public async Task AddPermissionToGroupAsync(int permissionId, int groupId)
        {
            throw new NotImplementedException();
        }

        public async Task RemovePermissionFromGroupAsync(int permissionId, int groupId)
        {
            throw new NotImplementedException();
        }

        public async Task<PermissionGroup> CreateNewPermissionGroupAsync(string name, string description)
        {
            name = name.Trim();
            using (var context = contextProvider.CreateLightweightContext())
            {
                var groupWithTheSameName = await context.Set<PermissionGroup>().FirstOrDefaultAsync(x => x.Name == name);
                if (groupWithTheSameName != null)
                {
                    throw new DataException("Group with the same name already exists");
                }
                var result = new PermissionGroup
                {
                    Name = name,
                    Description = description,
                    PermissionGroupMemberships = new HashSet<PermissionGroupMembership>(),
                    UserPermisionGroups = new HashSet<UserPermisionGroup>(),
                    PermissionGroups1 = new HashSet<PermissionGroup>()
                };
                context.Entry(result).State = EntityState.Added;
                await context.SaveChangesAsync();
                cacheService.InvalidateCache<PermissionGroup>();
                return result;
            }
        }

        public async Task<PermissionGroup> SavePermissionGroupAsync(string newName, string newDescription, PermissionGroup currentGroup)
        {
            newName = newName.Trim();
            newDescription = newDescription.Trim();
            var oldName = currentGroup.Name;
            var oldDescription = currentGroup.Description;
            DbContext context = null;
            try
            {
                context = contextProvider.CreateLightweightContext();
                var groupWithTheSameName = await context.Set<PermissionGroup>().FirstOrDefaultAsync(x => x.Name == newName && x.Id != currentGroup.Id);
                if (groupWithTheSameName != null)
                {
                    throw new DataException("Group with the same name already exists");
                }
                currentGroup.Name = newName;
                currentGroup.Description = newDescription;
                context.Entry(currentGroup).State = EntityState.Modified;
                await context.SaveChangesAsync();
                cacheService.InvalidateCache<PermissionGroup>();
                return currentGroup;
            }
            catch
            {
                currentGroup.Name = oldName;
                currentGroup.Description = oldDescription;
                throw;
            }
            finally
            {
                if (context != null)
                {
                    context.Dispose();
                }
            }

        }
    }
}