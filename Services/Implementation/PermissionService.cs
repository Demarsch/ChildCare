using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using DataLib;

namespace Core
{
    public class PermissionService : IPermissionService
    {
        private IDataAccessLayer data;

        public PermissionService(ISimpleLocator serviceLocator)
        {
            data = serviceLocator.Instance<IDataAccessLayer>();
        }

        public ICollection<Permission> GetRootPermissions()
        {
            return data.Get<Permission>(x => !x.PermissionLinks1.Any()).ToArray();           
        }

        public Permission GetPermissionById(int id)
        {
            return data.Cache<Permission>(id);
        }

        public ICollection<Permission> GetChildren(int parentId)
        {
            return data.Get<PermissionLink>(x => x.ParentId == parentId, x => x.Permission1).Select(x => x.Permission1).ToArray();
        }
    }
}
