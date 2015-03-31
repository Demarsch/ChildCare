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

        public ICollection<Permission> GetRootPermissions()
        {            
            return dataContextProvider.GetNewDataContext().GetData<Permission>().Where(x => !x.PermissionLinks1.Any()).ToArray();           
        }
    }
}
