using Core.Data;
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
using Core.Notification;
using System.Data.Entity;
using System.Threading;

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

        public IDisposableQueryable<CommissionProtocol> GetCommissionProtocolById(int protocolId)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<CommissionProtocol>(context.Set<CommissionProtocol>().Where(x => x.Id == protocolId), context);
        }

        public async Task<string> SaveDecision(int commissionDecisionId, int decisionId, string comment, DateTime? decisionDateTime, CancellationToken token)
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

        public async Task SaveCommissionProtocol(CommissionProtocol commissionProtocol, CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                throw new OperationCanceledException(token);
            }
            using (var context = contextProvider.CreateNewContext())
            {
                if (SpecialValues.NewId == commissionProtocol.Id)
                    context.Entry(commissionProtocol).State = EntityState.Added;
                else
                    context.Entry(commissionProtocol).State = EntityState.Modified;
                if (token.IsCancellationRequested)
                {
                    throw new OperationCanceledException(token);
                }
                await context.SaveChangesAsync(token);
            }
        }

        public CommissionFilter GetCommissionFilterById(int id)
        {
            using (var db = contextProvider.CreateNewContext())
            {
                return db.Set<CommissionFilter>().FirstOrDefault(x => x.Id == id);
            }
        }

        public async Task SaveCommissionProtocolAsync(CommissionProtocol newProtocol, INotificationServiceSubscription<CommissionProtocol> protocolChangeSubscription)
        {
            CommissionProtocol originalProtocol = null;
            using (var dataContext = contextProvider.CreateLightweightContext())
            {
                dataContext.Configuration.ProxyCreationEnabled = false;
                if (newProtocol.Id == SpecialValues.NewId)
                {
                    dataContext.Entry(newProtocol).State = EntityState.Added;
                }
                else
                {
                    originalProtocol = await dataContext.NoTrackingSet<CommissionProtocol>().FirstAsync(x => x.Id == newProtocol.Id);
                    dataContext.Entry(newProtocol).State = EntityState.Modified;
                }
                await dataContext.SaveChangesAsync();
                if (protocolChangeSubscription != null)
                {
                    protocolChangeSubscription.Notify(originalProtocol, newProtocol);
                }
            }
        }

        public async Task DeleteCommissionProtocolAsync(int protocolId, INotificationServiceSubscription<CommissionProtocol> protocolChangeSubscription)
        {
            using (var dataContext = contextProvider.CreateLightweightContext())
            {
                dataContext.Configuration.ProxyCreationEnabled = false;
                var originalProtocol = await dataContext.NoTrackingSet<CommissionProtocol>().FirstAsync(x => x.Id == protocolId);
                dataContext.Entry(originalProtocol).State = EntityState.Deleted;
                await dataContext.SaveChangesAsync();
                if (protocolChangeSubscription != null)
                {
                    protocolChangeSubscription.NotifyDelete(originalProtocol);
                }
            }
        }

        public async Task UpdateCommissionProtocolAsync(int protocolId, DateTime protocolDate,
                                                INotificationServiceSubscription<CommissionProtocol> protocolChangeSubscription)
        {
            using (var dataContext = contextProvider.CreateLightweightContext())
            {
                var originalProtocol = await dataContext.NoTrackingSet<CommissionProtocol>().FirstAsync(x => x.Id == protocolId);
                var newProtocol = (CommissionProtocol)originalProtocol.Clone();
                newProtocol.ProtocolDate = protocolDate;

                dataContext.Entry(newProtocol).State = EntityState.Modified;
                await dataContext.SaveChangesAsync();
                if (protocolChangeSubscription != null)
                {
                    protocolChangeSubscription.Notify(originalProtocol, newProtocol);
                }
            }
        }


        public IEnumerable<CommissionType> GetCommissionType(object onDate)
        {
            DateTime dt = SpecialValues.MinDate;
            DateTime.TryParse(onDate.ToSafeString(), out dt);
            return cacheService.GetItems<CommissionType>().Where(x => dt >= x.BeginDateTime && dt < x.EndDateTime);
        }

        public IEnumerable<CommissionSource> GetCommissionSource(object onDate)
        {
            DateTime dt = SpecialValues.MinDate;
            DateTime.TryParse(onDate.ToSafeString(), out dt);
            return cacheService.GetItems<CommissionSource>().Where(x => dt >= x.BeginDateTime && dt < x.EndDateTime);
        }

        public IDisposableQueryable<Org> GetCommissionSentLPUs(DateTime onDate)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<Org>(context.Set<Org>().Where(x => onDate >= x.BeginDateTime && onDate < x.EndDateTime && x.IsLpu), context);
        }

        public IDisposableQueryable<PersonTalon> GetPatientTalons(int personId)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<PersonTalon>(context.Set<PersonTalon>().Where(x => x.PersonId == personId), context);
        }

        public IDisposableQueryable<Person> GetPerson(int personId)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<Person>(context.Set<Person>().Where(x => x.Id == personId), context);
        }

        public IEnumerable<MedicalHelpType> GetCommissionMedicalHelpTypes(object onDate)
        {
            DateTime dt = SpecialValues.MinDate;
            DateTime.TryParse(onDate.ToSafeString(), out dt);
            return cacheService.GetItems<MedicalHelpType>().Where(x => dt >= x.BeginDateTime && dt < x.EndDateTime);
        }

        public IEnumerable<CommissionQuestion> GetCommissionQuestions(object onDate)
        {
            DateTime dt = SpecialValues.MinDate;
            DateTime.TryParse(onDate.ToSafeString(), out dt);
            return cacheService.GetItems<CommissionQuestion>().Where(x => dt >= x.BeginDateTime && dt < x.EndDateTime);
        }


        public IDisposableQueryable<PersonAddress> GetPatientAddresses(int personId)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<PersonAddress>(context.Set<PersonAddress>().Where(x => x.PersonId == personId), context);
        }

        public IDisposableQueryable<CommissionProtocol> GetPersonCommissionProtocols(int personId)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<CommissionProtocol>(context.Set<CommissionProtocol>().Where(x => x.PersonId == personId), context);
        }


        public IDisposableQueryable<PersonTalon> GetTalonById(int id)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<PersonTalon>(context.Set<PersonTalon>().Where(x => x.Id == id), context);
        }
    }
}
