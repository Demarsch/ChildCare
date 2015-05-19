using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using DataLib;
using System.Data.Entity.Core.Objects;

namespace Core
{
    public class CommissionService : ICommissionService
    {
        private IDataContextProvider provider;

        public CommissionService(IDataContextProvider Provider)
        {
            provider = Provider;
        }

        public int Save(CommissionDecision commissionDecision, out string msg)
        {
            string exception = string.Empty;
            try
            {
                using (var db = provider.GetNewDataContext())
                {
                    var decision = commissionDecision.Id > 0 ? db.GetById<CommissionDecision>(commissionDecision.Id) : new CommissionDecision();
                    decision.CommissionProtocolId = commissionDecision.CommissionProtocolId;
                    decision.CommissionMemberId = commissionDecision.CommissionMemberId;
                    decision.IsOfficial = commissionDecision.IsOfficial;
                    decision.DecisionId = commissionDecision.DecisionId;
                    decision.DecisionInDateTime = commissionDecision.DecisionInDateTime;
                    decision.Comment = commissionDecision.Comment;
                    decision.CommissionStage = commissionDecision.CommissionStage;
                    decision.InitiatorMemberId = commissionDecision.InitiatorMemberId;
                    decision.InDateTime = commissionDecision.InDateTime;
                    if (decision.Id == 0)
                        db.Add<CommissionDecision>(decision);
                    db.Save();
                    msg = exception;
                    return decision.Id;
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return 0;
            }
        }

        public int Save(CommissionMember commissionMember, out string msg)
        {
            string exception = string.Empty;
            try
            {
                using (var db = provider.GetNewDataContext())
                {
                    var member = commissionMember.Id > 0 ? db.GetById<CommissionMember>(commissionMember.Id) : new CommissionMember();
                    member.PersonStaffId = commissionMember.PersonStaffId;
                    member.CommissionMemberTypeId = commissionMember.CommissionMemberTypeId;
                    member.CommissionTypeId = commissionMember.CommissionTypeId;
                    member.BeginDateTime = commissionMember.BeginDateTime;
                    member.EndDateTime = commissionMember.EndDateTime;
                    if (member.Id == 0)
                        db.Add<CommissionMember>(member);
                    db.Save();
                    msg = exception;
                    return member.Id;
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return 0;
            }
        }

        public ICollection<CommissionProtocol> GetCommissionsByMemberPersonId(int personId, bool isCompleted)
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<CommissionDecision>().Where(x => x.CommissionMember.PersonStaff.PersonId == personId && (isCompleted ? x.CommissionProtocol.IsCompleted.Value : (!x.CommissionProtocol.IsCompleted.HasValue || x.CommissionProtocol.IsCompleted == false))).Select(x => x.CommissionProtocol).Distinct().ToArray();
            }
        }

        public ICollection<Decision> GetActualMainDecisions(DateTime begin, DateTime end)
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<Decision>().Where(x => !x.ParentId.HasValue && EntityFunctions.TruncateTime(x.BeginDateTime) <= EntityFunctions.TruncateTime(end) && EntityFunctions.TruncateTime(x.EndDateTime) >= EntityFunctions.TruncateTime(begin)).ToArray();
            }
        }

        public ICollection<Decision> GetActualSpecificDecisions(int mainDecisionId, DateTime begin, DateTime end)
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<Decision>().Where(x => x.ParentId == mainDecisionId && EntityFunctions.TruncateTime(x.BeginDateTime) <= EntityFunctions.TruncateTime(end) && EntityFunctions.TruncateTime(x.EndDateTime) >= EntityFunctions.TruncateTime(begin)).ToArray();
            }
        }

        public Decision GetDecisionById(int decisionId)
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<Decision>().FirstOrDefault(x => x.Id == decisionId);
            }
        }

        public ICollection<CommissionDecision> GetCommissionDecisionsByProtocolId(int commissionProtocolId)
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<CommissionDecision>().Where(x => x.CommissionProtocolId == commissionProtocolId).OrderBy(x => x.CommissionStage).ToArray();
            }
        }

        public CommissionDecision GetCommissionDecisionById(int commissionDecisionId)
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<CommissionDecision>().FirstOrDefault(x => x.Id == commissionDecisionId);
            }
        }

        public CommissionDecision GetLastCommissionDecisionByMemberPersonId(int commissionProtocolId, int personId)
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<CommissionDecision>().Where(x => x.CommissionProtocolId == commissionProtocolId && x.CommissionMember.PersonStaff.PersonId == personId).OrderByDescending(x => x.CommissionStage).FirstOrDefault();
            }
        }

        public Staff GetCommissionMemberStaffById(int commissionMemberId)
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<CommissionMember>().First(x => x.Id == commissionMemberId).PersonStaff.Staff;
            }
        }

        public Person GetCommissionMemberPersonById(int commissionMemberId)
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<CommissionMember>().First(x => x.Id == commissionMemberId).PersonStaff.Person;
            }
        }

        public string GetDecisionNameById(int decisionId)
        {
            using (var db = provider.GetNewDataContext())
            {
                var decision = db.GetData<Decision>().FirstOrDefault(x => x.Id == decisionId);
                return decision.ShortName + (decision.ShortName.ToLower().Trim() != decision.Decision1.ShortName.ToLower().Trim() ? " (" + decision.Decision1.ShortName + ")" : string.Empty);
            }
        }
     
        public ICollection<CommissionProtocol> GetCommissionsByFilter(CommissionServiceFilter filter)
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<CommissionProtocol>().ToArray();
            }
        }

        public CommissionProtocol GetCommissionProtocolById(int protocolId)
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<CommissionProtocol>().FirstOrDefault(x => x.Id == protocolId);
            }
        }

        public CommissionType GetCommissionTypeById(int commissionTypeId)
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<CommissionType>().FirstOrDefault(x => x.Id == commissionTypeId);
            }
        }

        public CommissionMember GetCommissionMemberById(int memberId)
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<CommissionMember>().FirstOrDefault(x => x.Id == memberId);
            }
        }
    }      
}
