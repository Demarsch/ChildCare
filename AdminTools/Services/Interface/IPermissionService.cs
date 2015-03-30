using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataLib;

namespace AdminTools
{
    public interface IPermissionService
    {
        ICollection<Permission> GetRootPermissions();
    }
}
