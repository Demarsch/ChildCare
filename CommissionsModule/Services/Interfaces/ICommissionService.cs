using Core.Data;
using Core.Data.Misc;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace CommissionsModule.Services
{
    public interface ICommissionService
    {
        IDisposableQueryable<CommissionFilter> GetCommissionFilters(string options = "");

        bool IsCommissionFilterHasDate(int id);

        IDisposableQueryable<CommissionProtocol> GetCommissionProtocols(int filterId, DateTime? date = null, bool onlyMyCommissions = false);

        IDisposableQueryable<CommissionDecision> GetCommissionDecisions(int commissionProtocolId);

        string GetDecisionColorHex(int? decisionId);
    }
}
