using Core.Data;
using Core.Data.Misc;
using Core.Data.Services;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
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

            if (option.Contains(OptionValues.ProtocolsInProcess))
                query = query.Where(x => x.IsCompleted == false);
            if (option.Contains(OptionValues.ProtocolsPreliminary))
                query = query.Where(x => x.IsCompleted == null);
            if (option.Contains(OptionValues.ProtocolsOnCommission))
                query = query.Where(x => x.IsCompleted == false && x.IsExecuting == true);
            if (option.Contains(OptionValues.ProtocolsOnDate))
                query = query.Where(x => x.ProtocolDate == date.Value);
            if (option.Contains(OptionValues.ProtocolsAdded))
                query = query.Where(x => x.IncomeDateTime == date.Value);
            if (option.Contains(OptionValues.ProtocolsAwaiting))
                query = query.Where(x => x.IsCompleted == true && EntityFunctions.TruncateTime(x.ToDoDateTime) > EntityFunctions.TruncateTime(DateTime.Now));

            if (onlyMyCommissions)
            {
                int currentPersonId = userService.GetCurrentUser().PersonId;
                query = query.Where(x => x.CommissionDecisions.Any(a => a.CommissionMember.PersonStaff.PersonId == currentPersonId));
            }
            return new DisposableQueryable<CommissionProtocol>(query, context);
        }

        public string GetDecisionColorHex(int? decisionId)
        {
            using (var context = contextProvider.CreateNewContext())
            {
                string defColor = HexConverter(System.Drawing.Color.White);
                if (!decisionId.HasValue) return defColor;
                var decision = context.Set<Decision>().FirstOrDefault(x => x.Id == decisionId.Value);
                //if (decision != null)
                //    return decision.ColorSettings.HexColor;
                return defColor;
            }
        }

        private static String HexConverter(System.Drawing.Color c)
        {
            return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
        }

        public IDisposableQueryable<CommissionDecision> GetCommissionDecisions(int commissionProtocolId)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<CommissionDecision>(context.Set<CommissionDecision>().Where(x => x.CommissionProtocolId == commissionProtocolId), context);
        }


        public IDisposableQueryable<CommissionDecision> GetCommissionDecision(int commissionDecisionId)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<CommissionDecision>(context.Set<CommissionDecision>().Where(x => x.Id == commissionDecisionId), context);
        }
    }
}
