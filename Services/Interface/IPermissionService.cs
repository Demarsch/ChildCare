using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataLib;

namespace Core
{
    public interface IPermissionService
    {
        Permission GetPermissionById(int id);
        ICollection<Permission> GetRootPermissions();
        ICollection<Permission> GetChildren(int parentId);
    }
}
