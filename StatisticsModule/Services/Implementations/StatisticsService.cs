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

        public IDisposableQueryable<PersonStaff> GetPersonStaffs()
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<PersonStaff>(context.Set<PersonStaff>().OrderBy(x => x.Person.ShortName), context);
        }

        public IDisposableQueryable<Record> GetRecords(DateTime beginDate, DateTime endDate, int finSourceId, bool isCompleted, bool isInProgress, bool isAmbulatory, bool isStationary, bool isDayStationary, int employeeId)
        {
            var context = contextProvider.CreateNewContext();
            var query = context.Set<Record>().Where(x => (finSourceId == -1 || x.RecordContract.FinancingSourceId == finSourceId) &&
                                                        DbFunctions.TruncateTime(x.BeginDateTime) <= DbFunctions.TruncateTime(endDate) &&
                                                        DbFunctions.TruncateTime(x.EndDateTime) >= DbFunctions.TruncateTime(beginDate) &&
                                                        ((isInProgress && x.IsCompleted == false) || (isCompleted && x.IsCompleted == true)) &&
                                                        (employeeId == -1 || x.RecordMembers.Any(a => a.PersonStaff.PersonId == employeeId)) &&
                                                        ((isAmbulatory && x.ExecutionPlace.Options.Contains(OptionValues.Ambulatory)) || (isStationary && x.ExecutionPlace.Options.Contains(OptionValues.Stationary)) || (isDayStationary && x.ExecutionPlace.Options.Contains(OptionValues.DayStationary)))
                                                        );
            
            return new DisposableQueryable<Record>(query, context);
                                            
        }


        public IDisposableQueryable<Assignment> GetAssignments(DateTime beginDate, DateTime endDate, int finSourceId, bool isAmbulatory, bool isStationary, bool isDayStationary)
        {
            var context = contextProvider.CreateNewContext();
            var query = context.Set<Assignment>().Where(x => (finSourceId == -1 || x.FinancingSourceId == finSourceId) &&
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
                var date = onDate.Date;
                var recordTypePrices = context.Set<RecordType>().Where(x => x.Id == recordTypeId || x.ParentId == recordTypeId)
                                                                .Select(x => new
                                                                {  
                                                                    x.Id,
                                                                    //x.IsAnalyse,
                                                                    Cost = x.RecordTypeCosts.Where(a => a.FinancingSourceId == financingSourceId &&
                                                                                                        date >= DbFunctions.TruncateTime(a.BeginDate) &&
                                                                                                        date < DbFunctions.TruncateTime(a.EndDate) &&
                                                                                                        a.IsIncome == isIncome &&
                                                                                                        (isChild == null || isChild == a.IsChild))
                                                                                            .OrderByDescending(a => a.InDateTime)
                                                                                            .Select(a => new { FullCost = a.FullPrice * a.Profitability })
                                                                                            .FirstOrDefault()
                                                                }).ToArray();
                if (!recordTypePrices.Any()) 
                    return cost;

                var recordTypePrice = recordTypePrices.FirstOrDefault(x => x.Id == recordTypeId);
                if (recordTypePrice != null && recordTypePrice.Cost != null)
                    return recordTypePrice.Cost.FullCost;

                if (recordTypePrices.Any(x => x.Cost != null))
                    return recordTypePrices.Where(x => x.Cost != null).Select(x => x.Cost.FullCost).Sum();
               
                return cost;
            }
        }
    }
}
