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
using Core.Misc;

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
                query = query.Where(x => DbFunctions.TruncateTime(x.ProtocolDate) == DbFunctions.TruncateTime(date.Value));
            if (option.Contains(OptionValues.ProtocolsAdded))
                query = query.Where(x => DbFunctions.TruncateTime(x.IncomeDateTime) == DbFunctions.TruncateTime(date.Value));
            if (option.Contains(OptionValues.ProtocolsAwaiting))
                query = query.Where(x => x.IsCompleted == true && DbFunctions.TruncateTime(x.ToDoDateTime) > DbFunctions.TruncateTime(DateTime.Now));

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

        public IEnumerable<Decision> GetDecisions(object commissionQuestionIdAndCommissionTypeMemberIdAndOnDate)
        {
            var tuple = commissionQuestionIdAndCommissionTypeMemberIdAndOnDate as Tuple<int, int, DateTime>;
            if (tuple == null) return null;
            var context = contextProvider.CreateNewContext();
            int commissionQuestionId = tuple.Item1;
            int commissionTypeMemberId = tuple.Item2;
            DateTime onDate = tuple.Item3;
            return context.Set<Decision>().Where(x => x.ParentId == null && onDate >= x.BeginDateTime && onDate < x.EndDateTime &&
                x.CommissionDecisionsLinks.Any(y => (commissionTypeMemberId == SpecialValues.NonExistingId || y.CommissionTypeMemberId == commissionTypeMemberId) &&
                    (commissionQuestionId == SpecialValues.NonExistingId || y.CommissionQuestionId == commissionQuestionId) && onDate >= y.BeginDateTime && onDate < y.EndDateTime)).ToArray()
                .Select(x => CopyDecision(x, onDate))
                .Where(x => x != null)
                .ToArray();
        }

        private Decision CopyDecision(Decision decision, DateTime onDate)
        {
            var result = new Decision { Id = decision.Id, Name = decision.Name };
            var children = decision.Decisions1.Where(x => onDate >= x.BeginDateTime && onDate < x.EndDateTime).Select(x => CopyDecision(x, onDate)).Where(x => x != null).ToArray();
            result.Decisions1 = children.Length == 0 ? null : children;
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

        public async Task SaveCommissionProtocolAsync(CommissionProtocol newProtocol, CancellationToken token, INotificationServiceSubscription<CommissionProtocol> protocolChangeSubscription)
        {
            using (var db = contextProvider.CreateNewContext())
            {
                var curUserId = userService.GetCurrentUserId();
                var originalProtocol = await db.NoTrackingSet<CommissionProtocol>().FirstOrDefaultAsync(x => x.Id == newProtocol.Id);
                var curCommissionProtocol = (CommissionProtocol)originalProtocol.Clone();
                if (curCommissionProtocol == null)
                {
                    curCommissionProtocol = new CommissionProtocol
                    {
                        PersonId = newProtocol.PersonId,
                        ProtocolNumber = 0,
                        ProtocolDate = DateTime.Now,
                        IsCompleted = newProtocol.IsCompleted,
                        InUserId = curUserId
                    };
                    db.Set<CommissionProtocol>().Add(curCommissionProtocol);
                }
                curCommissionProtocol.CommissionTypeId = newProtocol.CommissionTypeId;
                curCommissionProtocol.CommissionQuestionId = newProtocol.CommissionQuestionId;
                curCommissionProtocol.CommissionSourceId = newProtocol.CommissionSourceId;
                curCommissionProtocol.SentLPUId = newProtocol.SentLPUId;
                curCommissionProtocol.PersonTalonId = newProtocol.PersonTalonId;
                curCommissionProtocol.MedicalHelpTypeId = newProtocol.MedicalHelpTypeId;
                curCommissionProtocol.MKB = newProtocol.MKB;
                curCommissionProtocol.IncomeDateTime = newProtocol.IncomeDateTime;
                curCommissionProtocol.PersonAddressId = newProtocol.PersonAddressId;

                curCommissionProtocol.ProtocolNumber = newProtocol.ProtocolNumber;
                curCommissionProtocol.ProtocolDate = newProtocol.ProtocolDate;
                curCommissionProtocol.WaitingFor = newProtocol.WaitingFor;
                curCommissionProtocol.Diagnos = newProtocol.Diagnos;
                curCommissionProtocol.DecisionId = newProtocol.DecisionId;
                curCommissionProtocol.Comment = newProtocol.Comment;
                curCommissionProtocol.ToDoDateTime = newProtocol.ToDoDateTime;

                curCommissionProtocol.IsExecuting = newProtocol.IsExecuting;
                if (curCommissionProtocol.BeginDateTime == null && curCommissionProtocol.IsExecuting)
                    curCommissionProtocol.BeginDateTime = DateTime.Now;

                var newDecisionIds = newProtocol.CommissionDecisions.Select(x => x.Id).ToArray();

                foreach (var curDecision in curCommissionProtocol.CommissionDecisions.Where(x => x.RemovedByUserId == null))
                {
                    if (!newDecisionIds.Contains(curDecision.Id))
                        curDecision.RemovedByUserId = curUserId;
                    else
                    {
                        var changedDecision = newProtocol.CommissionDecisions.FirstOrDefault(x => x.Id == curDecision.Id);
                        curDecision.CommissionStage = changedDecision.CommissionStage;
                        curDecision.NeedAllMembersInStage = changedDecision.NeedAllMembersInStage;
                    }
                }
                var existDecisionIds = curCommissionProtocol.CommissionDecisions.Select(x => x.Id).ToArray();
                CommissionDecision decision;
                foreach (var newDecision in newProtocol.CommissionDecisions.Where(x => !existDecisionIds.Contains(x.Id)))
                {
                    decision = new CommissionDecision()
                    {
                        CommissionProtocol = curCommissionProtocol,
                        CommissionStage = newDecision.CommissionStage,
                        NeedAllMembersInStage = newDecision.NeedAllMembersInStage,
                        CommissionMemberId = newDecision.CommissionMemberId,
                        InitiatorUserId = curUserId,
                    };
                    db.Entry(decision).State = EntityState.Modified;
                    //commissionProtocol.CommissionDecisions.Add(decision);
                }
                db.Entry(curCommissionProtocol).State = EntityState.Modified;
                await db.SaveChangesAsync(token);
                if (protocolChangeSubscription != null)
                {
                    protocolChangeSubscription.Notify(originalProtocol, curCommissionProtocol);
                }
            }
        }

        public async Task UpdateCommissionProtocolAsync(int protocolId, DateTime protocolDate, CancellationToken token,
                                                INotificationServiceSubscription<CommissionProtocol> protocolChangeSubscription)
        {
            using (var dataContext = contextProvider.CreateLightweightContext())
            {
                var originalProtocol = await dataContext.NoTrackingSet<CommissionProtocol>().FirstAsync(x => x.Id == protocolId);
                var newProtocol = (CommissionProtocol)originalProtocol.Clone();
                newProtocol.ProtocolDate = protocolDate;

                dataContext.Entry(newProtocol).State = EntityState.Modified;
                await dataContext.SaveChangesAsync(token);
                if (protocolChangeSubscription != null)
                {
                    protocolChangeSubscription.Notify(originalProtocol, newProtocol);
                }
            }
        }

        public IEnumerable<CommissionType> GetCommissionTypes(object onDate)
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
            return new DisposableQueryable<PersonTalon>(context.Set<PersonTalon>().Where(x => x.PersonId == personId && !x.RemovedByUserId.HasValue), context);
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
            return new DisposableQueryable<CommissionProtocol>(context.Set<CommissionProtocol>().Where(x => x.PersonId == personId && !x.RemovedByUserId.HasValue), context);
        }


        public IDisposableQueryable<PersonTalon> GetTalonById(int id)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<PersonTalon>(context.Set<PersonTalon>().Where(x => x.Id == id), context);
        }

        public IDisposableQueryable<RecordContract> GetRecordContractsByOptions(string options, DateTime onDate)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<RecordContract>(context.Set<RecordContract>()
                                                                  .Where(x => x.Options.Contains(options) && x.BeginDateTime <= onDate && x.EndDateTime > onDate)
                                                                  .Take(AppConfiguration.SearchResultTakeTopCount), context);
        }

        public IDisposableQueryable<AddressType> GetAddressTypeByCategory(string category)
        {
            var context = contextProvider.CreateNewContext();
            var filter = category.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            var query = context.Set<AddressType>().ToList().Where(x => filter.Any(y => x.Category.IndexOf(y, StringComparison.CurrentCultureIgnoreCase) != -1)).AsQueryable();
            return new DisposableQueryable<AddressType>(query.Take(AppConfiguration.SearchResultTakeTopCount), context);
        }

        public async Task<int> SaveTalon(PersonTalon talon, CancellationToken token)
        {
            if (token.IsCancellationRequested)
                throw new OperationCanceledException(token);
            using (var context = contextProvider.CreateNewContext())
            {
                var savedTalon = talon.Id == SpecialValues.NewId ? new PersonTalon() : context.Set<PersonTalon>().First(x => x.Id == talon.Id);
                savedTalon.PersonId = talon.PersonId;
                savedTalon.TalonNumber = talon.TalonNumber.ToSafeString();
                savedTalon.TalonDateTime = talon.TalonDateTime;
                savedTalon.MKB = talon.MKB.ToSafeString();
                savedTalon.Comment = talon.Comment.ToSafeString();
                savedTalon.RecordContractId = talon.RecordContractId;
                savedTalon.MedicalHelpTypeId = talon.MedicalHelpTypeId;
                savedTalon.IsCompleted = talon.IsCompleted;
                savedTalon.PersonAddressId = talon.PersonAddressId;
                context.Entry<PersonTalon>(savedTalon).State = savedTalon.Id == SpecialValues.NewId ? EntityState.Added : EntityState.Modified;

                if (token.IsCancellationRequested)
                    throw new OperationCanceledException(token);
                await context.SaveChangesAsync(token);
                return savedTalon.Id;
            }
        }

        public async Task<bool> RemoveTalon(int talonId)
        {
            using (var context = contextProvider.CreateNewContext())
            {
                var talon = context.Set<PersonTalon>().First(x => x.Id == talonId);
                talon.RemovedByUserId = userService.GetCurrentUser().Id;
                context.Entry(talon).State = EntityState.Modified;
                try
                {
                    await context.SaveChangesAsync();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public async Task<int> SaveTalonAddress(PersonAddress talonAddress, CancellationToken token)
        {
            if (token.IsCancellationRequested)
                throw new OperationCanceledException(token);
            using (var context = contextProvider.CreateNewContext())
            {
                var savedAddress = talonAddress.Id == SpecialValues.NewId ? new PersonAddress() : context.Set<PersonAddress>().First(x => x.Id == talonAddress.Id);
                savedAddress.PersonId = talonAddress.PersonId;
                savedAddress.AddressTypeId = talonAddress.AddressTypeId;
                savedAddress.OkatoId = talonAddress.OkatoId;
                savedAddress.UserText = talonAddress.UserText.ToSafeString();
                savedAddress.House = talonAddress.House.ToSafeString();
                savedAddress.Building = talonAddress.Building.ToSafeString();
                savedAddress.Apartment = talonAddress.Apartment.ToSafeString();
                savedAddress.BeginDateTime = talonAddress.BeginDateTime;
                savedAddress.EndDateTime = talonAddress.EndDateTime;
                context.Entry<PersonAddress>(savedAddress).State = savedAddress.Id == SpecialValues.NewId ? EntityState.Added : EntityState.Modified;

                if (token.IsCancellationRequested)
                    throw new OperationCanceledException(token);
                await context.SaveChangesAsync(token);
                return savedAddress.Id;
            }
        }

        public async Task<string> RemoveCommissionProtocol(int protocolId, CancellationToken token, INotificationServiceSubscription<CommissionProtocol> protocolChangeSubscription)
        {
            using (var context = contextProvider.CreateLightweightContext())
            {
                try
                {
                    var originalProtocol = context.NoTrackingSet<CommissionProtocol>().First(x => x.Id == protocolId);
                    var newProtocol = (CommissionProtocol)originalProtocol.Clone();
                    newProtocol.RemovedByUserId = userService.GetCurrentUser().Id;
                    context.Entry(newProtocol).State = EntityState.Modified;
                    await context.SaveChangesAsync(token);
                    if (protocolChangeSubscription != null)
                    {
                        protocolChangeSubscription.Notify(originalProtocol, newProtocol);
                    }
                    return string.Empty;
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }
        }

        public async Task<string> ChangeIsExecutingCommissionProtocol(int protocolId, bool isExecuting, CancellationToken token, INotificationServiceSubscription<CommissionProtocol> protocolChangeSubscription)
        {
            using (var context = contextProvider.CreateLightweightContext())
            {
                try
                {
                    var originalProtocol = context.NoTrackingSet<CommissionProtocol>().First(x => x.Id == protocolId);
                    var newProtocol = (CommissionProtocol)originalProtocol.Clone();
                    newProtocol.IsExecuting = isExecuting;
                    context.Entry(newProtocol).State = EntityState.Modified;
                    await context.SaveChangesAsync(token);
                    if (protocolChangeSubscription != null)
                    {
                        protocolChangeSubscription.Notify(originalProtocol, newProtocol);
                    }
                    return string.Empty;
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }
        }

        public IDisposableQueryable<CommissionMember> GetCommissionMembers(int commissionTypeId, DateTime onDate)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<CommissionMember>(context.Set<CommissionMember>().Where(x => x.CommissionTypeId == commissionTypeId && onDate >= x.BeginDateTime && onDate < x.EndDateTime), context);
        }

        public IDisposableQueryable<CommissionMember> GetCommissionMember(int commissionMemberId)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<CommissionMember>(context.Set<CommissionMember>().Where(x => x.Id == commissionMemberId), context);
        }

        public IDisposableQueryable<CommissionMemberType> GetCommissionMemberTypes()
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<CommissionMemberType>(context.Set<CommissionMemberType>(), context);
        }

        public IDisposableQueryable<PersonStaff> GetPersonStaffs(object onDate)
        {
            DateTime dt = SpecialValues.MinDate;
            DateTime.TryParse(onDate.ToSafeString(), out dt);
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<PersonStaff>(context.Set<PersonStaff>().Where(x => dt >= x.BeginDateTime && dt < x.EndDateTime), context);
        }

        public IDisposableQueryable<Staff> GetStaffs()
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<Staff>(context.Set<Staff>(), context);
        }


        public async Task SaveCommissionMembersAsync(CommissionMember[] commissionMembers, DateTime onDate)
        {
            using (var context = contextProvider.CreateNewContext())
            {
                if (!commissionMembers.Any()) return;
                int commissionTypeId = commissionMembers.First().CommissionTypeId;
                var dbMembers = context.Set<CommissionMember>().Where(x => x.CommissionTypeId == commissionTypeId && x.BeginDateTime <= onDate.Date && x.EndDateTime > onDate.Date);

                var old = dbMembers.ToDictionary(x => x.Id);
                var @new = commissionMembers.Where(x => x.Id != SpecialValues.NewId).ToDictionary(x => x.Id);
                var added = commissionMembers.Where(x => x.Id == SpecialValues.NewId).ToArray();
                var removed = old.Where(x => !@new.ContainsKey(x.Key))
                             .Select(removedDocument => removedDocument.Value)
                             .ToArray();
                var existed = @new.Where(x => old.ContainsKey(x.Key))
                                  .Select(x => new { Old = old[x.Key], New = x.Value, IsChanged = !x.Value.Equals(old[x.Key]) })
                                  .ToArray();
                foreach (var member in added)
                {
                    context.Entry(member).State = EntityState.Added;
                }
                foreach (var member in removed)
                {
                    if (!member.CommissionDecisions.Any()/* && !member.CommissionDecisions1.Any()*/) //decisionMaker and initiator
                        context.Entry(member).State = EntityState.Deleted;
                }
                foreach (var member in existed.Where(x => x.IsChanged))
                {
                    member.Old.CommissionTypeId = member.New.CommissionTypeId;
                    member.Old.CommissionMemberTypeId = member.New.CommissionMemberTypeId;
                    member.Old.PersonStaffId = member.New.PersonStaffId;
                    member.Old.StaffId = member.New.StaffId;
                    member.Old.BeginDateTime = member.New.BeginDateTime.Date;
                    member.Old.EndDateTime = member.New.EndDateTime.Date;
                    context.Entry(member.Old).State = EntityState.Modified;
                }
                await context.SaveChangesAsync();
            }
        }

        public IDisposableQueryable<CommissionMember> CommissionMemberById(int id)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<CommissionMember>(context.Set<CommissionMember>().Where(x => x.Id == id), context);
        }

    }
}
