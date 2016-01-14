using System.Collections.Generic;
using System.Threading.Tasks;
using AdminModule.Model;
using Core.Data;

namespace AdminModule.Services
{
    public interface IUserAccessService
    {
        Task<IEnumerable<Permission>> GetPermissionsAsync();

        Task<IEnumerable<PermissionGroup>> GetGroupsAsync();

        Task<IEnumerable<UserDTO>> GetUsersAsync();

        Task AddUserToGroupAsync(int userId, int groupId);

        Task RemoveUserFromGroupAsync(int userId, int groupId);

        Task AddPermissionToGroupAsync(int permissionId, int groupId);

        Task RemovePermissionFromGroupAsync(int permissionId, int groupId);

        Task<PermissionGroup> CreateNewPermissionGroupAsync(string name, string description);

        Task<PermissionGroup> SavePermissionGroupAsync(string newName, string newDescription, PermissionGroup currentGroup);

        Task DeletePermissionGroupAsync(PermissionGroup group);

        Task ChangeUserActivationAsync(User user);
    }
}
