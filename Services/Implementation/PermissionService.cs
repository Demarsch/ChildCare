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

        public ICollection<Permission> GetPermissionsByName(string name)
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<Permission>().Where(x => x.Name.StartsWith(name)).ToArray();
            }
        }
        
        public bool Save(Permission permission, out string msg)
        {
            string exception = string.Empty;
            try
            {
                using (var db = provider.GetNewDataContext())
                {
                    var dbpermission = permission.Id > 0 ? db.GetById<Permission>(permission.Id) : new Permission();
                    dbpermission.Description = permission.Description;
                    dbpermission.IsGroup = permission.IsGroup;
                    dbpermission.Name = permission.Name;
                    dbpermission.ReadOnly = permission.ReadOnly;
                    db.Save();
                    msg = exception;
                    return true;
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return false;
            }
        }

        public bool Delete(Permission permission, out string msg)
        {
            string exception = string.Empty;
            try
            {
                using (var db = provider.GetNewDataContext())
                {
                    db.Remove<Permission>(db.GetById<Permission>(permission.Id));
                    db.Save();
                    msg = exception;
                    return true;
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return false;
            }
        }
    }
}
