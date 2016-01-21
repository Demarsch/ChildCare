using Core.Data;
using Core.Data.Misc;
using Core.Data.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommissionsModule.Services
{
    public class CommissionService : ICommissionService
    {
        private readonly IDbContextProvider contextProvider;

        public CommissionService(IDbContextProvider contextProvider)
        {
            if (contextProvider == null)
            {
                throw new ArgumentNullException("contextProvider");
            }
            this.contextProvider = contextProvider;
        }

        public IDisposableQueryable<CommissionFilter> GetCommissionFilters(string options = "")
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<CommissionFilter>(context.Set<CommissionFilter>().Where(x => string.IsNullOrEmpty(options) ? true : x.Options.Contains(options)), context);
        }


        public bool IsCommissionFilterHasDate(int id)
        {
            using (var context = contextProvider.CreateNewContext())
            {
                var filter = context.Set<CommissionFilter>().FirstOrDefault(x => x.Id == id);
                if (filter != null)
                    return filter.Options.Contains(OptionValues.CommissionFilterHasDate);
                return false;
            }
        }
        
        public IDisposableQueryable<CommissionProtocol> GetCommissionProtocols(int filterId, DateTime? date, bool onlyMyCommissions = false)
        {
            var context = contextProvider.CreateNewContext();
            IQueryable<CommissionProtocol> query = null;
            var option = context.Set<CommissionFilter>().First(x => x.Id == filterId).Options;

            // TODO: Apply Filters
            if (onlyMyCommissions)
                query = context.Set<CommissionProtocol>().Where(x => x.IsCompleted == false); 
            if (date.HasValue)
                query = context.Set<CommissionProtocol>().Where(x => x.IsCompleted == false && x.ProtocolDate == date.Value);

            if (option.Contains(OptionValues.ProtocolsInProcess))
                query = context.Set<CommissionProtocol>().Where(x => x.IsCompleted == false);
            if (option.Contains(OptionValues.ProtocolsPreliminary))
                query = context.Set<CommissionProtocol>().Where(x => x.IsCompleted == false);
            if (option.Contains(OptionValues.ProtocolsOnCommission))
                query = context.Set<CommissionProtocol>().Where(x => x.IsCompleted == false);
            if (option.Contains(OptionValues.ProtocolsOnDate))
                query = context.Set<CommissionProtocol>().Where(x => x.IsCompleted == false);
            if (option.Contains(OptionValues.ProtocolsAdded))
                query = context.Set<CommissionProtocol>().Where(x => x.IsCompleted == false);
            if (option.Contains(OptionValues.ProtocolsAwaiting))
                query = context.Set<CommissionProtocol>().Where(x => x.IsCompleted == false);            
            
            return new DisposableQueryable<CommissionProtocol>(query, context);
        }
    }
}
