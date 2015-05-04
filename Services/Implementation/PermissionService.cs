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

        public bool Save(Permission permission, out string msg)
        {
            string exception = string.Empty;
            try
            {
                data.Save<Permission>(permission);
                msg = exception;
                return true;
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
                Remove(permission.PermissionLinks);
                var child = GetPermissionById(permission.Id);
                data.Delete<PermissionLink>(child.PermissionLinks1.ToArray());
                data.Delete<Permission>(child);
                msg = exception;
                return true;
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return false;
            }
        }

        private void Remove(ICollection<PermissionLink> links)
        {
            foreach (var item in links)
            {
                var child = GetPermissionById(item.ChildId);
                if (child.PermissionLinks.Any())
                {
                    Remove(child.PermissionLinks);
                    child = GetPermissionById(item.ChildId);
                }
                data.Delete<PermissionLink>(child.PermissionLinks1.ToArray());
                data.Delete<Permission>(child);                
            }            
        }

        public ICollection<Permission> GetRootPermissions()
        {
            return data.Get<PermissionLink>(x => !x.ParentId.HasValue, (x => x.Permission1)).Select(x => x.Permission1).ToArray();        
        }

        public Permission GetPermissionById(int id)
        {
            return data.First<Permission>(x => x.Id == id, (x => x.PermissionLinks1), (x => x.PermissionLinks));
        }

        public ICollection<Permission> GetChildren(int parentId)
        {
            return data.Get<PermissionLink>(x => x.ParentId == parentId, x => x.Permission1).Select(x => x.Permission1).ToArray();
        }

        public ICollection<Permission> GetPermissionsByName(string name)
        {
            return data.Get<Permission>(x => x.Name.ToLower().Trim().Contains(name.ToLower().Trim())).ToArray();
        }
    }
}
