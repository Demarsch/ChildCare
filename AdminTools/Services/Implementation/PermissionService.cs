using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using DataLib;

namespace AdminTools
{
    public class PermissionService : IPermissionService
    {
        private readonly IDataContextProvider dataContextProvider;

        public PermissionService(IDataContextProvider dataContextProvider)
        {
            if (dataContextProvider == null)
                throw new ArgumentNullException("dataContextProvider");
            this.dataContextProvider = dataContextProvider;
        }

        public Permission GetRootPermission()
        {            
            return dataContextProvider.GetNewDataContext().GetData<Permission>().FirstOrDefault(x => !x.PermissionLinks1.Any());           
        }

        public ICollection<Permission> GetChildren(Permission parent)
        {
            return parent.PermissionLinks.Select(x => x.Permission1).ToArray();
        }
    }
}
