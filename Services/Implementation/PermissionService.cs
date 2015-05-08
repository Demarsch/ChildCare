using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using DataLib;

namespace Core
{
    public class PermissionService : IPermissionService
    {
        private IDataContextProvider provider;

        public PermissionService(IDataContextProvider Provider)
        {
            provider = Provider;
        }

        public ICollection<Permission> GetRootPermissions()
        {
            using(var db = provider.GetNewDataContext())
            {
                return db.GetData<Permission>().Where(x => !x.PermissionLinks1.Any()).ToArray();
            }
        }

        public Permission GetPermissionById(int id)
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetById<Permission>(id);
            }
        }

        public ICollection<Permission> GetChildren(int parentId)
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<PermissionLink>().Where(x => x.ParentId == parentId).Select(x => x.Permission1).ToArray();
            }
        }
    }
}
