using Core.Data;
using Core.Data.Misc;
using Core.Data.Services;
using StatisticsModule.DTO;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Extensions;

namespace StatisticsModule.Services
{
    public class StatisticsService : IStatisticsService
    {
        private readonly IDbContextProvider contextProvider;

        public StatisticsService(IDbContextProvider contextProvider)
        {
            if (contextProvider == null)
            {
                throw new ArgumentNullException("contextProvider");
            }
            this.contextProvider = contextProvider;
        }

        public IDisposableQueryable<FinancingSource> GetActualFinancingSources()
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<FinancingSource>(context.Set<FinancingSource>().AsNoTracking().Where(x => x.IsActive), context);
        }


        public IDisposableQueryable<Record> GetRecords(DateTime beginDate, DateTime endDate, int selectedFinSourceId, bool isCompleted, bool isInProgress, bool isAmbulatory, bool isStationary, bool isDayStationary)
        {
            var context = contextProvider.CreateNewContext();
            var query = context.Set<Record>().Where(x => (selectedFinSourceId != -1 ? x.RecordContract.FinancingSourceId == selectedFinSourceId : true) &&
                                                        DbFunctions.TruncateTime(x.BeginDateTime) <= DbFunctions.TruncateTime(endDate) &&
                                                        DbFunctions.TruncateTime(x.EndDateTime) >= DbFunctions.TruncateTime(beginDate) &&
                                                        ((isInProgress && x.IsCompleted == false) || (isCompleted && x.IsCompleted == true)) &&
                                                        ((isAmbulatory && x.ExecutionPlace.Options.Contains(OptionValues.Ambulatory)) || (isStationary && x.ExecutionPlace.Options.Contains(OptionValues.Stationary)) || (isDayStationary && x.ExecutionPlace.Options.Contains(OptionValues.DayStationary)))
                                                        );
            
            return new DisposableQueryable<Record>(query, context);
                                            
        }


        public IDisposableQueryable<Assignment> GetAssignments(DateTime beginDate, DateTime endDate, int selectedFinSourceId, bool isAmbulatory, bool isStationary, bool isDayStationary)
        {
            var context = contextProvider.CreateNewContext();
            var query = context.Set<Assignment>().Where(x => (selectedFinSourceId != -1 ? x.FinancingSourceId == selectedFinSourceId : true) &&
                                                        DbFunctions.TruncateTime(beginDate) <= DbFunctions.TruncateTime(x.AssignDateTime) &&
                                                        DbFunctions.TruncateTime(endDate) >= DbFunctions.TruncateTime(x.AssignDateTime) &&
                                                        !x.RecordId.HasValue &&
                                                        ((isAmbulatory && x.ExecutionPlace.Options.Contains(OptionValues.Ambulatory)) || (isStationary && x.ExecutionPlace.Options.Contains(OptionValues.Stationary)) || (isDayStationary && x.ExecutionPlace.Options.Contains(OptionValues.DayStationary)))
                                                        );

            return new DisposableQueryable<Assignment>(query, context);
        }

        public double GetRecordTypeCost(int recordTypeId, int financingSourceId, DateTime onDate, bool? isChild = null, bool isIncome = true)
        {
            using (var context = contextProvider.CreateNewContext())
            {
                var cost = 0.0;
                var recordType = context.Set<RecordType>().FirstOrDefault(x => x.Id == recordTypeId);
                if (recordType == null) return cost;

                var recordCost = context.Set<RecordTypeCost>()
                                        .Where(x => x.RecordTypeId == recordTypeId && x.FinancingSourceId == financingSourceId &&
                                                    DbFunctions.TruncateTime(onDate) >= DbFunctions.TruncateTime(x.BeginDate) && DbFunctions.TruncateTime(onDate) < DbFunctions.TruncateTime(x.EndDate) &&
                                                    x.IsIncome == isIncome)
                                        .OrderByDescending(x => x.InDateTime)
                                        .Select(x => new { x.FullPrice, x.Profitability, x.IsChild })
                                        .FirstOrDefault(x => (x.IsChild != null ? x.IsChild == isChild : true));
                if (recordCost != null)
                    cost = recordCost.FullPrice * recordCost.Profitability;
                else if (recordType.IsAnalyse)
                {
                    foreach (var parameter in recordType.RecordTypes1)
                    {
                        var parameterCost = recordType.RecordTypeCosts
                                                    .Where(x => x.FinancingSourceId == financingSourceId &&
                                                                DbFunctions.TruncateTime(onDate) >= DbFunctions.TruncateTime(x.BeginDate) && DbFunctions.TruncateTime(onDate) < DbFunctions.TruncateTime(x.EndDate) &&
                                                                x.IsIncome == isIncome)
                                                    .OrderByDescending(x => x.InDateTime)
                                                    .Select(x => new { x.FullPrice, x.Profitability, x.IsChild })
                                                    .FirstOrDefault(x => (x.IsChild != null ? x.IsChild == isChild : true));
                        if (parameterCost != null)
                            cost += parameterCost.FullPrice * parameterCost.Profitability;
                    }
                }
                return cost;
            }
        }
    }
}
