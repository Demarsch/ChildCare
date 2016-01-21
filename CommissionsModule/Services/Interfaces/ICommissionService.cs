using Core.Data;
using Core.Data.Misc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CommissionsModule.Services
{
    public interface ICommissionService
    {
        IDisposableQueryable<CommissionFilter> GetCommissionFilters(string options = "");

        bool IsCommissionFilterHasDate(int id);

        IDisposableQueryable<CommissionProtocol> GetCommissionProtocols(int filterId, DateTime? date, bool onlyMyCommissions = false);
    }
}
