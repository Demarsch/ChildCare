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
using System.Data.Entity;
using System.Threading;
using Core.Misc;
using Core.Notification;

namespace Shared.Commissions.Services
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
               
        public IEnumerable<CommissionTypeGroup> GetCommissionTypeGroups(object onDate)
        {
            DateTime dt = SpecialValues.MinDate;
            DateTime.TryParse(onDate.ToSafeString(), out dt);
            return cacheService.GetItems<CommissionTypeGroup>().Where(x => dt >= x.BeginDateTime && dt < x.EndDateTime);
        }

        public IEnumerable<CommissionType> GetCommissionTypes(object onDate, int commissionTypeGroupId = -1)
        {
            DateTime dt = SpecialValues.MinDate;
            DateTime.TryParse(onDate.ToSafeString(), out dt);
            return cacheService.GetItems<CommissionType>().Where(x => dt >= x.BeginDateTime && dt < x.EndDateTime && x.CommissionTypeGroupId == commissionTypeGroupId);
        }

        public IEnumerable<CommissionQuestion> GetCommissionQuestions(object onDate, int commissionTypeId = -1)
        {
            DateTime dt = SpecialValues.MinDate;
            DateTime.TryParse(onDate.ToSafeString(), out dt);
            return cacheService.GetItems<CommissionQuestion>().Where(x => dt >= x.BeginDateTime && dt < x.EndDateTime && x.CommissionTypeId == commissionTypeId);
        }  

        public IDisposableQueryable<Person> GetPerson(int personId)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<Person>(context.Set<Person>().Where(x => x.Id == personId), context);
        }

        
        public async Task<int> CreateCommissionAssignment(int personId, DateTime commissionDate, int commissionTypeId, int commissionQuestionId, string mkb, CancellationToken token)
        {
            using (var db = contextProvider.CreateNewContext())
            {
                var curUserId = userService.GetCurrentUserId();

                var protocol = new CommissionProtocol();
                protocol.PersonId = personId;
                protocol.CommissionNumber = 0;                
                protocol.CommissionDate = commissionDate;
                protocol.IncomeDateTime = commissionDate;
                protocol.ProtocolNumber = 0;
                protocol.CommissionTypeId = commissionTypeId;
                protocol.CommissionQuestionId = commissionQuestionId;
                protocol.MKB = mkb;
                protocol.InUserId = curUserId;
                protocol.IsSended = true;

                protocol.IsCompleted = null;
                protocol.IsExecuting = false;

                protocol.CommissionSourceId = null;
                protocol.SentLPUId = null;
                protocol.PersonTalonId = null;
                protocol.MedicalHelpTypeId = null;
                protocol.PersonAddressId = null;
                
                protocol.WaitingFor = string.Empty;
                protocol.Diagnos = string.Empty;
                protocol.DecisionId = null;
                protocol.Comment = string.Empty;
                
                protocol.ToDoDateTime = null;                
                protocol.BeginDateTime = null;
                
                db.Entry(protocol).State = EntityState.Added;
                await db.SaveChangesAsync(token);
                return protocol.Id;
            }
        }
    }
}
