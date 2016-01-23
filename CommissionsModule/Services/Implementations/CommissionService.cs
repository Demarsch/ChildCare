using Core.Data;
using Core.Data.Misc;
using Core.Data.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace CommissionsModule.Services
{
    public class CommissionService : ICommissionService
    {
        private readonly IDbContextProvider contextProvider;
        private readonly IUserService userService;

        public CommissionService(IDbContextProvider contextProvider, IUserService userService)
        {
            if (contextProvider == null)
            {
                throw new ArgumentNullException("contextProvider");
            }
            if (userService == null)
            {
                throw new ArgumentNullException("userService");
            }
            this.contextProvider = contextProvider;
            this.userService = userService;
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
        
        public IDisposableQueryable<CommissionProtocol> GetCommissionProtocols(int filterId, DateTime? date = null, bool onlyMyCommissions = false)
        {
            var context = contextProvider.CreateNewContext();
            var option = context.Set<CommissionFilter>().First(x => x.Id == filterId).Options;
                       
            IQueryable<CommissionProtocol> query = context.Set<CommissionProtocol>();
            if (date.HasValue)
                query = query.Where(x => x.ProtocolDate == date.Value);

            // TODO: Apply Filters   
            if (option.Contains(OptionValues.ProtocolsInProcess))
                query = query.Where(x => x.IsCompleted == false);
            if (option.Contains(OptionValues.ProtocolsPreliminary))
                query = query.Where(x => x.IsCompleted == true);
            if (option.Contains(OptionValues.ProtocolsOnCommission))
                query = query.Where(x => x.IsCompleted == null);
            if (option.Contains(OptionValues.ProtocolsOnDate))
                query = query.Where(x => x.ProtocolDate == date.Value);
            if (option.Contains(OptionValues.ProtocolsAdded))
                query = query.Where(x => x.ToDoDateTime == date.Value);
            if (option.Contains(OptionValues.ProtocolsAwaiting))
                query = query.Where(x => true);

            if (onlyMyCommissions)
            {
                int currentPersonId = userService.GetCurrentUser().PersonId;
                query = query.Where(x => x.CommissionDecisions.Any(a => a.CommissionMember.PersonStaff.PersonId == currentPersonId));
            }
            return new DisposableQueryable<CommissionProtocol>(query, context);
        }

        public SolidColorBrush GetColor(string option)
        {
            using (var context = contextProvider.CreateNewContext())
            {
                var color = context.Set<ColorsSetting>().FirstOrDefault(x => x.Options.Contains(option));
                if (color != null)
                    return (SolidColorBrush)new BrushConverter().ConvertFromString(color.ColorName);
                return new SolidColorBrush(Colors.White);
            }
        }

        public IDisposableQueryable<CommissionDecision> GetCommissionDecisions(int commissionProtocolId)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<CommissionDecision>(context.Set<CommissionDecision>().Where(x => x.CommissionProtocolId == commissionProtocolId), context);
        }
    }
}
