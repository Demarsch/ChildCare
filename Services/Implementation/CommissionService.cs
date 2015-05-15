using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using DataLib;

namespace Core
{
    public class CommissionService : ICommissionService
    {
        private IDataContextProvider provider;

        public CommissionService(IDataContextProvider Provider)
        {
            provider = Provider;
        }

        public bool Save(CommissionDecision commissionDecision, out string msg)
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
                    return true;
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return false;
            }
        }

        public ICollection<CommissionProtocol> GetCommissionsByUserId(int userId)
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<CommissionProtocol>().Where(x => x.InUserId == userId).ToArray();
            }
        }

        public ICollection<Decision> GetActualMainDecisions()
        {
            using (var db = provider.GetNewDataContext())
            {
                var DateTimeNow = DateTime.Now;
                return db.GetData<Decision>().Where(x => !x.ParentId.HasValue && DateTimeNow >= x.BeginDateTime && DateTimeNow < x.EndDateTime).ToArray();
            }
        }

        public ICollection<Decision> GetActualSpecificDecisions(int mainDecisionId)
        {
            using (var db = provider.GetNewDataContext())
            {
                var DateTimeNow = DateTime.Now;
                return db.GetData<Decision>().Where(x => x.ParentId == mainDecisionId && DateTimeNow >= x.BeginDateTime && DateTimeNow < x.EndDateTime).ToArray();
            }
        }

        public ICollection<CommissionDecision> GetCommissionDecisionsByProtocolId(int commissionProtocolId)
        {
            using (var db = provider.GetNewDataContext())
            {
                var DateTimeNow = DateTime.Now;
                return db.GetData<CommissionDecision>().Where(x => x.CommissionProtocolId == commissionProtocolId).ToArray();
            }
        }

        public CommissionDecision GetCommissionDecisionById(int commissionDecisionId)
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<CommissionDecision>().First(x => x.Id == commissionDecisionId);
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

        public Decision GetDecisionById(int decisionId)
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<Decision>().First(x => x.Id == decisionId);
            }
        }


        public ICollection<CommissionProtocol> GetCommissionsByFilter(CommissionServiceFilter filter)
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<CommissionProtocol>().ToArray();
            }
        }
    }      
}
