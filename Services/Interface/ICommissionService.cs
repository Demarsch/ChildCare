using System;
using System.Collections.Generic;
using DataLib;

namespace Core
{
    public interface ICommissionService
    {
        CommissionProtocol GetCommissionProtocolById(int protocolId);
        ICollection<CommissionProtocol> GetCommissionsByMemberPersonId(int personId, bool isCompleted);
        ICollection<CommissionProtocol> GetCommissionsByFilter(CommissionServiceFilter filter);
       
        ICollection<Decision> GetActualMainDecisions(DateTime begin, DateTime end);
        ICollection<Decision> GetActualSpecificDecisions(int mainDecisionId, DateTime begin, DateTime end);
        Decision GetDecisionById(int decisionId);

        ICollection<CommissionDecision> GetCommissionDecisionsByProtocolId(int commissionProtocolId);
        CommissionDecision GetCommissionDecisionById(int commissionDecisionId);
        CommissionDecision GetLastCommissionDecisionByMemberPersonId(int commissionProtocolId, int personId);

        CommissionType GetCommissionTypeById(int commissionTypeId);

        CommissionMember GetCommissionMemberById(int memberId);

        Staff GetCommissionMemberStaffById(int commissionMemberId);
        Person GetCommissionMemberPersonById(int commissionMemberId);
        string GetDecisionNameById(int commissionDecisionId);
        
        int Save(CommissionDecision commissionDecision, out string msg);
        int Save(CommissionMember commissionMember, out string msg);
    }

    public class CommissionServiceFilter
    {
        public bool IsActive;
    }
}
