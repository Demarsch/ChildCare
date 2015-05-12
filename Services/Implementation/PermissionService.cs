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
                return db.GetData<PermissionLink>().Where(x => !x.ParentId.HasValue).Select(x => x.Permission1).ToArray();
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
        
        public int Save(Permission permission, int? parentId)
        {           
            using (var db = provider.GetNewDataContext())
            {
                var dbpermission = permission.Id > 0 ? db.GetById<Permission>(permission.Id) : new Permission();
                dbpermission.Description = permission.Description;
                dbpermission.IsGroup = permission.IsGroup;
                dbpermission.Name = permission.Name;
                dbpermission.ReadOnly = permission.ReadOnly;

                if (dbpermission.Id == 0)
                {
                    dbpermission.PermissionLinks1.Add(new PermissionLink() { ParentId = parentId });
                    db.Add<Permission>(dbpermission);
                }
                db.Save();
                return dbpermission.Id;
            }            
        }

        public void Delete(Permission permission)
        {
            using (var db = provider.GetNewDataContext())            
                Remove(db.GetById<Permission>(permission.Id).PermissionLinks, db);
            
            using (var db = provider.GetNewDataContext())
            {
                var curPermission = db.GetById<Permission>(permission.Id);
                db.RemoveRange<PermissionLink>(curPermission.PermissionLinks1);
                db.Remove<Permission>(curPermission);
                db.Save();
            }           
        }

        private void Remove(ICollection<PermissionLink> links, IDataContext db)
        {            
            foreach (var item in links.ToList())
            {
                var child = db.GetById<Permission>(item.ChildId);
                if (child.PermissionLinks.Any())
                    Remove(child.PermissionLinks, db);
                db.Remove<PermissionLink>(db.GetById<PermissionLink>(item.Id));
                db.Remove<Permission>(db.GetById<Permission>(item.ChildId));
                db.Save();
            }            
        }
    }
}


/*
 *              var oldPermissionsLinks = dbpermission.PermissionLinks1.ToList();
                var newPermissionsLinks = permission.PermissionLinks1.ToList();
                int oldCount = oldPermissionsLinks.Count;
                int newCount = newPermissionsLinks.Count;
                while (oldCount > newCount)
                {
                    db.Remove<PermissionLink>(oldPermissionsLinks[oldCount - 1]);
                    oldPermissionsLinks.RemoveAt(oldCount - 1);
                }
                int j = oldCount;
                while (j < newCount)
                {
                    PermissionLink link = new PermissionLink();
                    db.Add<PermissionLink>(link);
                    oldPermissionsLinks.Add(link);
                    j++;
                }
                for (int i = 0; i < newCount; i++)
                {
                    oldPermissionsLinks[i].ParentId = newPermissionsLinks[i].ParentId;
                    oldPermissionsLinks[i].ChildId = dbpermission.Id;
                }
                db.Save();*/