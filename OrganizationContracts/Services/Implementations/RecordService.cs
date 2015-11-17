using System;
using System.Linq;
using Core.Data;
using Core.Data.Misc;
using Core.Data.Services;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Threading;

namespace OrganizationContractsModule.Services
{
    public class RecordService : IRecordService
    {
        private readonly IDbContextProvider contextProvider;

        public RecordService(IDbContextProvider contextProvider)
        {
            if (contextProvider == null)
            {
                throw new ArgumentNullException("contextProvider");
            }
            this.contextProvider = contextProvider;
        }        
    }
}
