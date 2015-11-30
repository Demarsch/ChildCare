using System.Collections.Generic;
using System;
using Core.Data.Classes;

namespace Core.Data.Services
{
    public interface IUserService
    {
        List<Permission> GetCurrentUserPermissions();

        /// <summary>
        /// Check if user has this permission
        /// </summary>
        /// <param name="permission">permission name. Attention! Use PermissionType class for it!</param>
        /// <returns></returns>
        bool HasPermission(string permission);

        User GetUserBySID();
    }
}
