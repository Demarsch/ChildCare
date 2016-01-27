﻿using Core.Data;
using Core.Data.Misc;
using Core.Data.Services;
using Core.Services;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Core.Extensions;

namespace CommissionsModule.Services
{
    public class CommissionService : ICommissionService
    {
        private readonly IDbContextProvider contextProvider;
        private readonly IUserService userService;
        private readonly ICacheService cacheService;

        public CommissionService(IDbContextProvider contextProvider, IUserService userService, ICacheService cacheService)
        {
            if (contextProvider == null)
            {
                throw new ArgumentNullException("contextProvider");
            }
            if (userService == null)
            {
                throw new ArgumentNullException("userService");
            }
            if (cacheService == null)
            {
                throw new ArgumentNullException("cacheService");
            }
            this.cacheService = cacheService;
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
                return (decision != null && decision.ColorsSetting != null && !string.IsNullOrEmpty(decision.ColorsSetting.Hex)) ? decision.ColorsSetting.Hex : defColor;
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

        public IEnumerable<Decision> GetDecisions(object commissionQuestionIdAndCommissionTypeMemberId)
        {
            var context = contextProvider.CreateNewContext();
            int commissionQuestionId = (commissionQuestionIdAndCommissionTypeMemberId as int[])[0].ToInt();
            int commissionTypeMemberId = (commissionQuestionIdAndCommissionTypeMemberId as int[])[1].ToInt();
            return context.Set<Decision>().Where(x => x.ParentId == null && x.CommissionDecisionsLinks.Any(y => y.CommissionTypeMemberId == commissionTypeMemberId && y.CommissionQuestionId == commissionQuestionId))
                .Select(CopyDecision)
                .Where(x => x != null)
                .ToArray();
        }

        private Decision CopyDecision(Decision decision)
        {
            var result = new Decision { Id = decision.Id, Name = decision.Name };
            var children = decision.Decisions1.Select(CopyDecision).Where(x => x != null).ToList();
            result.Decisions1 = children.Count == 0 ? null : children;
            foreach (var childRecortType in children)
            {
                childRecortType.Decision1 = result;
            }
            return result;
        }


        public async Task<string> SaveDecision(int commissionDecisionId, int decisionId, string comment, DateTime? decisionDateTime, System.Threading.CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                throw new OperationCanceledException(token);
            }
            using (var context = contextProvider.CreateNewContext())
            {
                var commissionDecision = context.Set<CommissionDecision>().FirstOrDefault(x => x.Id == commissionDecisionId);
                if (commissionDecision == null)
                    return "Не найдено решение для комиссии";
                commissionDecision.DecisionId = decisionId;
                commissionDecision.Comment = comment;
                commissionDecision.DecisionDateTime = decisionDateTime;
                if (token.IsCancellationRequested)
                {
                    throw new OperationCanceledException(token);
                }
                await context.SaveChangesAsync(token);
            }
            return string.Empty;
        }
    }
}
