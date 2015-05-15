using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataLib;

namespace Core
{
    public interface ICommissionService
    {
        ICollection<CommissionProtocol> GetCommissionsByUserId(int userId);
        ICollection<CommissionProtocol> GetCommissionsByFilter(CommissionServiceFilter filter);
        ICollection<Decision> GetActualMainDecisions();
        ICollection<Decision> GetActualSpecificDecisions(int mainDecisionId);

        ICollection<CommissionDecision> GetCommissionDecisionsByProtocolId(int commissionProtocolId);
        CommissionDecision GetCommissionDecisionById(int commissionDecisionId);
        Staff GetCommissionMemberStaffById(int commissionMemberId);
        Person GetCommissionMemberPersonById(int commissionMemberId);
        string GetDecisionNameById(int commissionDecisionId);
        Decision GetDecisionById(int commissionDecisionId);
        
        bool Save(CommissionDecision commissionDecision, out string msg);
    }

    public class CommissionServiceFilter
    {
        public bool IsActive;
    }
}
