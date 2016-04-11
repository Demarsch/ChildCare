using Core.Data;
using Core.Data.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatisticsModule.Services
{
    public interface IStatisticsService
    {
        IDisposableQueryable<FinancingSource> GetActualFinancingSources();
    }
}
