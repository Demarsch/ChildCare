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
using Core.Services;

namespace StatisticsModule.Services
{
    public class StatisticsService : IStatisticsService
    {
        private readonly IDbContextProvider contextProvider;

        private readonly ICacheService cacheService;

        public StatisticsService(IDbContextProvider contextProvider, ICacheService cacheService)
        {
            if (contextProvider == null)
            {
                throw new ArgumentNullException("contextProvider");
            }
            if (cacheService == null)
            {
                throw new ArgumentNullException("cacheService");
            }
            this.contextProvider = contextProvider;
            this.cacheService = cacheService;
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

        public IDisposableQueryable<Record> GetRecords(DateTime beginDate, DateTime endDate, int finSourceId, string codeOKATO, bool isCompleted, bool isInProgress, bool isAmbulatory, bool isStationary, bool isDayStationary, int employeeId)
        {
            var context = contextProvider.CreateNewContext();
            var query = context.Set<Record>().Where(x => (finSourceId == -1 || x.RecordContract.FinancingSourceId == finSourceId) &&
                                                        DbFunctions.TruncateTime(x.BeginDateTime) <= DbFunctions.TruncateTime(endDate) &&
                                                        DbFunctions.TruncateTime(x.EndDateTime) >= DbFunctions.TruncateTime(beginDate) &&
                                                        ((isInProgress && x.IsCompleted == false) || (isCompleted && x.IsCompleted == true)) &&
                                                        x.Visit.OKATO.StartsWith(codeOKATO) &&
                                                        (employeeId == -1 || x.RecordMembers.Any(a => a.PersonStaff.PersonId == employeeId)) &&
                                                        ((isAmbulatory && x.ExecutionPlace.Options.Contains(OptionValues.Ambulatory)) || (isStationary && x.ExecutionPlace.Options.Contains(OptionValues.Stationary)) || (isDayStationary && x.ExecutionPlace.Options.Contains(OptionValues.DayStationary)))
                                                        );            
            return new DisposableQueryable<Record>(query, context);                                            
        }

        public IDisposableQueryable<Visit> GetVisits(DateTime beginDate, DateTime endDate, int finSourceId, string codeOKATO, bool isCompleted, bool isInProgress, bool isAmbulatory, bool isPlanned, bool isStationary, bool isDayStationary)
        {
            var context = contextProvider.CreateNewContext();
            var query = context.Set<Visit>().Where(x => (finSourceId == -1 || x.VisitTemplate.FinancingSourceId == finSourceId) &&
                                                        ((isCompleted && DbFunctions.TruncateTime(x.BeginDateTime) <= DbFunctions.TruncateTime(endDate) && DbFunctions.TruncateTime(x.EndDateTime) >= DbFunctions.TruncateTime(beginDate)) || ((isPlanned || isInProgress) && DbFunctions.TruncateTime(x.BeginDateTime) >= DbFunctions.TruncateTime(beginDate) && DbFunctions.TruncateTime(x.BeginDateTime) <= DbFunctions.TruncateTime(endDate))) &&
                                                        ((isPlanned && !x.IsCompleted.HasValue) || (isInProgress && x.IsCompleted == false) || (isCompleted && x.IsCompleted == true)) &&
                                                        x.OKATO.StartsWith(codeOKATO) &&
                                                        ((isAmbulatory && x.ExecutionPlace.Options.Contains(OptionValues.Ambulatory)) || (isStationary && x.ExecutionPlace.Options.Contains(OptionValues.Stationary)) || (isDayStationary && x.ExecutionPlace.Options.Contains(OptionValues.DayStationary)))
                                                        );
            return new DisposableQueryable<Visit>(query, context);
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

        public IEnumerable<MKBGroup> GetMKBGroups()
        {
            return cacheService.GetItems<MKBGroup>().Where(x => x.ParentId == null)
                .Select(CopyMKBGroup)
                .Where(x => x != null)
                .ToArray();
        }

        private MKBGroup CopyMKBGroup(MKBGroup group)
        {
            var result = (MKBGroup)group.Clone();
            result.MKBGroups1 = group.MKBGroups1.ToList();
            foreach (var childMKBGroup in result.MKBGroups1)
                childMKBGroup.MKBGroup1 = result;
            return result;           
        }

        static Dictionary<string, Dictionary<string, bool>> mkbfilterCache = new Dictionary<string, Dictionary<string, bool>>();
        public bool MKBFilter(string filter, string mkb)
        {
            if (!mkbfilterCache.ContainsKey(filter))
            {
                if (mkbfilterCache.Count > 300)
                    mkbfilterCache.Remove(mkbfilterCache.Keys.Last());
                mkbfilterCache.Add(filter, new Dictionary<string, bool>());
            }
            else if (mkbfilterCache[filter].ContainsKey(mkb))
                return mkbfilterCache[filter][mkb];

            var m = mkb.Trim().ToUpper();
            if (m.Length == 0) return false;
            foreach (var s in filter.ToUpper().Split(',', ';'))
            {
                if (m.Contains(s.Trim()))
                {
                    mkbfilterCache[filter].Add(m, true);
                    return true;
                }
                var i = s.IndexOf('-');
                if (i > 0 && m.CompareTo(s.Substring(0, i).Trim()) >= 0)
                {
                    var e = s.Substring(i + 1).Trim();
                    if (m.Substring(0, Math.Min(e.Length, m.Length)).CompareTo(e) <= 0)
                    {
                        mkbfilterCache[filter].Add(m, true);
                        return true;
                    }
                }
            }
            mkbfilterCache[filter].Add(m, false);
            return false;
        }

    }
}
