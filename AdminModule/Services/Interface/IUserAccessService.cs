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
    }
}
