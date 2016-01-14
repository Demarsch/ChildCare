using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using AdminModule.Model;
using Core.Data;
using Core.Data.Misc;
using Core.Data.Services;
using Core.Extensions;
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
            var group = cacheService.GetItemById<PermissionGroup>(groupId);
            if (group.UserPermissionGroups.Any(x => x.UserId == userId))
            {
                return;
            }
            var userGroupMembership = new UserPermissionGroup { PermissionGroupId = groupId, UserId = userId };
            await cacheService.AddItemAsync(userGroupMembership);
        }

        public async Task RemoveUserFromGroupAsync(int userId, int groupId)
        {
            var group = cacheService.GetItemById<PermissionGroup>(groupId);
            var userGroupMembership = group.UserPermissionGroups.FirstOrDefault(x => x.UserId == userId);
            if (userGroupMembership == null)
            {
                return;
            }
            await cacheService.RemoveItemAsync(userGroupMembership);
        }

        public async Task AddPermissionToGroupAsync(int permissionId, int groupId)
        {
            var group = cacheService.GetItemById<PermissionGroup>(groupId);
            if (group.PermissionGroupMemberships.Any(x => x.PermissionId == permissionId))
            {
                return;
            }
            var permissionGroupMembership = new PermissionGroupMembership() { GroupId = groupId, PermissionId = permissionId };
            await cacheService.AddItemAsync(permissionGroupMembership);
        }

        public async Task RemovePermissionFromGroupAsync(int permissionId, int groupId)
        {
            var group = cacheService.GetItemById<PermissionGroup>(groupId);
            var permissionGroupMembership = group.PermissionGroupMemberships.FirstOrDefault(x => x.PermissionId == permissionId);
            if (permissionGroupMembership == null)
            {
                return;
            }
            await cacheService.RemoveItemAsync(permissionGroupMembership);
        }

        public async Task<PermissionGroup> CreateNewPermissionGroupAsync(string name, string description)
        {
            name = name.Trim();
            var groupWithTheSameName = cacheService.GetItemByName<PermissionGroup>(name);
            if (groupWithTheSameName != null)
            {
                throw new DataException("Group with the same name already exists");
            }
            var result = new PermissionGroup
            {
                Name = name,
                Description = description,
                PermissionGroupMemberships = new HashSet<PermissionGroupMembership>(),
                UserPermissionGroups = new HashSet<UserPermissionGroup>(),
            };
            await Task.Factory.StartNew(() => cacheService.AddItem(result));
            return result;
        }

        public async Task<PermissionGroup> SavePermissionGroupAsync(string newName, string newDescription, PermissionGroup currentGroup)
        {
            newName = newName.Trim();
            newDescription = (newDescription ?? string.Empty).Trim();
            var groupWithTheSameName = cacheService.GetItemByName<PermissionGroup>(newName);
            if (groupWithTheSameName != null && groupWithTheSameName.Id != currentGroup.Id)
            {
                throw new DataException("Group with the same name already exists");
            }
            currentGroup.Name = newName;
            currentGroup.Description = newDescription;
            await cacheService.UpdateItemAsync(currentGroup);
            return currentGroup;
        }

        public async Task DeletePermissionGroupAsync(PermissionGroup group)
        {
            await cacheService.RemoveItemAsync(group);
        }

        public async Task ChangeUserActivationAsync(User user)
        {
            user.EndDateTime = user.EndDateTime == SpecialValues.MaxDate ? DateTime.Today.AddDays(-1.0) : SpecialValues.MaxDate;
            await cacheService.UpdateItemAsync(user);
        }
    }
}